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
    
    public string GetFailedTestsFilter(IFileInfo trxFile)
    {
        const string outcome = "Failed";
        var trx = TrxDeserializer.Deserialize(trxFile.FullName);
        reportFiles.Add(trxFile.FullName);
        
        var tests = trx.Results.UnitTestResults
            .Where(t => t.Outcome.Equals(outcome, StringComparison.InvariantCultureIgnoreCase))
            .Select(t => $"FullyQualifiedName~{(t.TestName.IndexOf("(") > 0 ? t.TestName.Substring(0, t.TestName.IndexOf("(")) : t.TestName).TrimEnd()}")
            .Distinct()
            .ToList();

        if (tests.Count == 0)
        {
            Log.Warning($"No tests found with the Outcome {outcome}");
            return "";
        }

        var testFilter = string.Join(" | ", tests);
        Log.Debug(testFilter);
        return testFilter;
    }
    
    public IFileInfo? GetTrxFile(IDirectoryInfo resultsDirectory)
        => resultsDirectory.EnumerateFiles("*.trx").MaxBy(f => f.Name);

    public void AddLastTrxFile(IDirectoryInfo resultsDirectory)
    {
        var fileInfo = GetTrxFile(resultsDirectory);

        if (fileInfo is not null)
            reportFiles.Add(fileInfo.FullName);
    }
    
    public HashSet<string> GetReportFiles()
        => reportFiles;
}