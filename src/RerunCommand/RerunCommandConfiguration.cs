using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Logging;

namespace dotnet.test.rerun.RerunCommand;

public class RerunCommandConfiguration
{
    private const string MSBuildPropertyPrefix = "/p:";
    
    #region Properties

    public string? Path { get; internal set; }
    public string? Filter { get; internal set; }
    public string? Settings { get; internal set; }
    public IEnumerable<string> Logger { get; internal set; } = Array.Empty<string>();
    public string ResultsDirectory { get; internal set; } = string.Empty;
    public int RerunMaxAttempts { get; internal set; }
    public int RerunMaxFailedTests { get; internal set; }
    public LogLevel LogLevel { get; internal set; }
    public bool NoBuild { get; internal set; }
    public bool NoRestore { get; internal set; }
    public int Delay { get; internal set; }
    public bool Blame { get; internal set; }
    public bool DeleteReportFiles { get; internal set; }
    public string? Collector { get; internal set; }
    public CoverageFormat? MergeCoverageFormat { get; internal set; }
    public string? Configuration { get; internal set; }
    public LoggerVerbosity? Verbosity { get; internal set; }
    public string? Framework { get; internal set; }
    public string PArguments { get; internal set; } = string.Empty;
    public string InlineRunSettings { get; internal set; } = string.Empty;
    public IEnumerable<string>? EnvironmentVariables { get; internal set; }
    
    #endregion Properties

    #region Arguments

    private Argument<string> PathArgument = new("path")
    {
        Description = "Path to a test project .dll file.",
        Arity = ArgumentArity.ZeroOrOne
    };

    #endregion Arguments

    #region Options

    private readonly Option<string> FilterOption = new("--filter")
    {
        Description = "Run tests that match the given expression.",
        Required = false
    };

    private readonly Option<string> SettingsOption = new("--settings", "-s")
    {
        Description = "The run settings file to use when running tests.",
        Required = false
    };

    private readonly Option<IEnumerable<string>> LoggerOption = new("--logger", "-l")
    {
        Description = "Specifies a logger for test results.",
        Required = false,
        DefaultValueFactory = _ => new[] { "trx" }
    };

    private readonly Option<string> ResultsDirectoryOption =
        new("--results-directory", "-r")
        {
            Description =
                "The directory where the test results will be placed.\nThe specified directory will be created if it does not exist.",
            Required = false,
            DefaultValueFactory = _ => "."
        };

    private readonly Option<int> RerunMaxAttemptsOption = new("--rerunMaxAttempts")
    {
        Description = "Maximum # of attempts.",
        Required = false,
        DefaultValueFactory = _ => 3
    };

    private readonly Option<int> RerunMaxFailedTestsOption = new("--rerunMaxFailedTests")
    {
        Description = "Maximum # of failed tests to rerun. If exceeded, tests will not be rerun.",
        Required = false,
        DefaultValueFactory = _ => -1
    };

    private readonly Option<LogLevel> LogLevelOption =
        new("--loglevel")
        {
            Description = "Log Level",
            Required = false,
            CustomParser = Logging.Logger.ParseLogLevel,
            DefaultValueFactory = _ => LogLevel.Verbose
        };

    private readonly Option<string> NoBuildOption =
        new("--no-build")
        {
            Description = "Do not build the project before testing. Implies --no-restore.",
            Required = false,
            Arity = ArgumentArity.Zero
        };

    private readonly Option<string> NoRestoreOption =
        new("--no-restore")
        {
            Description = "Do not restore the project before building.",
            Required = false,
            Arity = ArgumentArity.Zero
        };

    private readonly Option<int> DelayOption =
        new("--delay", "-d")
        {
            Description = "Delay between test runs in seconds.",
            Required = false,
        };

    private readonly Option<string> BlameOption =
        new("--blame")
        {
            Description = "Runs the tests in blame mode.",
            Required = false,
            Arity = ArgumentArity.Zero
        };

