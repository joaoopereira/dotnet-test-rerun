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
            : Array.Empty<IFileInfo>();

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

        string? framework = trx.TestDefinitions?.UnitTests.Select(test => test.Storage.FetchDotNetVersion())
            .FirstOrDefault(val => string.IsNullOrWhiteSpace(val) is false);

        var failedTests = trx.Results?.UnitTestResults
            .Where(t => t.Outcome.Equals(outcome, StringComparison.InvariantCultureIgnoreCase))
            .ToList() ?? [];

        if (failedTests.Count == 0)
            Log.Warning($"No tests found with the Outcome {outcome} in file {trxFile.Name}");

        var filters = failedTests
            .Select(t => BuildFilters([
                BuildFullyQualifiedNameFilter(t, fullMethodNameByTestId),
                BuildDisplayNameFilter(t.TestName)]))
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
        Dictionary<string, string> fullMethodNameByTestId)
    {
        var fullyQualifiedName = fullMethodNameByTestId.TryGetValue(testResult.TestId, out var fullMethodName)
            ? fullMethodName
            : testResult.TestName;

        return $"FullyQualifiedName~{fullyQualifiedName.Split('(')[0]}";
    }

    private static string? BuildDisplayNameFilter(string testName)
    {
        var openParenthesisIndex = testName.IndexOf("(");
        var closeParenthesisIndex = testName.IndexOf(")");
        var canRunSpecific = testName.IndexOf(":") > 0;
        if (openParenthesisIndex == -1 || !canRunSpecific)
        {
            return null;
        }

        var testParameters = testName
            .Substring(openParenthesisIndex + 1, closeParenthesisIndex - openParenthesisIndex - 1)
            .Replace("\"", "%22");

        return $"DisplayName~{testParameters}";
    }

    private string EscapeAll(string input)
        => Regex.Replace(input, @"[()?{}]", match => "\\" + match.Value);
}
