using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: TestCaseOrderer("IntegrationTests.AlphabeticalOrderer", "IntegrationTests")]
[assembly: TestCollectionOrderer("IntegrationTests.AlphabeticalCollectionOrderer", "IntegrationTests")]
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace IntegrationTests
{
    public class AlphabeticalOrderer : ITestCaseOrderer
    {
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
        {
            var order = testCases.OrderBy(tc => tc.TestMethod.Method.Name);

            return order;
        }
    }

    public class AlphabeticalCollectionOrderer : ITestCollectionOrderer
    {
        public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> testCollections)
        {
            return testCollections.OrderBy(tc => tc.DisplayName);
        }
    }
}