    private readonly Option<string> DeleteReportFilesOption =
        new("--deleteReports")
        {
            Description = "Delete the report files generated.",
            Required = false,
            Arity = ArgumentArity.Zero
        };
    
    private readonly Option<string> CollectorOption =
        new("--collect")
        {
            Description =
                "Enables data collector for the test run.",
            Required = false
        };

    private readonly Option<CoverageFormat?> MergeCoverageFormatOption =
        new("--mergeCoverageFormat")
        {
            Description =
                "Output coverage format. Note: requires dotnet coverage tool to be installed.",
            Required = false
        };
    
    private readonly Option<string> ConfigurationOption =
        new("-c", "--configuration")
        {
            Description =
                "Defines the build configuration.",
            Required = false
        };
    
    private readonly Option<string> FrameworkOption =
        new("-f", "--framework")
        {
            Description =
                "Defines the target framework to run the tests.",
            Required = false
        };
    
    private readonly Option<LoggerVerbosity?> VerbosityOption =
        new("-v", "--verbosity")
        {
            Description =
                "Sets the verbosity level of the command. Possible values: Quiet, Minimal, Normal, Detailed, Diagnostic",
            Required = false,
        };
    
    private readonly Option<string[]> InlineRunSettingsOption = new("--inlineRunSettings")
    {
        Description = "Specifies the inline run settings.",
        Required = false,
        AllowMultipleArgumentsPerToken = true
    };
    
    private readonly Option<IEnumerable<string>> EnvironmentVariablesOption = new("-e", "--environment")
    {
        Description = "Sets the value of an environment variable.",
        Required = false,
        AllowMultipleArgumentsPerToken = true
    };

    #endregion Options

    private string? OriginalFilter;
    
    public void Set(Command cmd)
    {
        cmd.Add(PathArgument);
        cmd.Add(FilterOption);
        cmd.Add(SettingsOption);
        cmd.Add(LoggerOption);
        cmd.Add(ResultsDirectoryOption);
        cmd.Add(RerunMaxAttemptsOption);
        cmd.Add(RerunMaxFailedTestsOption);
        cmd.Add(LogLevelOption);
        cmd.Add(NoBuildOption);
        cmd.Add(NoRestoreOption);
        cmd.Add(DelayOption);
        cmd.Add(BlameOption);
        cmd.Add(ConfigurationOption);
        cmd.Add(FrameworkOption);
        cmd.Add(VerbosityOption);
        cmd.Add(DeleteReportFilesOption);
        cmd.Add(CollectorOption);
        cmd.Add(MergeCoverageFormatOption);
        cmd.Add(InlineRunSettingsOption);
        cmd.Add(EnvironmentVariablesOption);
    }

    public void GetValues(ParseResult parseResult)
    {
        Path = parseResult.GetValue(PathArgument);
        Filter = parseResult.GetValue(FilterOption);
        Settings = parseResult.GetValue(SettingsOption);
        Logger = parseResult.GetValue(LoggerOption)!;
        ResultsDirectory = parseResult.GetValue(ResultsDirectoryOption)!;
        RerunMaxAttempts = parseResult.GetValue(RerunMaxAttemptsOption);
        RerunMaxFailedTests = parseResult.GetValue(RerunMaxFailedTestsOption);
        LogLevel = parseResult.GetValue(LogLevelOption);
        NoBuild = parseResult.GetResult(NoBuildOption) is not null;
        NoRestore = parseResult.GetResult(NoRestoreOption) is not null;
        Delay = parseResult.GetValue(DelayOption) * 1000;
        Blame = parseResult.GetResult(BlameOption) is not null;
        Configuration = parseResult.GetValue(ConfigurationOption);
        Framework = parseResult.GetValue(FrameworkOption);
        Verbosity = parseResult.GetValue(VerbosityOption);
        DeleteReportFiles = parseResult.GetResult(DeleteReportFilesOption) is not null;
        Collector = parseResult.GetValue(CollectorOption);
        MergeCoverageFormat = parseResult.GetValue(MergeCoverageFormatOption);
        PArguments = FetchPArgumentsFromParse(parseResult);
        InlineRunSettings = FetchInlineRunSettingsFromParse(parseResult);
        EnvironmentVariables = parseResult.GetValue(EnvironmentVariablesOption);
        
        //Store Original Values
        OriginalFilter = Filter;
    }

