using System.IO.Abstractions;
using dotnet.test.rerun.Domain;

namespace dotnet.test.rerun.Analyzers;

public interface ITestResultsAnalyzer
{
    TestFilterCollection GetFailedTestsFilter(IFileInfo[] trxFiles);
    
    TestFilterCollection GetFailedTestsFilter(IFileInfo[] trxFiles, string? consoleOutput);

    IFileInfo[] GetTrxFiles(IDirectoryInfo resultsDirectory, DateTime startSearchTime);

    void AddLastTrxFiles(IDirectoryInfo resultsDirectory, DateTime startSearchTime);

    HashSet<string> GetReportFiles();
}