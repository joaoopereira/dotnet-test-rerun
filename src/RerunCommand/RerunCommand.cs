using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using dotnet.test.rerun.Analyzers;
using dotnet.test.rerun.Cli;
using dotnet.test.rerun.Configuration;
using dotnet.test.rerun.Domain;
using dotnet.test.rerun.DotNetRunner;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Execution;
using dotnet.test.rerun.Logging;

namespace dotnet.test.rerun.RerunCommand;

public class RerunCommand : RootCommand
{
    private readonly ILogger Log;
    private readonly IDotNetTestRunner DotNetTestRunner;
    private readonly IDotNetCoverageRunner DotNetCoverageRunner;
    private readonly IFileSystem FileSystem;
    private readonly ITestResultsAnalyzer TestResultsAnalyzer;

    public RerunCommand(ILogger logger,
        RerunCliDefinition cliDefinition,
        IDotNetTestRunner dotNetTestRunner,
        IDotNetCoverageRunner dotNetCoverageRunner,
        IFileSystem fileSystem,
        ITestResultsAnalyzer testResultsAnalyzer) :
        base("wrapper of dotnet test command with the extra option to automatic rerun failed tests")
    {
        Log = logger;
        DotNetTestRunner = dotNetTestRunner;
        DotNetCoverageRunner = dotNetCoverageRunner;
        FileSystem = fileSystem;
        TestResultsAnalyzer = testResultsAnalyzer;

        // Allow unmatched tokens (e.g., MSBuild arguments) to pass through
        TreatUnmatchedTokensAsErrors = false;

        // Set Arguments and Options
        cliDefinition.AddTo(this);

        this.SetAction(async (parseResult, cancellationToken) =>
        {
            var options = cliDefinition.ToOptions(parseResult);
            logger.SetLogLevel(options.LogLevel);
            await Run(options, cancellationToken);
            return Environment.ExitCode;
        });
    }

    public async Task Run(RerunOptions options, CancellationToken cancellationToken = default)
    {
        var startOfProcess = DateTime.Now;
        var startOfDotnetRun = DateTime.Now;
        IDirectoryInfo resultsDirectory = FileSystem.DirectoryInfo.New(options.ResultsDirectory);
        await DotNetTestRunner.Test(options, new RunRequest(options.Filter, options.Framework, resultsDirectory.FullName), cancellationToken);
        LogTestResults(options, resultsDirectory, startOfDotnetRun);
        var errorCode = DotNetTestRunner.GetErrorCode();
        if (errorCode == ErrorCode.FailedTests)
        {
            var attempt = 1;
            while (attempt <= options.RerunMaxAttempts)
            {
                await Task.Delay(options.Delay, cancellationToken);
                var trxFiles = TestResultsAnalyzer.GetTrxFiles(resultsDirectory, startOfDotnetRun);

                if (trxFiles.Length > 0)
                {
                    var testsToRerun = TestResultsAnalyzer.GetFailedTestsFilter(trxFiles);
                    if (testsToRerun.HasTestsToReRun is false)
                    {
                        Environment.ExitCode = 0;
                        Log.Information($"Rerun attempt {attempt} not needed. All tests Passed.");
                        break;
                    }

                    // Check if the number of failed tests exceeds the threshold
                    if (options.RerunMaxFailedTests > 0 && testsToRerun.TotalFailedTests > options.RerunMaxFailedTests)
                    {
                        Environment.ExitCode = 1;
                        Log.Error($"Failed tests ({testsToRerun.TotalFailedTests}) exceeded the maximum threshold ({options.RerunMaxFailedTests}). Skipping rerun.");
                        break;
                    }

                    // Check if the percentage of failed tests exceeds the threshold
                    if (options.RerunFailedThreshold >= 0 && testsToRerun.TotalTests > 0)
                    {
                        var failedPercentage = testsToRerun.TotalFailedTests * 100d / testsToRerun.TotalTests;
                        if (failedPercentage > options.RerunFailedThreshold)
                        {
                            Environment.ExitCode = 1;
                            Log.Error($"Failed tests percentage ({failedPercentage:0.##}%) exceeded the maximum threshold ({options.RerunFailedThreshold}%). Skipping rerun.");
                            break;
                        }
                    }

                    Log.Information($"Rerun attempt {attempt}/{options.RerunMaxAttempts}");
                    Log.Warning($"Found Failed tests: {testsToRerun}");
                    startOfDotnetRun = DateTime.Now;

                    var attemptResults = new List<ErrorCode>();
                    foreach (var tests in testsToRerun.Filters)
                    {
                        var request = new RunRequest(
                            FilterComposer.Combine(options.Filter, tests.Value.Filter),
                            options.Framework,
                            resultsDirectory.FullName);

                        Log.Warning($"Rerun filter: {request.Filter}");
                        await DotNetTestRunner.Test(options, request, cancellationToken);
                        attemptResults.Add(DotNetTestRunner.GetErrorCode());
                    }
                    errorCode = AggregateErrorCodes(attemptResults);
                    LogTestResults(options, resultsDirectory, startOfDotnetRun);
                    attempt++;
                }
                else
                {
                    Log.Information($"No trx file found in {resultsDirectory.FullName}");
                    break;
                }
            }
        }

        if (errorCode == ErrorCode.FailedTests)
        {
            Environment.ExitCode = 1;
        }

        if (options.DeleteReportFiles)
        {
            TestResultsAnalyzer.AddLastTrxFiles(resultsDirectory, startOfDotnetRun);
            DeleteFiles();
        }
        
        if (options.MergeCoverageFormat.HasValue)
        {
            await DotNetCoverageRunner.Merge(options, resultsDirectory.FullName, startOfProcess, cancellationToken);
        }
    }

    /// <summary>
    /// An attempt runs one dotnet test per target framework; it failed if any of them failed.
    /// </summary>
    private static ErrorCode AggregateErrorCodes(IReadOnlyCollection<ErrorCode> errorCodes)
        => errorCodes.Contains(ErrorCode.FailedTests)
            ? ErrorCode.FailedTests
            : errorCodes.LastOrDefault(ErrorCode.Success);

    private void DeleteFiles()
    {
        foreach (var file in TestResultsAnalyzer.GetReportFiles())
        {
            FileSystem.File.Delete(file);
        }
    }

    private void LogTestResults(RerunOptions options, IDirectoryInfo resultsDirectory, DateTime startSearchTime)
    {
        if (options.LogTestResults is false)
            return;

        var trxFiles = TestResultsAnalyzer.GetTrxFiles(resultsDirectory, startSearchTime);
        TestResultsAnalyzer.LogTestResults(trxFiles);
    }
}