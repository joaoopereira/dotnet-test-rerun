using System.CommandLine;
using dotnet.test.rerun.Configuration;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Logging;

namespace dotnet.test.rerun.Cli;

/// <summary>
/// Defines the command-line arguments and options, and maps a parse result to <see cref="RerunOptions"/>.
/// </summary>
public class RerunCliDefinition
{
    #region Arguments

    private readonly Argument<string> PathArgument = new("path")
    {
        Description = "Path to a test project .dll file.",
        Arity = ArgumentArity.ZeroOrOne
    };

    #endregion Arguments

    #region Options

    private readonly Option<string> FilterOption = new("--filter")
    {
        Description = "Run tests that match the given expression."
    };

    private readonly Option<string> SettingsOption = new("--settings", "-s")
    {
        Description = "The run settings file to use when running tests."
    };

    private readonly Option<IEnumerable<string>> LoggerOption = new("--logger", "-l")
    {
        Description = "Specifies a logger for test results.",
        DefaultValueFactory = _ => new[] { "trx" }
    };

    private readonly Option<string> ResultsDirectoryOption = new("--results-directory", "-r")
    {
        Description = "The directory where the test results will be placed.\nThe specified directory will be created if it does not exist.",
        DefaultValueFactory = _ => "."
    };

    private readonly Option<int> RerunMaxAttemptsOption = new("--rerunMaxAttempts")
    {
        Description = "Maximum # of attempts.",
        DefaultValueFactory = _ => 3
    };

    private readonly Option<int> RerunMaxFailedTestsOption = new("--rerunMaxFailedTests")
    {
        Description = "Maximum # of failed tests to rerun. If exceeded, tests will not be rerun.",
        DefaultValueFactory = _ => -1
    };

    private readonly Option<double> RerunFailedThresholdOption = new("--rerunFailedThreshold")
    {
        Description = "Maximum percentage of failed tests allowed to trigger a rerun. If exceeded, tests will not be rerun.",
        DefaultValueFactory = _ => -1
    };

    private readonly Option<LogLevel> LogLevelOption = new("--loglevel")
    {
        Description = "Log Level",
        DefaultValueFactory = _ => LogLevel.Verbose
    };

    private readonly Option<bool> NoBuildOption = new("--no-build")
    {
        Description = "Do not build the project before testing. Implies --no-restore.",
        Arity = ArgumentArity.Zero
    };

    private readonly Option<bool> NoRestoreOption = new("--no-restore")
    {
        Description = "Do not restore the project before building.",
        Arity = ArgumentArity.Zero
    };

    private readonly Option<int> DelayOption = new("--delay", "-d")
    {
        Description = "Delay between test runs in seconds."
    };

    private readonly Option<bool> BlameOption = new("--blame")
    {
        Description = "Runs the tests in blame mode.",
        Arity = ArgumentArity.Zero
    };

    private readonly Option<bool> DeleteReportFilesOption = new("--deleteReports")
    {
        Description = "Delete the report files generated.",
        Arity = ArgumentArity.Zero
    };
    
    private readonly Option<string> CollectorOption = new("--collect")
    {
        Description = "Enables data collector for the test run."
    };

    private readonly Option<CoverageFormat?> MergeCoverageFormatOption = new("--mergeCoverageFormat")
    {
        Description = "Output coverage format. Note: requires dotnet coverage tool to be installed."
    };
    
    private readonly Option<string> ConfigurationOption = new("--configuration", "-c")
    {
        Description = "Defines the build configuration."
    };
    
    private readonly Option<string> FrameworkOption = new("--framework", "-f")
    {
        Description = "Defines the target framework to run the tests."
    };
    
    private readonly Option<LoggerVerbosity?> VerbosityOption = new("--verbosity", "-v")
    {
        Description = "Sets the verbosity level of the command. Possible values: Quiet, Minimal, Normal, Detailed, Diagnostic"
    };
    
    private readonly Option<string[]> InlineRunSettingsOption = new("--inlineRunSettings")
    {
        Description = "Specifies the inline run settings.",
        AllowMultipleArgumentsPerToken = true
    };
    
    private readonly Option<IEnumerable<string>> EnvironmentVariablesOption = new("--environment", "-e")
    {
        Description = "Sets the value of an environment variable.",
        AllowMultipleArgumentsPerToken = true
    };

