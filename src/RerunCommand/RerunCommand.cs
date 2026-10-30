using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using dotnet.test.rerun.Analyzers;
using dotnet.test.rerun.Domain;
using dotnet.test.rerun.DotNetRunner;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Logging;

namespace dotnet.test.rerun.RerunCommand;

public class RerunCommand : RootCommand
{
    private readonly ILogger Log;
    private readonly RerunCommandConfiguration Config;
    private readonly IDotNetTestRunner DotNetTestRunner;
    private readonly IDotNetCoverageRunner DotNetCoverageRunner;
    private readonly IFileSystem FileSystem;
    private readonly ITestResultsAnalyzer TestResultsAnalyzer;

    public RerunCommand(ILogger logger,
        RerunCommandConfiguration config,
        IDotNetTestRunner dotNetTestRunner,
        IDotNetCoverageRunner dotNetCoverageRunner,
        IFileSystem fileSystem,
        ITestResultsAnalyzer testResultsAnalyzer) :
        base("wrapper of dotnet test command with the extra option to automatic rerun failed tests")
    {
        Log = logger;
        Config = config;
        DotNetTestRunner = dotNetTestRunner;
        DotNetCoverageRunner = dotNetCoverageRunner;
        FileSystem = fileSystem;
        TestResultsAnalyzer = testResultsAnalyzer;

        // Allow unmatched tokens (e.g., MSBuild arguments) to pass through
        TreatUnmatchedTokensAsErrors = false;

        // Set Arguments and Options
        config.Set(this);

        this.SetAction(async (parseResult, cancellationToken) =>
        {
            Config.GetValues(parseResult);
            logger.SetLogLevel(Config.LogLevel);
            await Run(cancellationToken);
            return Environment.ExitCode;
        });
    }

    public async Task Run(CancellationToken cancellationToken = default)
    {
        var startOfProcess = DateTime.Now;
        var startOfDotnetRun = DateTime.Now;
        IDirectoryInfo resultsDirectory = FileSystem.DirectoryInfo.New(Config.ResultsDirectory);
        await DotNetTestRunner.Test(Config, resultsDirectory.FullName, cancellationToken);
        LogTestResults(resultsDirectory, startOfDotnetRun);
        var errorCode = DotNetTestRunner.GetErrorCode();
        if (errorCode == ErrorCode.FailedTests)
        {
            var userFramework = Config.Framework;
            var attempt = 1;
            while (attempt <= Config.RerunMaxAttempts)
            {
                await Task.Delay(Config.Delay, cancellationToken);
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
                    if (Config.RerunMaxFailedTests > 0 && testsToRerun.TotalFailedTests > Config.RerunMaxFailedTests)
                    {
                        Environment.ExitCode = 1;
                        Log.Error($"Failed tests ({testsToRerun.TotalFailedTests}) exceeded the maximum threshold ({Config.RerunMaxFailedTests}). Skipping rerun.");
                        break;
                    }

                    // Check if the percentage of failed tests exceeds the threshold
                    if (Config.RerunFailedThreshold >= 0 && testsToRerun.TotalTests > 0)
                    {
                        var failedPercentage = testsToRerun.TotalFailedTests * 100d / testsToRerun.TotalTests;
                        if (failedPercentage > Config.RerunFailedThreshold)
                        {
                            Environment.ExitCode = 1;
                            Log.Error($"Failed tests percentage ({failedPercentage:0.##}%) exceeded the maximum threshold ({Config.RerunFailedThreshold}%). Skipping rerun.");
                            break;
                        }
                    }

                    Log.Information($"Rerun attempt {attempt}/{Config.RerunMaxAttempts}");
                    Log.Warning($"Found Failed tests: {testsToRerun}");
                    startOfDotnetRun = DateTime.Now;

                    var attemptResults = new List<ErrorCode>();
                    foreach (var tests in testsToRerun.Filters)
                    {
                        Config.Filter = Config.AppendFailedTests(tests.Value.Filter);
                        // Rerun only the target framework where the tests failed, unless the user chose one
                        Config.Framework = userFramework ?? GetFrameworkToRerun(tests.Value);

                        Log.Warning($"Rerun filter: {Config.Filter}");
                        await DotNetTestRunner.Test(Config, resultsDirectory.FullName, cancellationToken);
                        attemptResults.Add(DotNetTestRunner.GetErrorCode());
                    }
                    Config.Framework = userFramework;
                    errorCode = AggregateErrorCodes(attemptResults);
                    LogTestResults(resultsDirectory, startOfDotnetRun);
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

        if (Config.DeleteReportFiles)
        {
            TestResultsAnalyzer.AddLastTrxFiles(resultsDirectory, startOfDotnetRun);
            DeleteFiles();
        }
        
        if (Config.MergeCoverageFormat.HasValue)
        {
            await MergeCoverageResults(resultsDirectory, startOfProcess, cancellationToken);
        }
    }

    private static string? GetFrameworkToRerun(TestFilter testFilter)
        => testFilter.Framework == TestFilter.NoFrameworkName ? null : testFilter.Framework;

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

    private void LogTestResults(IDirectoryInfo resultsDirectory, DateTime startSearchTime)
    {
        if (Config.LogTestResults is false)
            return;

        var trxFiles = TestResultsAnalyzer.GetTrxFiles(resultsDirectory, startSearchTime);
        TestResultsAnalyzer.LogTestResults(trxFiles);
    }

    private Task MergeCoverageResults(IDirectoryInfo resultsDirectory, DateTime startTime, CancellationToken cancellationToken)
        => DotNetCoverageRunner.Merge(Config, resultsDirectory.FullName, startTime, cancellationToken);
}