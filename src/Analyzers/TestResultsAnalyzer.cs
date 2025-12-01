using System.IO.Abstractions;
using System.Text.RegularExpressions;
using dotnet.test.rerun.Domain;
using dotnet.test.rerun.Extensions;
using dotnet.test.rerun.Logging;
using TrxFileParser;

namespace dotnet.test.rerun.Analyzers;

using TrxFileParser.Models;

public class TestResultsAnalyzer : ITestResultsAnalyzer
{
    private readonly ILogger Log;
    public HashSet<string> reportFiles;

    public TestResultsAnalyzer(ILogger logger)
    {
        Log = logger;
        reportFiles = new();
    }

    public TestFilterCollection GetFailedTestsFilter(IFileInfo[] trxFiles)
    {
        var allFailedTests = new TestFilterCollection();
        foreach (var trxFile in trxFiles)
        {
            var failedTests = GetFailedTestsFilter(trxFile);
            allFailedTests.Add(failedTests);
        }

        return allFailedTests;
    }

    public IFileInfo[] GetTrxFiles(IDirectoryInfo resultsDirectory, DateTime startSearchTime)
        => resultsDirectory.Exists
            ? resultsDirectory.EnumerateFiles("*.trx").Where(file => file.CreationTime >= startSearchTime).ToArray()
            : [];

    public void AddLastTrxFiles(IDirectoryInfo resultsDirectory, DateTime startSearchTime)
    {
        foreach (var fileInfo in GetTrxFiles(resultsDirectory, startSearchTime))
            reportFiles.Add(fileInfo.FullName);
    }

    public HashSet<string> GetReportFiles()
        => reportFiles;

    private TestFilter GetFailedTestsFilter(IFileInfo trxFile)
    {
        const string outcome = "Failed";
        var trx = TrxDeserializer.Deserialize(trxFile.FullName);
        reportFiles.Add(trxFile.FullName);

        var fullMethodNameByTestId = trx.TestDefinitions?.UnitTests
            .ToDictionary(x => x.Id, x => $"{x.TestMethod.ClassName}.{x.TestMethod.Name}") ?? new();
        
        var classNameByTestId = trx.TestDefinitions?.UnitTests
            .ToDictionary(x => x.Id, x => x.TestMethod.ClassName) ?? new();
        
        var adapterTypeByTestId = trx.TestDefinitions?.UnitTests
            .ToDictionary(x => x.Id, x => x.TestMethod.AdapterTypeName) ?? new();

        string? framework = trx.TestDefinitions?.UnitTests.Select(test => test.Storage.FetchDotNetVersion())
            .FirstOrDefault(val => string.IsNullOrWhiteSpace(val) is false);

        var failedTests = trx.Results?.UnitTestResults
            .Where(t => t.Outcome.Equals(outcome, StringComparison.InvariantCultureIgnoreCase))
            .ToList() ?? [];

        if (failedTests.Count == 0)
            Log.Warning($"No tests found with the Outcome {outcome} in file {trxFile.Name}");

        var filters = failedTests
            .Select(t =>
            {
                var adapterType = adapterTypeByTestId.TryGetValue(t.TestId, out var adapter) ? adapter : string.Empty;
                var isNUnit = adapterType.Contains("nunit", StringComparison.OrdinalIgnoreCase);
                var isMSTest = adapterType.Contains("mstest", StringComparison.OrdinalIgnoreCase);
                // Only NUnit supports filtering by specific test cases with parameters
                var useContainsOperator = isNUnit;
                
                return BuildFilters([
                    BuildFullyQualifiedNameFilter(t, fullMethodNameByTestId, useContainsOperator, false, string.Empty),
                    // MSTest and NUnit don't use DisplayName filters (NUnit uses ~ operator, MSTest doesn't support case-level filtering)
                    (useContainsOperator || isMSTest) ? null : BuildDisplayNameFilter(t.TestName)]);
            })
            .Distinct()
            .ToList();

        return new(framework, filters);
    }

    private string BuildFilters(string?[] filters)
        => EscapeAll(
            string.Join(
                "&",
                filters.Where(x => x is not null)));

    private static string BuildFullyQualifiedNameFilter(
        UnitTestResult testResult,
        Dictionary<string, string> fullMethodNameByTestId,
        bool useContainsOperator,
        bool isMSTest,
        string className)
    {
        var fullyQualifiedName = fullMethodNameByTestId.TryGetValue(testResult.TestId, out var fullMethodName)
            ? fullMethodName
            : testResult.TestName;

        // Check if the test has parameters
        var openParenIndex = fullyQualifiedName.IndexOf('(');
        var closeParenIndex = fullyQualifiedName.IndexOf(')');
        var hasParameters = openParenIndex > 0 && closeParenIndex > openParenIndex + 1; // +1 to check for non-empty params
        
        // For NUnit with parameterized tests, use the ~ (contains) operator with full test name including parameters
        if (useContainsOperator && hasParameters)
        {
            // Remove quotes from string parameters to avoid command-line parsing issues
            // NUnit test names with string parameters include quotes: MethodName("param1","param2")
            // The ~ operator matches substrings, so MethodName(param1,param2) will still match
            var filterValue = fullyQualifiedName.Replace("\"", "");
            return $"FullyQualifiedName~{filterValue}";
        }
        
        // For other frameworks or non-parameterized tests, use exact match with method name only
        var fqn = fullyQualifiedName.Split('(')[0];
        // Escape spaces in the fully qualified name
        fqn = fqn.Replace(" ", "\\ ");
        return $"FullyQualifiedName={fqn}";
    }

    private static string? BuildDisplayNameFilter(string testName)
    {
        var openParenthesisIndex = testName.IndexOf("(");
        var closeParenthesisIndex = testName.IndexOf(")");
        
        // Check if there are parameters in the test name
        if (openParenthesisIndex == -1 || closeParenthesisIndex == -1 || closeParenthesisIndex <= openParenthesisIndex)
        {
            return null;
        }

        var testParameters = testName
            .Substring(openParenthesisIndex + 1, closeParenthesisIndex - openParenthesisIndex - 1);
        
        // If there are no parameters, return null
        if (string.IsNullOrWhiteSpace(testParameters))
        {
            return null;
        }

        // Escape double quotes for filter compatibility
        testParameters = testParameters.Replace("\"", "%22");

        return $"DisplayName~{testParameters}";
    }

    private string EscapeAll(string input)
        => Regex.Replace(input, @"[()?{}]", match => "\\" + match.Value);
}
