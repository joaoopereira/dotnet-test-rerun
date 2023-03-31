using System.CommandLine;
using System.IO.Abstractions;
using dotnet.test.rerun.Analyzers;
using dotnet.test.rerun.DotNetTestRunner;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Logging;

namespace dotnet.test.rerun.RerunCommand;

public class RerunCommand : RootCommand
{
    private readonly ILogger Log;
    private readonly RerunCommandConfiguration Config;
    private readonly IDotNetTestRunner DotNetTestRunner;
    private readonly IFileSystem FileSystem;
    private readonly ITestResultsAnalyzer TestResultsAnalyzer;

    public RerunCommand(ILogger logger,
        RerunCommandConfiguration config,
        IDotNetTestRunner dotNetTestRunner,
        IFileSystem fileSystem,
        ITestResultsAnalyzer testResultsAnalyzer) :
        base("wrapper of dotnet test command with the extra option to automatic rerun failed tests")
    {
        Log = logger;
        Config = config;
        DotNetTestRunner = dotNetTestRunner;
        FileSystem = fileSystem;
        TestResultsAnalyzer = testResultsAnalyzer;

        // Set Arguments and Options
        config.Set(this);

        this.SetHandler(async (context) =>
        {
            Config.GetValues(context);
            logger.SetLogLevel(Config.LogLevel);
            await Run();
        });
    }

    public async Task Run()
    {
        IDirectoryInfo resultsDirectory = FileSystem.DirectoryInfo.New(Config.ResultsDirectory);
        var oldTrxFile = TestResultsAnalyzer.GetTrxFile(resultsDirectory);
        await DotNetTestRunner.Test(Config, resultsDirectory.FullName);
        if (DotNetTestRunner.GetErrorCode() == ErrorCode.FailedTests)
        {
            var attempt = 1;
            while (attempt <= Config.RerunMaxAttempts)
            {
                await Task.Delay(Config.Delay);
                var trxFile = TestResultsAnalyzer.GetTrxFile(resultsDirectory);

                if (trxFile != null)
                {
                    if (oldTrxFile != null &&
                        trxFile.FullName.Equals(oldTrxFile.FullName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Log.Error("No new trx file was generated");
                        break;
                    }

                    var testsToRerun = TestResultsAnalyzer.GetFailedTestsFilter(trxFile);
                    oldTrxFile = trxFile;
                    if (string.IsNullOrEmpty(testsToRerun))
                    {
                        Log.Information($"Rerun attempt {attempt} not needed. All testes Passed.");
                        break;
                    }

                    Log.Information($"Rerun attempt {attempt}/{Config.RerunMaxAttempts}");
                    Log.Warning($"Found Failed tests. Rerun filter: {testsToRerun}");
                    await DotNetTestRunner.Test(Config, resultsDirectory.FullName);
                    attempt++;
                }
                else
                {
                    Log.Information($"No trx file found in {resultsDirectory.FullName}");
                    break;
                }
            }
        }
    }
}