    public string GetTestArgumentList(string resultsDirectory)
        => string.Concat("test",
            string.IsNullOrWhiteSpace(Path) ? string.Empty : $" {Path}",
            AddArguments(Filter, FilterOption),
            AddArguments(Settings, SettingsOption),
            AddArguments(Logger, LoggerOption),
            AddArguments(NoBuild, NoBuildOption),
            AddArguments(NoRestore, NoRestoreOption),
            AddArguments(Blame, BlameOption),
            AddArguments(Configuration, ConfigurationOption),
            AddArguments(Framework, FrameworkOption),
            AddArguments(Verbosity, VerbosityOption),
            AddArguments(Collector, CollectorOption),
            AddArguments(EnvironmentVariables, EnvironmentVariablesOption),
            string.IsNullOrWhiteSpace(resultsDirectory) ? resultsDirectory : AddArguments(resultsDirectory, ResultsDirectoryOption),
            GetPArguments(),
            InlineRunSettings);
    
    public string GetMergeCoverageArgumentList(string fileNames, string resultsDirectory)
        => string.Concat("merge ",
            $"-o {System.IO.Path.Combine(resultsDirectory, $"merged.{GetMergeExtension()}")} ",
            $"-f {MergeCoverageFormat} ",
            $"-r {fileNames}");
    
    public string AddArguments<T>(T value, Option<T> option)
        => value is not null
            ? $" {option.Name} \"{value}\""
            : string.Empty;
    
    public string AddArguments(string? value, Option<string> option)
        => value is not null
            ? $" {option.Name} \"{value}\""
            : string.Empty;
    
    public string AddArguments<T>(T value, Option<IEnumerable<T>> option)
        => value is not null
            ? $" {option.Name} \"{value}\""
            : string.Empty;

    public string AddArguments<T>(bool value, Option<T> option)
        => value
            ? $" {option.Name}"
            : string.Empty;
    
    public string AddArguments<T>(IEnumerable<T>? values, Option<IEnumerable<T>> option)
    {
        if (values is null)
            return string.Empty;
        
        StringBuilder str = new StringBuilder();
        foreach (var value in values)
            str.Append(AddArguments(value, option));

        return str.ToString();
    } 

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
        => string.IsNullOrWhiteSpace(PArguments) ? PArguments : $" {PArguments}";
    
    private string FetchPArgumentsFromParse(ParseResult parseResult)
        => string.Join(' ', parseResult.UnmatchedTokens.Where(p => p.StartsWith(MSBuildPropertyPrefix)));
    
    private string FetchInlineRunSettingsFromParse(ParseResult parseResult)
    {
        var inlineSettings = new StringBuilder();
        var inlineSettingsOption = parseResult.GetValue(InlineRunSettingsOption);
        var nonPArgumentTokens = parseResult.UnmatchedTokens.Where(p => !p.StartsWith(MSBuildPropertyPrefix)).ToList();

        if ((inlineSettingsOption is not null &&
            inlineSettingsOption.Length > 0) ||
            (nonPArgumentTokens is not null &&
             nonPArgumentTokens.Count > 0))
        {
            inlineSettings.Append(" -- ");
            inlineSettings.Append(string.Join(" ", inlineSettingsOption ?? []));
            inlineSettings.Append(string.Join(" ", nonPArgumentTokens ?? []));
        }

        return inlineSettings.ToString().Replace("\"", "\\\"");
    } 
}