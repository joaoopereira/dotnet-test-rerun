using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using dotnet.test.rerun.Logging;
using TrxFileParser;

namespace dotnet.test.rerun
{
    public class RerunCommand : RootCommand
    {
        private readonly ILogger Log;
        private readonly RerunCommandConfiguration config;
        private readonly dotnet dotnet;
        private readonly IFileSystem fileSystem;

        public RerunCommand(ILogger logger, RerunCommandConfiguration config, dotnet dotnet, IFileSystem fileSystem)
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
            dotnet.Test(config.Path, config.Filter, config.Settings, config.TrxLogger, config.ResultsDirectory);
            if (dotnet.ErrorCode == ErrorCode.FailedTests)
            {
                IDirectoryInfo resultsDirectory = fileSystem.DirectoryInfo.New(config.ResultsDirectory);

                var attempt = 1;
                while (attempt < config.RerunMaxAttempts)
                {
                    var trxFile = resultsDirectory.EnumerateFiles("*.trx").OrderBy(f => f.Name).LastOrDefault();
                    if (trxFile != null)
                    {
                        var testsToRerun = GetFailedTestsFilter(trxFile);
                        if (!string.IsNullOrEmpty(testsToRerun))
                        {
                            Log.Information($"Rerun attempt {attempt}/{config.RerunMaxAttempts}");
                            Log.Warning($"Found Failed tests. Rerun filter: {testsToRerun}");
                            dotnet.Test(config.Path, config.Filter, config.Settings, config.TrxLogger, config.ResultsDirectory);
                            attempt++;
                        }
                        else
                        {
                            Log.Information($"Rerun attempt {attempt} not needed. All testes Passed.");
                            attempt = config.RerunMaxAttempts;
                        }
                    }
                }
            }
        }

        private string GetFailedTestsFilter(IFileInfo trxFile)
        {
            var testFilter = string.Empty;
            var outcome = "Failed";

            var trx = TrxDeserializer.Deserialize(trxFile.FullName);

            var tests = trx.Results.UnitTestResults.Where(t => t.Outcome.Equals(outcome, StringComparison.InvariantCultureIgnoreCase)).ToList();

            if (tests != null && !tests.Any())
            {
                Log.Warning($"No tests found with the Outcome {outcome}");
            }
            else
            {
                for (var i = 0; i < tests.Count; i++)
                {
                    testFilter += $"FullyQualifiedName~{tests[i].TestName}" + (tests.Count() - 1 != i ? " | " : string.Empty);
                }
                Log.Debug(testFilter);
            }
            return testFilter;
        }
    }

    public class RerunCommandConfiguration
    {
        #region Properties

        public string Path { get; private set; }
        public string Filter { get; private set; }
        public string Settings { get; private set; }
        public string TrxLogger { get; private set; }
        public string ResultsDirectory { get; private set; }
        public int RerunMaxAttempts { get; private set; }
        public LogLevel LogLevel { get; private set; }

        #endregion Properties

        #region Arguments

        private Argument<string> PathArgument = new("path")
        {
            Description = "Path to a test project .dll file."
        };

        #endregion Arguments

        #region Options

        private readonly Option<string> FilterOption = new(new[] { "--filter" })
        {
            Description = "Run tests that match the given expression.",
            IsRequired = true
        };

        private readonly Option<string> SettingsOption = new(new[] { "--settings", "-s" })
        {
            Description = "The run settings file to use when running tests.",
            IsRequired = true
        };

        private readonly Option<string> LoggerOption = new(new[] { "--logger", "-l" }, getDefaultValue: () => "trx")
        {
            Description = "Specifies a logger for test results.",
            IsRequired = false
        };

        private readonly Option<string> ResultsDirectoryOption = new(new[] { "--results-directory", "-r" }, getDefaultValue: () => ".")
        {
            Description = "The directory where the test results will be placed.\nThe specified directory will be created if it does not exist.",
            IsRequired = false
        };

        private readonly Option<int> RerunMaxAttemptsOption = new(new[] { "--rerunMaxAttempts" }, getDefaultValue: () => 3)
        {
            Description = "Maximum # of attempts. Default: 3.",
            IsRequired = false
        };

        private readonly Option<LogLevel> LogLevelOption = new(new[] { "--loglevel" }, parseArgument: Logger.ParseLogLevel, isDefault: true)
        {
            Description = "ParseLogLevel",
            IsRequired = false,
        };

        #endregion Options

        public void Set(Command cmd)
        {
            cmd.Add(PathArgument);
            cmd.Add(FilterOption);
            cmd.Add(SettingsOption);
            cmd.Add(LoggerOption);
            cmd.Add(ResultsDirectoryOption);
            cmd.Add(RerunMaxAttemptsOption);
            cmd.Add(LogLevelOption);
        }

        public void GetValues(InvocationContext context)
        {
            Path = context.ParseResult.GetValueForArgument(PathArgument);
            Filter = context.ParseResult.GetValueForOption(FilterOption);
            Settings = context.ParseResult.GetValueForOption(SettingsOption);
            TrxLogger = context.ParseResult.GetValueForOption(LoggerOption);
            ResultsDirectory = context.ParseResult.GetValueForOption(ResultsDirectoryOption);
            RerunMaxAttempts = context.ParseResult.GetValueForOption(RerunMaxAttemptsOption);
            LogLevel = context.ParseResult.GetValueForOption(LogLevelOption);
        }
    }
}