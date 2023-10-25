namespace dotnet.test.rerun.Domain;

public class TestFilter
{
    public const string NoFrameworkName = "NoFramework";
    public TestFilter(string? framework, List<string> tests)
    {
        Framework = string.IsNullOrWhiteSpace(framework) ? NoFrameworkName : framework;
        Tests = tests;
    }

    public string Framework { get; }
    public List<string> Tests { get; }
    public string Filter => string.Join(" | ", Tests);

    internal void Merge(TestFilter testFilter)
    {
        Tests.AddRange(testFilter.Tests);
    }
}