namespace dotnet.test.rerun.Analyzers.Filters;

/// <summary>
/// NUnit includes the parameters in the FullyQualifiedName, so a parameterised test case is selected with
/// the ~ (contains) operator on the full name, quotes included.
/// </summary>
public class NUnitFilterStrategy : ITestFilterStrategy
{
    public bool CanHandle(string adapterTypeName)
        => adapterTypeName.Contains("nunit", StringComparison.OrdinalIgnoreCase);

    public string BuildFilter(FailedTest test)
        => VsTestFilter.Escape(HasParameters(test.FullyQualifiedName)
            ? $"FullyQualifiedName~{VsTestFilter.EncodeQuotes(test.FullyQualifiedName)}"
            : VsTestFilter.FullyQualifiedNameEquals(test.FullyQualifiedName));

    private static bool HasParameters(string fullyQualifiedName)
    {
        var open = fullyQualifiedName.IndexOf('(');
        var close = fullyQualifiedName.IndexOf(')');
        return open > 0 && close > open + 1;
    }
}
