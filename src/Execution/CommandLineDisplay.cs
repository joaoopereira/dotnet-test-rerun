using System.Text.RegularExpressions;

namespace dotnet.test.rerun.Execution;

/// <summary>
/// Renders an argument list as a single line for logs and error messages.
/// </summary>
public static class CommandLineDisplay
{
    private static readonly Regex NeedsQuotes = new(@"[\s""';|&<>()]", RegexOptions.Compiled);

    public static string Render(string fileName, IEnumerable<string> arguments)
        => string.Join(' ', new[] { fileName }.Concat(arguments.Select(Quote)));

    private static string Quote(string argument)
        => argument.Length == 0 || NeedsQuotes.IsMatch(argument)
            ? $"\"{argument.Replace("\"", "\\\"")}\""
            : argument;
}
