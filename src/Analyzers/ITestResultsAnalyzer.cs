using System.IO.Abstractions;

namespace dotnet.test.rerun.Analyzers;

public interface ITestResultsAnalyzer
{
    string GetFailedTestsFilter(IFileInfo[] trxFiles);

    IFileInfo[] GetTrxFiles(IDirectoryInfo resultsDirectory, DateTime startSearchTime);

    void AddLastTrxFiles(IDirectoryInfo resultsDirectory, DateTime startSearchTime);

    HashSet<string> GetReportFiles();
}