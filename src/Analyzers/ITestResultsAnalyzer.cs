using System.IO.Abstractions;
using dotnet.test.rerun.Domain;

namespace dotnet.test.rerun.Analyzers;

public interface ITestResultsAnalyzer
{
    TestFilterCollection GetFailedTestsFilter(IFileInfo[] trxFiles);

    IFileInfo[] GetTrxFiles(IDirectoryInfo resultsDirectory, DateTime startSearchTime);

    void AddLastTrxFiles(IDirectoryInfo resultsDirectory, DateTime startSearchTime);

    HashSet<string> GetReportFiles();
}