using System.IO.Abstractions;
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
        reportFiles = new ();
    }

    public string GetFailedTestsFilter(IFileInfo[] trxFiles)
    {
        var failedTests = new List<string>();
        foreach (var trxFile in trxFiles)
            failedTests.AddRange(GetFailedTestsFilter(trxFile));
        
        var testFilter = string.Join(" | ", failedTests);
        return testFilter;
    }
    
    public IFileInfo[] GetTrxFiles(IDirectoryInfo resultsDirectory, DateTime startSearchTime)
        => resultsDirectory.Exists ?
           resultsDirectory.EnumerateFiles("*.trx").Where(file => file.CreationTime >= startSearchTime).ToArray() :
           Array.Empty<IFileInfo>();

    public void AddLastTrxFiles(IDirectoryInfo resultsDirectory, DateTime startSearchTime)
    {
        foreach (var fileInfo in GetTrxFiles(resultsDirectory, startSearchTime))
            reportFiles.Add(fileInfo.FullName);
    }
    
    public HashSet<string> GetReportFiles()
        => reportFiles;

    private List<string> GetFailedTestsFilter(IFileInfo trxFile)
    {
        const string outcome = "Failed";
        var trx = TrxDeserializer.Deserialize(trxFile.FullName);
        reportFiles.Add(trxFile.FullName);

        Dictionary<string, string> testClassByTestId = new Dictionary<string, string>();
        trx.TestDefinitions?.UnitTests.ForEach(t => testClassByTestId[t.Id] = t.TestMethod.ClassName);

        var tests = trx.Results?.UnitTestResults
            .Where(t => t.Outcome.Equals(outcome, StringComparison.InvariantCultureIgnoreCase))
            .Select(t => $"FullyQualifiedName~" +
                         $"{(testClassByTestId.ContainsKey(t.TestId) && t.TestName.Contains(testClassByTestId[t.TestId]) is false ? $"{testClassByTestId[t.TestId]}." : string.Empty)}" +
                         $"{(t.TestName.IndexOf("(") > 0 ? t.TestName.Substring(0, t.TestName.IndexOf("(")) : t.TestName).TrimEnd()}")
            .Distinct()
            .ToList() ?? new List<string>();

        if (tests.Count == 0)
            Log.Warning($"No tests found with the Outcome {outcome} in file {trxFile.Name}");

        return tests;
    }
}