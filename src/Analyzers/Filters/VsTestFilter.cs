using System.Text.RegularExpressions;

namespace dotnet.test.rerun.Analyzers.Filters;

/// <summary>
/// Escaping rules of dotnet test --filter values.
/// </summary>
public static class VsTestFilter
{
    /// <summary>
    /// Backslash-escapes the characters that have a meaning in the filter syntax.
    /// </summary>
    public static string Escape(string value)
        => Regex.Replace(value, @"[()?{}]", match => "\\" + match.Value);

    /// <summary>
    /// A literal quote breaks the "dotnet test" command line: the filter is forwarded to MSBuild as the
    /// VSTestTestCaseFilter property, whose parser cannot handle raw quotes. MSBuild decodes %22 back to a
    /// literal quote before invoking VSTest, so the filter that VSTest evaluates still contains the quotes.
    /// </summary>
    public static string EncodeQuotes(string value)
        => value.Replace("\"", "%22");

    /// <summary>
    /// Exact match on the method, without parameters. Spaces are escaped.
    /// </summary>
    public static string FullyQualifiedNameEquals(string fullyQualifiedName)
        => $"FullyQualifiedName={MethodName(fullyQualifiedName).Replace(" ", "\\ ")}";

    /// <summary>
    /// Matches every test of a class, used when a test run was aborted.
    /// </summary>
    public static string ClassFilter(string className)
        => $"FullyQualifiedName~{Escape(className)}";

    public static string MethodName(string fullyQualifiedName)
        => fullyQualifiedName.Split('(')[0];

    /// <summary>
    /// The text between the first parentheses, or null when there are no parameters.
    /// </summary>
    public static string? Parameters(string name)
    {
        var open = name.IndexOf('(');
        var close = name.IndexOf(')');
        if (open == -1 || close == -1 || close <= open)
            return null;

        var parameters = name.Substring(open + 1, close - open - 1);
        return string.IsNullOrWhiteSpace(parameters) ? null : parameters;
    }
}
