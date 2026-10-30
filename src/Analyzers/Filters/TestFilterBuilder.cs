namespace dotnet.test.rerun.Analyzers.Filters;

/// <summary>
/// Picks the filter strategy of each failed test's framework. To support a new framework,
/// add an <see cref="ITestFilterStrategy"/> before the default one.
/// </summary>
public class TestFilterBuilder
{
    private readonly IReadOnlyList<ITestFilterStrategy> Strategies = new ITestFilterStrategy[]
    {
        new NUnitFilterStrategy(),
        new MSTestFilterStrategy(),
        new DefaultFilterStrategy()
    };

    public string BuildFilter(FailedTest test)
        => Strategies.First(strategy => strategy.CanHandle(test.AdapterTypeName)).BuildFilter(test);
}
