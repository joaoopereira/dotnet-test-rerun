using System.IO.Abstractions;
using dotnet.test.rerun.Analyzers.Filters;
using dotnet.test.rerun.Domain;
using dotnet.test.rerun.Extensions;
using dotnet.test.rerun.Logging;
using TrxFileParser;

namespace dotnet.test.rerun.Analyzers;

using TrxFileParser.Models;

public class TestResultsAnalyzer : ITestResultsAnalyzer
{
    private readonly ILogger Log;
    private readonly TestFilterBuilder FilterBuilder = new();

    public TestResultsAnalyzer(ILogger logger)
    {
        Log = logger;
    }

    public TestFilterCollection GetFailedTestsFilter(IFileInfo[] trxFiles)
    {
        var allFailedTests = new TestFilterCollection();
        foreach (var trxFile in trxFiles)
        {
            var (failedTests, totalTests) = GetFailedTestsFilter(trxFile);
            allFailedTests.Add(failedTests);
            allFailedTests.TotalTests += totalTests;
        }

        return allFailedTests;
    }

    public void LogTestResults(IFileInfo[] trxFiles)
    {
        const string passedOutcome = "Passed";

        foreach (var trxFile in trxFiles)
        {
            var trx = TrxDeserializer.Deserialize(trxFile.FullName);

            var fullMethodNameByTestId = trx.TestDefinitions?.UnitTests
                .ToDictionary(x => x.Id, x => $"{x.TestMethod.ClassName}.{x.TestMethod.Name}") ?? new();

            foreach (var result in trx.Results?.UnitTestResults ?? new List<UnitTestResult>())
            {
                var testName = BuildFullTestName(result, fullMethodNameByTestId);
                var passed = result.Outcome.Equals(passedOutcome, StringComparison.InvariantCultureIgnoreCase);

                Log.TestResult(testName, passed, result.Output?.ErrorInfo?.Message, result.Output?.ErrorInfo?.StackTrace);
            }
        }
    }

    private static string BuildFullTestName(UnitTestResult result, Dictionary<string, string> fullMethodNameByTestId)
    {
        var fullMethodName = fullMethodNameByTestId.TryGetValue(result.TestId, out var name)
            ? name
            : result.TestName;

        // Some frameworks (e.g. NUnit, MSTest) don't include parameters in the test definition's method name,
        // while others (e.g. xUnit) already include the parameters in the result's test name.
        var parametersStartIndex = result.TestName.IndexOf('(');
        return parametersStartIndex >= 0 && fullMethodName.Contains('(') is false
            ? fullMethodName + result.TestName.Substring(parametersStartIndex)
            : fullMethodName;
    }

    public IFileInfo[] GetTrxFiles(IDirectoryInfo resultsDirectory, DateTime startSearchTime)
        => resultsDirectory.Exists
            ? resultsDirectory.EnumerateFiles("*.trx").Where(file => file.CreationTime >= startSearchTime).ToArray()
            : new IFileInfo[0];

    private (TestFilter Filter, int TotalTests) GetFailedTestsFilter(IFileInfo trxFile)
    {
        const string outcome = "Failed";
        var trx = TrxDeserializer.Deserialize(trxFile.FullName);
        var totalTests = trx.ResultSummary?.Counters?.Total ?? 0;

        var fullMethodNameByTestId = trx.TestDefinitions?.UnitTests
            .ToDictionary(x => x.Id, x => $"{x.TestMethod.ClassName}.{x.TestMethod.Name}") ?? new();

        var adapterTypeByTestId = trx.TestDefinitions?.UnitTests
            .ToDictionary(x => x.Id, x => x.TestMethod.AdapterTypeName) ?? new();

        string? framework = trx.TestDefinitions?.UnitTests.Select(test => test.Storage.FetchDotNetVersion())
            .FirstOrDefault(val => string.IsNullOrWhiteSpace(val) is false);

        var failedTests = trx.Results?.UnitTestResults
            .Where(t => t.Outcome.Equals(outcome, StringComparison.InvariantCultureIgnoreCase))
            .ToList() ?? new List<UnitTestResult>();

        // Check if the test run was aborted (e.g., test host crash)
        var wasAborted = trx.ResultSummary?.Outcome != null &&
                        !trx.ResultSummary.Outcome.Equals("Completed", StringComparison.OrdinalIgnoreCase) &&
                        !trx.ResultSummary.Outcome.Equals("Failed", StringComparison.OrdinalIgnoreCase);

        if (failedTests.Count == 0)
        {
            if (wasAborted)
            {
                Log.Warning($"Test run was aborted in file {trxFile.Name}. Will retry all tests from this assembly.");
                // When test run is aborted with no failed tests, rerun all tests from all classes
                var allClassNames = trx.TestDefinitions?.UnitTests
                    .Select(t => t.TestMethod.ClassName)
                    .Distinct()
                    .ToList() ?? new List<string>();

                var abortedFilters = allClassNames
                    .Select(VsTestFilter.ClassFilter)
                    .ToList();

                return (new TestFilter(framework, abortedFilters), totalTests);
            }
            else
            {
                Log.Warning($"No tests found with the Outcome {outcome} in file {trxFile.Name}");
            }
        }

        var filters = failedTests
            .Select(t => FilterBuilder.BuildFilter(new FailedTest(
                fullMethodNameByTestId.TryGetValue(t.TestId, out var fullMethodName) ? fullMethodName : t.TestName,
                t.TestName,
                adapterTypeByTestId.TryGetValue(t.TestId, out var adapter) ? adapter : string.Empty)))
            .Distinct()
            .ToList();

        return (new TestFilter(framework, filters), totalTests);
    }
}
