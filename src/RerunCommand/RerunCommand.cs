﻿using System.CommandLine;
using System.CommandLine.Invocation;
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

        // Set Arguments and Options
        config.Set(this);

        // Set up action using custom CommandLineAction
        this.Action = new RerunCommandAction(async parseResult =>
        {
            Config.GetValues(parseResult);
            logger.SetLogLevel(Config.LogLevel);
            await Run();
            return Environment.ExitCode;
        });
    }

    public async Task Run()
    {
        var startOfProcess = DateTime.Now;
        var startOfDotnetRun = DateTime.Now;
        IDirectoryInfo resultsDirectory = FileSystem.DirectoryInfo.New(Config.ResultsDirectory);
        await DotNetTestRunner.Test(Config, resultsDirectory.FullName);
        if (DotNetTestRunner.GetErrorCode() == ErrorCode.FailedTests)
        {
            var attempt = 1;
            while (attempt <= Config.RerunMaxAttempts)
            {
                await Task.Delay(Config.Delay);
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

                    Log.Information($"Rerun attempt {attempt}/{Config.RerunMaxAttempts}");
                    Log.Warning($"Found Failed tests: {testsToRerun}");
                    startOfDotnetRun = DateTime.Now;

                    foreach (var tests in testsToRerun.Filters)
                    {
                        Config.Filter = Config.AppendFailedTests(tests.Value.Filter);

                        Log.Warning($"Rerun filter: {Config.Filter}");
                        await DotNetTestRunner.Test(Config, resultsDirectory.FullName);
                    }
                    attempt++;
                }
                else
                {
                    Log.Information($"No trx file found in {resultsDirectory.FullName}");
                    break;
                }
            }
        }

        if (DotNetTestRunner.GetErrorCode() == ErrorCode.FailedTests)
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
            MergeCoverageResults(resultsDirectory, startOfProcess);
        }
    }

    private void DeleteFiles()
    {
        foreach (var file in TestResultsAnalyzer.GetReportFiles())
        {
            FileSystem.File.Delete(file);
        }
    }

    private void MergeCoverageResults(IDirectoryInfo resultsDirectory, DateTime startTime)
        => DotNetCoverageRunner.Merge(Config, resultsDirectory.FullName, startTime);
}

/// <summary>
/// Custom CommandLineAction for handling asynchronous rerun command execution
/// </summary>
internal class RerunCommandAction : AsynchronousCommandLineAction
{
    private readonly Func<ParseResult, Task<int>> _handler;
    
    public RerunCommandAction(Func<ParseResult, Task<int>> handler)
    {
        _handler = handler;
    }
    
    public override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        return _handler(parseResult);
    }
}