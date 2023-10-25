using System.Collections;

namespace dotnet.test.rerun.Domain;

public class TestFilterCollection
{
    private Dictionary<string, TestFilter> filters = new();

    public bool HasTestsToReRun => filters.Any();
    public Dictionary<string, TestFilter> Filters => filters;

    internal void Add(TestFilter testFilter)
    {
        if (testFilter.Tests.Count == 0)
            return;
        
        TestFilter existingTestFilter = new TestFilter(testFilter.Framework, new List<string>());
        if (filters.ContainsKey(testFilter.Framework))
            existingTestFilter = filters[testFilter.Framework];
        
        existingTestFilter.Merge(testFilter);
        filters[existingTestFilter.Framework] = existingTestFilter;
    }
}