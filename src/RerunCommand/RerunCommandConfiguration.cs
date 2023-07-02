using System.CommandLine;
using System.CommandLine.Invocation;
using dotnet.test.rerun.Logging;

namespace dotnet.test.rerun.RerunCommand;

public class RerunCommandConfiguration
{
    #region Properties

    public string Path { get; private set; }
    public string Filter { get; internal set; }
    public string Settings { get; private set; }
    public string TrxLogger { get; private set; }
    public string ResultsDirectory { get; private set; }
    public int RerunMaxAttempts { get; private set; }
    public LogLevel LogLevel { get; private set; }
    public bool NoBuild { get; private set; }
    public bool NoRestore { get; private set; }
    public int Delay { get; private set; }
    public bool Blame { get; private set; }
    public bool DeleteReportFiles { get; private set; }

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
        IsRequired = false
    };

    private readonly Option<string> SettingsOption = new(new[] { "--settings", "-s" })
    {
        Description = "The run settings file to use when running tests.",
        IsRequired = false
    };

    private readonly Option<string> LoggerOption = new(new[] { "--logger", "-l" }, getDefaultValue: () => "trx")
    {
        Description = "Specifies a logger for test results.",
        IsRequired = false
    };

    private readonly Option<string> ResultsDirectoryOption =
        new(new[] { "--results-directory", "-r" }, getDefaultValue: () => ".")
        {
            Description =
                "The directory where the test results will be placed.\nThe specified directory will be created if it does not exist.",
            IsRequired = false
        };

    private readonly Option<int> RerunMaxAttemptsOption = new(new[] { "--rerunMaxAttempts" }, getDefaultValue: () => 3)
    {
        Description = "Maximum # of attempts.",
        IsRequired = false
    };

    private readonly Option<LogLevel> LogLevelOption =
        new(new[] { "--loglevel" }, parseArgument: Logger.ParseLogLevel, isDefault: true)
        {
            Description = "Log Level",
            IsRequired = false,
        };

    private readonly Option<string> NoBuildOption =
        new(new[] { "--no-build" })
        {
            Description = "Do not build the project before testing. Implies --no-restore.",
            IsRequired = false,
            Arity = ArgumentArity.Zero
        };

    private readonly Option<string> NoRestoreOption =
        new(new[] { "--no-restore" })
        {
            Description = "Do not restore the project before building.",
            IsRequired = false,
            Arity = ArgumentArity.Zero
        };

    private readonly Option<int> DelayOption =
        new(new[] { "--delay", "-d" })
        {
            Description = "Delay between test runs in seconds.",
            IsRequired = false,
        };

    private readonly Option<string> BlameOption =
        new(new[] { "--blame" })
        {
            Description = "Runs the tests in blame mode.",
            IsRequired = false,
            Arity = ArgumentArity.Zero
        };

    private readonly Option<string> DeleteReportFilesOption =
        new(new[] { "--deleteReports" })
        {
            Description = "Delete the report files generated.",
            IsRequired = false,
            Arity = ArgumentArity.Zero
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
        cmd.Add(NoBuildOption);
        cmd.Add(NoRestoreOption);
        cmd.Add(DelayOption);
        cmd.Add(BlameOption);
        cmd.Add(DeleteReportFilesOption);
    }

    public void GetValues(InvocationContext context)
    {
        Path = context.ParseResult.GetValueForArgument(PathArgument);
        Filter = context.ParseResult.GetValueForOption(FilterOption)!;
        Settings = context.ParseResult.GetValueForOption(SettingsOption)!;
        TrxLogger = context.ParseResult.GetValueForOption(LoggerOption)!;
        ResultsDirectory = context.ParseResult.GetValueForOption(ResultsDirectoryOption)!;
        RerunMaxAttempts = context.ParseResult.GetValueForOption(RerunMaxAttemptsOption);
        LogLevel = context.ParseResult.GetValueForOption(LogLevelOption);
        NoBuild = context.ParseResult.FindResultFor(NoBuildOption) is not null;
        NoRestore = context.ParseResult.FindResultFor(NoRestoreOption) is not null;
        Delay = context.ParseResult.GetValueForOption(DelayOption) * 1000;
        Blame = context.ParseResult.FindResultFor(BlameOption) is not null;
        DeleteReportFiles = context.ParseResult.FindResultFor(DeleteReportFilesOption) is not null;
    }

    public string GetArgumentList()
        => string.Concat("test ",
            $"{Path}",
            AddArguments(Filter, FilterOption),
            AddArguments(Settings, SettingsOption),
            AddArguments(TrxLogger, LoggerOption),
            AddArguments(NoBuild, NoBuildOption),
            AddArguments(NoRestore, NoRestoreOption),
            AddArguments(Blame, BlameOption));

    public string AddArguments<T>(T value, Option<T> option)
        => value is not null
            ? $" {option.Aliases.First()} \"{value}\""
            : string.Empty;

    public string AddArguments<T>(bool value, Option<T> option)
        => value
            ? $" {option.Aliases.First()}"
            : string.Empty;
}