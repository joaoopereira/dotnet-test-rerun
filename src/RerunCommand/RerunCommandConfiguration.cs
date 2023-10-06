using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Logging;

namespace dotnet.test.rerun.RerunCommand;

public class RerunCommandConfiguration
{
    #region Properties

    public string Path { get; internal set; }
    public string Filter { get; internal set; }
    public string Settings { get; internal set; }
    public string TrxLogger { get; internal set; }
    public string ResultsDirectory { get; internal set; }
    public int RerunMaxAttempts { get; internal set; }
    public LogLevel LogLevel { get; internal set; }
    public bool NoBuild { get; internal set; }
    public bool NoRestore { get; internal set; }
    public int Delay { get; internal set; }
    public bool Blame { get; internal set; }
    public bool DeleteReportFiles { get; internal set; }
    public string Collector { get; internal set; }
    public CoverageFormat? MergeCoverageFormat { get; internal set; }
    public string Configuration { get; internal set; }
    public LoggerVerbosity? Verbosity { get; internal set; }
    public string pArguments { get; internal set; }
    
    #endregion Properties

    #region Arguments

    private Argument<string> PathArgument = new("path")
    {
        Description = "Path to a test project .dll file.",
        Arity = ArgumentArity.ZeroOrOne
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
    
    private readonly Option<string> CollectorOption =
        new(new[] { "--collect" })
        {
            Description =
                "Enables data collector for the test run.",
            IsRequired = false
        };

    private readonly Option<CoverageFormat?> MergeCoverageFormatOption =
        new(new[] { "--mergeCoverageFormat" })
        {
            Description =
                "Output coverage format. Note: requires dotnet coverage tool to be installed.",
            IsRequired = false
        };
    
    private readonly Option<string> ConfigurationOption =
        new(new[] { "-c", "--configuration" })
        {
            Description =
                "Defines the build configuration.",
            IsRequired = false
        };
    
    private readonly Option<LoggerVerbosity?> VerbosityOption =
        new(new[] { "-v", "--verbosity" })
        {
            Description =
                "Sets the verbosity level of the command. Possible values: Quiet, Minimal, Normal, Detailed, Diagnostic",
            IsRequired = false,
        };

    #endregion Options

    private string OriginalFilter;
    
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
        cmd.Add(ConfigurationOption);
        cmd.Add(VerbosityOption);
        cmd.Add(DeleteReportFilesOption);
        cmd.Add(CollectorOption);
        cmd.Add(MergeCoverageFormatOption);
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
        Configuration = context.ParseResult.GetValueForOption(ConfigurationOption)!;
        Verbosity = context.ParseResult.GetValueForOption(VerbosityOption);
        DeleteReportFiles = context.ParseResult.FindResultFor(DeleteReportFilesOption) is not null;
        Collector = context.ParseResult.GetValueForOption(CollectorOption)!;
        MergeCoverageFormat = context.ParseResult.GetValueForOption(MergeCoverageFormatOption);
        pArguments = FetchPArgumentsFromParse(context.ParseResult);
        
        //Store Original Values
        OriginalFilter = Filter;
    }

    public string GetTestArgumentList(string resultsDirectory)
        => string.Concat("test",
            string.IsNullOrWhiteSpace(Path) ? string.Empty : $" {Path}",
            AddArguments(Filter, FilterOption),
            AddArguments(Settings, SettingsOption),
            AddArguments(TrxLogger, LoggerOption),
            AddArguments(NoBuild, NoBuildOption),
            AddArguments(NoRestore, NoRestoreOption),
            AddArguments(Blame, BlameOption),
            AddArguments(Configuration, ConfigurationOption),
            AddArguments(Verbosity, VerbosityOption),
            AddArguments(Collector, CollectorOption),
            string.IsNullOrWhiteSpace(resultsDirectory) ? resultsDirectory : AddArguments(resultsDirectory, ResultsDirectoryOption),
            GetPArguments());
    
    public string GetMergeCoverageArgumentList(string fileNames, string resultsDirectory)
        => string.Concat("merge ",
            $"-o {System.IO.Path.Combine(resultsDirectory, $"merged.{GetMergeExtension()}")} ",
            $"-f {MergeCoverageFormat} ",
            $"-r {fileNames}");
    
    public string AddArguments<T>(T value, Option<T> option)
        => value is not null
            ? $" {option.Aliases.First()} \"{value}\""
            : string.Empty;

    public string AddArguments<T>(bool value, Option<T> option)
        => value
            ? $" {option.Aliases.First()}"
            : string.Empty;

    public string AppendFailedTests(string failedTests)
        => string.IsNullOrWhiteSpace(OriginalFilter) ? 
            failedTests : 
            string.Concat("(", OriginalFilter, ")", "&(", failedTests, ")");
    
    private string GetMergeExtension()
        => MergeCoverageFormat switch
        {
            CoverageFormat.Cobertura => "cobertura.xml",
            CoverageFormat.Xml => "xml",
            CoverageFormat.Coverage => "coverage",
            _ => throw new ArgumentOutOfRangeException(nameof(MergeCoverageFormat), MergeCoverageFormat, null)
        };

    private string GetPArguments()
        => string.IsNullOrWhiteSpace(pArguments) ? pArguments : $" {pArguments}";
    
    private string FetchPArgumentsFromParse(ParseResult parseResult)
        => string.Join(' ', parseResult.UnmatchedTokens.Where(p => p.StartsWith("/p:")));

}