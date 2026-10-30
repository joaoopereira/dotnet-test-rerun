namespace dotnet.test.rerun.Analyzers.Filters;

/// <summary>
/// Builds the dotnet test filter clause that selects one failed test, for one test framework.
/// </summary>
public interface ITestFilterStrategy
{
    bool CanHandle(string adapterTypeName);

    string BuildFilter(FailedTest test);
}
