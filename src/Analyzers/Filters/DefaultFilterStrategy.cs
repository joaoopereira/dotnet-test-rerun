namespace dotnet.test.rerun.Analyzers.Filters;

/// <summary>
/// xUnit and other frameworks: the FullyQualifiedName has no parameters, so a parameterised test case
/// is selected by also matching its parameters in the DisplayName.
/// </summary>
public class DefaultFilterStrategy : ITestFilterStrategy
{
    public bool CanHandle(string adapterTypeName) => true;

    public string BuildFilter(FailedTest test)
    {
        var fullyQualifiedName = VsTestFilter.FullyQualifiedNameEquals(test.FullyQualifiedName);
        var parameters = VsTestFilter.Parameters(test.TestName);
        return VsTestFilter.Escape(parameters is null
            ? fullyQualifiedName
            : $"{fullyQualifiedName}&DisplayName~{VsTestFilter.EncodeQuotes(parameters)}");
    }
}
