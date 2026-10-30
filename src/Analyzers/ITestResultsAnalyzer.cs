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

    /// <summary>
    /// The TRX files in the directory and its subdirectories.
    /// </summary>
    IFileInfo[] GetTrxFiles(IDirectoryInfo resultsDirectory);
}