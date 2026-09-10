using System.IO.Abstractions;
using dotnet.test.rerun.Domain;

namespace dotnet.test.rerun.Analyzers;

public interface ITestResultsAnalyzer
{
    TestFilterCollection GetFailedTestsFilter(IFileInfo[] trxFiles);

    /// <summary>
    /// Logs the result of each individual test found in the given trx files,
    /// independently of the dotnet test verbosity.
    /// </summary>
    /// <param name="trxFiles">The trx files to read the test results from.</param>
    void LogTestResults(IFileInfo[] trxFiles);

    IFileInfo[] GetTrxFiles(IDirectoryInfo resultsDirectory, DateTime startSearchTime);

    void AddLastTrxFiles(IDirectoryInfo resultsDirectory, DateTime startSearchTime);

    HashSet<string> GetReportFiles();
}