namespace dotnet.test.rerun.Analyzers
{
    internal class TestFilter
    {
        private const string NoFrameworkName = "NoFramework";
        public TestFilter(string? framework, List<string> tests)
        {
            Framework = framework ?? NoFrameworkName;
            Tests = tests;
        }

        public static TestFilter NoFramework = new TestFilter(NoFrameworkName, new List<string>());
        public string Framework { get; }
        public List<string> Tests { get; }
        public string Filter => string.Join(" | ", Tests);

        internal void Merge(TestFilter testFilter)
        {
            Tests.AddRange(testFilter.Tests);
        }
    }
}