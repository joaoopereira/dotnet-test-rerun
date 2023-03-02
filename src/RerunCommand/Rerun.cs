using System.CommandLine;
using System.IO.Abstractions;
using dotnet.test.rerun.Logging;
using TrxFileParser;

namespace dotnet.test.rerun.RerunCommand;

public class RerunCommand : RootCommand
{
    private readonly ILogger Log;
    private readonly RerunCommandConfiguration config;
    private readonly dotnet dotnet;
    private readonly IFileSystem fileSystem;

    public RerunCommand(ILogger logger, RerunCommandConfiguration config, dotnet dotnet, IFileSystem fileSystem) :
        base("wrapper of dotnet test command with the extra option to automatic rerun failed tests")
    {
        this.Log = logger;
        this.config = config;
        this.dotnet = dotnet;
        this.fileSystem = fileSystem;

        // Set Arguments and Options
        config.Set(this);

        this.SetHandler((context) =>
        {
            this.config.GetValues(context);
            logger.SetLogLevel(this.config.LogLevel);
            Run();
        });
    }

    public void Run()
    {
        IDirectoryInfo resultsDirectory = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var oldTrxFile = GetTrxFile(resultsDirectory);
        dotnet.Test(config, resultsDirectory.FullName);
        if (dotnet.ErrorCode == ErrorCode.FailedTests)
        {
            var attempt = 1;
            while (attempt <= config.RerunMaxAttempts)
            {
                var trxFile = GetTrxFile(resultsDirectory);

                if (trxFile != null)
                {
                    if (oldTrxFile != null &&
                        trxFile.FullName.Equals(oldTrxFile.FullName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Log.Error("No new trx file was generated");
                        break;
                    }


                    var testsToRerun = GetFailedTestsFilter(trxFile);
                    oldTrxFile = trxFile;
                    if (!string.IsNullOrEmpty(testsToRerun))
                    {
                        Log.Information($"Rerun attempt {attempt}/{config.RerunMaxAttempts}");
                        Log.Warning($"Found Failed tests. Rerun filter: {testsToRerun}");
                        dotnet.Test(config, config.ResultsDirectory);
                        attempt++;
                    }
                    else
                    {
                        Log.Information($"Rerun attempt {attempt} not needed. All testes Passed.");
                        attempt = config.RerunMaxAttempts;
                    }
                }
                else
                {
                    Log.Information($"No trx file found in {resultsDirectory.FullName}");
                    break;
                }
            }
        }
    }

    internal string GetFailedTestsFilter(IFileInfo trxFile)
    {
        var testFilter = string.Empty;
        var outcome = "Failed";

        var trx = TrxDeserializer.Deserialize(trxFile.FullName);

        var tests = trx.Results.UnitTestResults
            .Where(t => t.Outcome.Equals(outcome, StringComparison.InvariantCultureIgnoreCase)).ToList();

        if (tests != null && !tests.Any())
        {
            Log.Warning($"No tests found with the Outcome {outcome}");
        }
        else
        {
            for (var i = 0; i < tests.Count; i++)
            {
                testFilter += $"FullyQualifiedName~{tests[i].TestName}" +
                              (tests.Count() - 1 != i ? " | " : string.Empty);
            }

            Log.Debug(testFilter);
        }

        return testFilter;
    }

    private IFileInfo? GetTrxFile(IDirectoryInfo resultsDirectory)
        => resultsDirectory.EnumerateFiles("*.trx").OrderBy(f => f.Name).LastOrDefault();
}