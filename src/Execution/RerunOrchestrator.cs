using System.IO.Abstractions;
using dotnet.test.rerun.Analyzers;
using dotnet.test.rerun.Configuration;
using dotnet.test.rerun.DotNetRunner;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Logging;

namespace dotnet.test.rerun.Execution;

/// <summary>
/// Runs dotnet test, then reruns the failed tests until they pass or the attempts run out.
/// </summary>
public class RerunOrchestrator
{
    private readonly ILogger Log;
    private readonly IDotNetTestRunner DotNetTestRunner;
    private readonly IDotNetCoverageRunner DotNetCoverageRunner;
    private readonly IFileSystem FileSystem;
    private readonly ITestResultsAnalyzer TestResultsAnalyzer;

    public RerunOrchestrator(ILogger logger,
        IDotNetTestRunner dotNetTestRunner,
        IDotNetCoverageRunner dotNetCoverageRunner,
        IFileSystem fileSystem,
        ITestResultsAnalyzer testResultsAnalyzer)
    {
        Log = logger;
        DotNetTestRunner = dotNetTestRunner;
        DotNetCoverageRunner = dotNetCoverageRunner;
        FileSystem = fileSystem;
        TestResultsAnalyzer = testResultsAnalyzer;
    }

    public async Task<RerunOutcome> RunAsync(RerunOptions options, CancellationToken cancellationToken = default)
    {
        var startOfProcess = DateTime.Now;
        var startOfDotnetRun = DateTime.Now;
        var reportFiles = new HashSet<string>();
        IDirectoryInfo resultsDirectory = FileSystem.DirectoryInfo.New(options.ResultsDirectory);

        var result = await DotNetTestRunner.Test(options, new RunRequest(options.Filter, options.Framework, resultsDirectory.FullName), cancellationToken);
        LogTestResults(options, resultsDirectory, startOfDotnetRun, reportFiles);
        var errorCode = result.Outcome;
        var stoppedEarly = (RerunOutcome?)null;
        var attempt = 1;

        if (errorCode == ErrorCode.FailedTests)
        {
            while (attempt <= options.RerunMaxAttempts)
            {
                var trxFiles = TestResultsAnalyzer.GetTrxFiles(resultsDirectory, startOfDotnetRun);

                if (trxFiles.Length == 0)
                {
                    Log.Information($"No trx file found in {resultsDirectory.FullName}");
                    stoppedEarly = RerunOutcome.NoTestResults;
                    break;
                }

                reportFiles.UnionWith(trxFiles.Select(file => file.FullName));
                var testsToRerun = TestResultsAnalyzer.GetFailedTestsFilter(trxFiles);
                if (testsToRerun.HasTestsToReRun is false)
                {
                    Log.Information($"Rerun attempt {attempt} not needed. All tests Passed.");
                    break;
                }

                // Check if the number of failed tests exceeds the threshold
                if (options.RerunMaxFailedTests > 0 && testsToRerun.TotalFailedTests > options.RerunMaxFailedTests)
                {
                    Log.Error($"Failed tests ({testsToRerun.TotalFailedTests}) exceeded the maximum threshold ({options.RerunMaxFailedTests}). Skipping rerun.");
                    stoppedEarly = RerunOutcome.ThresholdExceeded;
                    break;
                }

                // Check if the percentage of failed tests exceeds the threshold
                if (options.RerunFailedThreshold >= 0 && testsToRerun.TotalTests > 0)
                {
                    var failedPercentage = testsToRerun.TotalFailedTests * 100d / testsToRerun.TotalTests;
                    if (failedPercentage > options.RerunFailedThreshold)
                    {
                        Log.Error($"Failed tests percentage ({failedPercentage:0.##}%) exceeded the maximum threshold ({options.RerunFailedThreshold}%). Skipping rerun.");
                        stoppedEarly = RerunOutcome.ThresholdExceeded;
                        break;
                    }
                }

                // Only wait when there is something to rerun
                await Task.Delay(options.Delay, cancellationToken);
                Log.Information($"Rerun attempt {attempt}/{options.RerunMaxAttempts}");
                Log.Warning($"Found Failed tests: {testsToRerun}");
                startOfDotnetRun = DateTime.Now;

                var attemptResults = new List<ErrorCode>();
                foreach (var tests in testsToRerun.Filters)
                {
                    var batches = FilterBatches.Split(tests.Value.Tests);
                    if (batches.Count > 1)
                        Log.Information($"Rerun filter split into {batches.Count} batches to keep the command line short.");

                    foreach (var batch in batches)
                    {
                        var request = new RunRequest(
                            FilterComposer.Combine(options.Filter, batch),
                            options.Framework,
                            resultsDirectory.FullName);

                        Log.Warning($"Rerun filter: {request.Filter}");
                        attemptResults.Add((await DotNetTestRunner.Test(options, request, cancellationToken)).Outcome);
                    }
                }
                errorCode = AggregateErrorCodes(attemptResults);
                LogTestResults(options, resultsDirectory, startOfDotnetRun, reportFiles);
                attempt++;
            }
        }

        if (options.DeleteReportFiles)
        {
            reportFiles.UnionWith(TestResultsAnalyzer.GetTrxFiles(resultsDirectory, startOfDotnetRun).Select(file => file.FullName));
            foreach (var file in reportFiles)
                FileSystem.File.Delete(file);
        }

        if (options.MergeCoverageFormat.HasValue)
        {
            await DotNetCoverageRunner.Merge(options, resultsDirectory.FullName, startOfProcess, cancellationToken);
        }

        if (errorCode != ErrorCode.FailedTests)
            return attempt > 1 ? RerunOutcome.PassedAfterRerun : RerunOutcome.Passed;

        return stoppedEarly ?? RerunOutcome.Failed;
    }

    /// <summary>
    /// An attempt runs one dotnet test per target framework; it failed if any of them failed.
    /// </summary>
    private static ErrorCode AggregateErrorCodes(IReadOnlyCollection<ErrorCode> errorCodes)
        => errorCodes.Contains(ErrorCode.FailedTests)
            ? ErrorCode.FailedTests
            : errorCodes.LastOrDefault(ErrorCode.Success);

    private void LogTestResults(RerunOptions options, IDirectoryInfo resultsDirectory, DateTime startSearchTime, HashSet<string> reportFiles)
    {
        if (options.LogTestResults is false)
            return;

        var trxFiles = TestResultsAnalyzer.GetTrxFiles(resultsDirectory, startSearchTime);
        reportFiles.UnionWith(trxFiles.Select(file => file.FullName));
        TestResultsAnalyzer.LogTestResults(trxFiles);
    }
}
