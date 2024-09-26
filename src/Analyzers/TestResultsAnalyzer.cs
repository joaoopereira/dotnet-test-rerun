using System.IO.Abstractions;
using System.Text;
using System.Text.RegularExpressions;
using dotnet.test.rerun.Domain;
using dotnet.test.rerun.Extensions;
using dotnet.test.rerun.Logging;
using TrxFileParser;

namespace dotnet.test.rerun.Analyzers;

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

        Dictionary<string, string> testClassByTestId = new Dictionary<string, string>();
        trx.TestDefinitions?.UnitTests.ForEach(t => testClassByTestId[t.Id] = t.TestMethod.ClassName);
        string? framework = trx.TestDefinitions?.UnitTests.Select(test => test.Storage.FetchDotNetVersion())
            .FirstOrDefault(val => string.IsNullOrWhiteSpace(val) is false);

        var tests = trx.Results?.UnitTestResults
            .Where(t => t.Outcome.Equals(outcome, StringComparison.InvariantCultureIgnoreCase))
            .Select(t => EscapeAll(new StringBuilder()
                .Append($"FullyQualifiedName~")
                .Append(
                    $"{(testClassByTestId.ContainsKey(t.TestId) && t.TestName.Contains(testClassByTestId[t.TestId]) is false ? $"{testClassByTestId[t.TestId]}." : string.Empty)}")
                .Append(GetTestName(t.TestName)).ToString()))
            .Distinct()
            .ToList() ?? new List<string>();

        if (tests.Count == 0)
            Log.Warning($"No tests found with the Outcome {outcome} in file {trxFile.Name}");

        return new(framework, tests);
    }

    private string GetTestName(string testName)
    {
        var openParenthesisIndex = testName.IndexOf("(");
        var closeParenthesisIndex = testName.IndexOf(")");
        var canRunSpecific = testName.IndexOf(":") > 0;
        if (openParenthesisIndex == -1)
            return testName.TrimEnd();

        var testToRerun = new StringBuilder().Append(testName.Substring(0, openParenthesisIndex).TrimEnd());
        if (canRunSpecific)
            testToRerun
                .Append("&DisplayName~")
                .Append(testName.Substring(openParenthesisIndex + 1, closeParenthesisIndex - openParenthesisIndex - 1))
                .Replace("\"", "%22");

        return testToRerun.ToString().TrimEnd();
    }

    private string EscapeAll(string input)
        => Regex.Replace(input, @"[()?{}]", match => "\\" + match.Value);
}