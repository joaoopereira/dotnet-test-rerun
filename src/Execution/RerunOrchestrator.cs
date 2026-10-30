using System.IO.Abstractions;
using System.Text.RegularExpressions;
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
    private const string AttemptDirectoryPrefix = "attempt-";
    private static readonly Regex AttemptDirectoryName = new(@"^attempt-\d+$");
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
        var reportFiles = new HashSet<string>();
        IDirectoryInfo resultsDirectory = FileSystem.DirectoryInfo.New(options.ResultsDirectory);
        RemovePreviousAttemptDirectories(resultsDirectory);
        var attemptDirectories = new List<IDirectoryInfo>();
        // Each attempt writes to its own directory, so its results are found without relying on timestamps
        var attemptDirectory = NewAttemptDirectory(resultsDirectory, 0, attemptDirectories);

        var result = await DotNetTestRunner.Test(options, new RunRequest(options.Filter, options.Framework, attemptDirectory.FullName), cancellationToken);
        LogTestResults(options, attemptDirectory, reportFiles);
        var errorCode = result.Outcome;
        var stoppedEarly = (RerunOutcome?)null;
        var attempt = 1;

        if (errorCode == ErrorCode.FailedTests)
        {
            while (attempt <= options.RerunMaxAttempts)
            {
                var trxFiles = TestResultsAnalyzer.GetTrxFiles(attemptDirectory);

                if (trxFiles.Length == 0)
                {
                    Log.Information($"No trx file found in {attemptDirectory.FullName}");
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
                attemptDirectory = NewAttemptDirectory(resultsDirectory, attempt, attemptDirectories);

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
                            attemptDirectory.FullName);

                        Log.Warning($"Rerun filter: {request.Filter}");
                        attemptResults.Add((await DotNetTestRunner.Test(options, request, cancellationToken)).Outcome);
                    }
                }
                errorCode = AggregateErrorCodes(attemptResults);
                LogTestResults(options, attemptDirectory, reportFiles);
                attempt++;
            }
        }

        if (options.MergeCoverageFormat.HasValue)
        {
            await DotNetCoverageRunner.Merge(options,
                attemptDirectories.Select(directory => directory.FullName), resultsDirectory.FullName, cancellationToken);
        }

        if (options.DeleteReportFiles)
        {
            foreach (var directory in attemptDirectories)
                reportFiles.UnionWith(TestResultsAnalyzer.GetTrxFiles(directory).Select(file => file.FullName));
            foreach (var file in reportFiles)
                FileSystem.File.Delete(file);
            RemoveEmptyDirectories(attemptDirectories);
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

    private IDirectoryInfo NewAttemptDirectory(IDirectoryInfo resultsDirectory, int attempt, List<IDirectoryInfo> attemptDirectories)
    {
        var directory = FileSystem.DirectoryInfo.New(FileSystem.Path.Combine(resultsDirectory.FullName, $"{AttemptDirectoryPrefix}{attempt}"));
        attemptDirectories.Add(directory);
        return directory;
    }

    /// <summary>
    /// Results of a previous test-rerun in the same results directory would be mistaken for this run's.
    /// </summary>
    private void RemovePreviousAttemptDirectories(IDirectoryInfo resultsDirectory)
    {
        if (!resultsDirectory.Exists)
            return;

        foreach (var directory in resultsDirectory.EnumerateDirectories($"{AttemptDirectoryPrefix}*")
                     .Where(directory => AttemptDirectoryName.IsMatch(directory.Name)))
        {
            Log.Debug($"Removing results of a previous run: {directory.FullName}");
            directory.Delete(recursive: true);
        }
    }

    private static void RemoveEmptyDirectories(IEnumerable<IDirectoryInfo> directories)
    {
        foreach (var directory in directories)
        {
            directory.Refresh();
            if (directory.Exists && !directory.EnumerateFileSystemInfos().Any())
                directory.Delete();
        }
    }

    private void LogTestResults(RerunOptions options, IDirectoryInfo attemptDirectory, HashSet<string> reportFiles)
    {
        if (options.LogTestResults is false)
            return;

        var trxFiles = TestResultsAnalyzer.GetTrxFiles(attemptDirectory);
        reportFiles.UnionWith(trxFiles.Select(file => file.FullName));
        TestResultsAnalyzer.LogTestResults(trxFiles);
    }
}