    private readonly Option<bool> LogPassedTestsOption = new("--logPassedTests")
    {
        Description = "Logs each passed test as it completes during execution. This can produce a lot of output.",
        Arity = ArgumentArity.Zero
    };

    private readonly Option<bool> LogTestResultsOption = new("--logTestResults")
    {
        Description = "Logs the result (passed/failed) of each individual test, independently of the dotnet test verbosity.",
        Arity = ArgumentArity.Zero
    };

    #endregion Options

    private static readonly string[] MsBuildArgumentPrefixes =
    {
        "/p:", "-p:", "--property:", "/property:", "-property:",
        "/m:", "-m:", "/maxCpuCount:", "-maxCpuCount:", "--maxCpuCount:"
    };

    public void AddTo(Command cmd)
    {
        cmd.Arguments.Add(PathArgument);
        cmd.Options.Add(FilterOption);
        cmd.Options.Add(SettingsOption);
        cmd.Options.Add(LoggerOption);
        cmd.Options.Add(ResultsDirectoryOption);
        cmd.Options.Add(RerunMaxAttemptsOption);
        cmd.Options.Add(RerunMaxFailedTestsOption);
        cmd.Options.Add(RerunFailedThresholdOption);
        cmd.Options.Add(LogLevelOption);
        cmd.Options.Add(NoBuildOption);
        cmd.Options.Add(NoRestoreOption);
        cmd.Options.Add(DelayOption);
        cmd.Options.Add(BlameOption);
        cmd.Options.Add(ConfigurationOption);
        cmd.Options.Add(FrameworkOption);
        cmd.Options.Add(VerbosityOption);
        cmd.Options.Add(DeleteReportFilesOption);
        cmd.Options.Add(CollectorOption);
        cmd.Options.Add(MergeCoverageFormatOption);
        cmd.Options.Add(InlineRunSettingsOption);
        cmd.Options.Add(EnvironmentVariablesOption);
        cmd.Options.Add(LogPassedTestsOption);
        cmd.Options.Add(LogTestResultsOption);
    }

    public RerunOptions ToOptions(ParseResult parseResult)
        => new()
        {
            Path = parseResult.GetValue(PathArgument),
            Filter = parseResult.GetValue(FilterOption),
            Settings = parseResult.GetValue(SettingsOption),
            Loggers = (parseResult.GetValue(LoggerOption) ?? Array.Empty<string>()).ToArray(),
            ResultsDirectory = parseResult.GetValue(ResultsDirectoryOption)!,
            RerunMaxAttempts = parseResult.GetValue(RerunMaxAttemptsOption),
            RerunMaxFailedTests = parseResult.GetValue(RerunMaxFailedTestsOption),
            RerunFailedThreshold = parseResult.GetValue(RerunFailedThresholdOption),
            LogLevel = parseResult.GetValue(LogLevelOption),
            NoBuild = parseResult.GetValue(NoBuildOption),
            NoRestore = parseResult.GetValue(NoRestoreOption),
            Delay = TimeSpan.FromSeconds(parseResult.GetValue(DelayOption)),
            Blame = parseResult.GetValue(BlameOption),
            Configuration = parseResult.GetValue(ConfigurationOption),
            Framework = parseResult.GetValue(FrameworkOption),
            Verbosity = parseResult.GetValue(VerbosityOption),
            DeleteReportFiles = parseResult.GetValue(DeleteReportFilesOption),
            Collector = parseResult.GetValue(CollectorOption),
            MergeCoverageFormat = parseResult.GetValue(MergeCoverageFormatOption),
            MsBuildArguments = parseResult.UnmatchedTokens.Where(IsMsBuildArgument).ToArray(),
            InlineRunSettings = (parseResult.GetValue(InlineRunSettingsOption) ?? Array.Empty<string>())
                .Concat(parseResult.UnmatchedTokens.Where(token => !IsMsBuildArgument(token)))
                .ToArray(),
            EnvironmentVariables = (parseResult.GetValue(EnvironmentVariablesOption) ?? Array.Empty<string>()).ToArray(),
            LogPassedTests = parseResult.GetValue(LogPassedTestsOption),
            LogTestResults = parseResult.GetValue(LogTestResultsOption)
        };

    private static bool IsMsBuildArgument(string token)
        => MsBuildArgumentPrefixes.Any(prefix => token.StartsWith(prefix));
}
