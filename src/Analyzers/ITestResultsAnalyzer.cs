using System.IO.Abstractions;

namespace dotnet.test.rerun.Analyzers;

public interface ITestResultsAnalyzer
{
    string GetFailedTestsFilter(IFileInfo trxFile);

    IFileInfo? GetTrxFile(IDirectoryInfo resultsDirectory);

    void AddLastTrxFile(IDirectoryInfo resultsDirectory);

    HashSet<string> GetReportFiles();
}