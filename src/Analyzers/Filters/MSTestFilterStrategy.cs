namespace dotnet.test.rerun.Analyzers.Filters;

/// <summary>
/// MSTest does not support selecting a single data row, so the whole test method is rerun.
/// </summary>
public class MSTestFilterStrategy : ITestFilterStrategy
{
    public bool CanHandle(string adapterTypeName)
        => adapterTypeName.Contains("mstest", StringComparison.OrdinalIgnoreCase);

    public string BuildFilter(FailedTest test)
        => VsTestFilter.Escape(VsTestFilter.FullyQualifiedNameEquals(test.FullyQualifiedName));
}
