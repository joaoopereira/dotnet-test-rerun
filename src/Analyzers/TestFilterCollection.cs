using System.Collections;

namespace dotnet.test.rerun.Analyzers
{
    public class TestFilterCollection : IEnumerable<TestFilter>
    {
        private Dictionary<string, TestFilter> filters = new();

        public bool HasTestsToReRun => filters.Any();

        public IEnumerator GetEnumerator()
        {
            return filters.Values.GetEnumerator();
        }

        internal void Add(TestFilter testFilter)
        {
            if(testFilter is null)
            {
                return;
            }
            TestFilter existingTestFilter = TestFilter.NoFramework;
            if (filters.ContainsKey(testFilter.Framework))
            {
                existingTestFilter = filters[testFilter.Framework];
            }
            existingTestFilter.Merge(testFilter);
            filters[existingTestFilter.Framework] = existingTestFilter;
        }

        IEnumerator<TestFilter> IEnumerable<TestFilter>.GetEnumerator()
        {
            return filters.Values.GetEnumerator();
        }
    }
}