using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Logging;

namespace dotnet.test.rerun.RerunCommand;

public class RerunCommandConfiguration
{
    #region Properties

    public string? Path { get; internal set; }
    public string? Filter { get; internal set; }
    public string? Settings { get; internal set; }
    public IEnumerable<string> Logger { get; internal set; } = [];
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
    public string ExtraArguments { get; internal set; } = string.Empty;
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

    private readonly Option<LogLevel> LogLevelOption = new("--loglevel")
    {
        Description = "Log Level",
        DefaultValueFactory = _ => LogLevel.Verbose,
        CustomParser = Logging.Logger.ParseLogLevel
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

    #endregion Options

    private string? OriginalFilter;
    
    public void Set(Command cmd)
    {
        cmd.Arguments.Add(PathArgument);
        cmd.Options.Add(FilterOption);
        cmd.Options.Add(SettingsOption);
        cmd.Options.Add(LoggerOption);
        cmd.Options.Add(ResultsDirectoryOption);
        cmd.Options.Add(RerunMaxAttemptsOption);
        cmd.Options.Add(RerunMaxFailedTestsOption);
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
        NoBuild = parseResult.GetValue(NoBuildOption);
        NoRestore = parseResult.GetValue(NoRestoreOption);
        Delay = parseResult.GetValue(DelayOption) * 1000;
        Blame = parseResult.GetValue(BlameOption);
        Configuration = parseResult.GetValue(ConfigurationOption);
        Framework = parseResult.GetValue(FrameworkOption);
        Verbosity = parseResult.GetValue(VerbosityOption);
        DeleteReportFiles = parseResult.GetValue(DeleteReportFilesOption);
        Collector = parseResult.GetValue(CollectorOption);
        MergeCoverageFormat = parseResult.GetValue(MergeCoverageFormatOption);
        ExtraArguments = FetchExtraArgumentsFromParse(parseResult);
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
            GetExtraArguments(),
            InlineRunSettings);
    
    public string GetMergeCoverageArgumentList(string fileNames, string resultsDirectory)
        => string.Concat("merge ",
            $"-o {System.IO.Path.Combine(resultsDirectory, $"merged.{GetMergeExtension()}")} ",
            $"-f {MergeCoverageFormat} ",
            $"-r {fileNames}");
    
    private static string GetOptionName(Option option)
    {
        // Use short aliases for specific commonly-used options to keep command lines shorter
        // Otherwise use the full name for clarity
        return option.Name switch
        {
            "--configuration" => "-c",
            "--verbosity" => "-v",
            "--environment" => "-e",
            _ => option.Name
        };
    }
    
    public string AddArguments<T>(T value, Option<T> option)
        => value is not null
            ? $" {GetOptionName(option)} \"{value}\""
            : string.Empty;
    
    public string AddArguments(string? value, Option<string> option)
        => value is not null
            ? $" {GetOptionName(option)} \"{value}\""
            : string.Empty;
    
    public string AddArguments<T>(T value, Option<IEnumerable<T>> option)
        => value is not null
            ? $" {GetOptionName(option)} \"{value}\""
            : string.Empty;

    public string AddArguments<T>(bool value, Option<T> option)
        => value
            ? $" {GetOptionName(option)}"
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

    private string GetExtraArguments()
        => string.IsNullOrWhiteSpace(ExtraArguments) ? ExtraArguments : $" {ExtraArguments}";
    
    private string FetchExtraArgumentsFromParse(ParseResult parseResult)
        => string.Join(' ', parseResult.UnmatchedTokens.Where(IsMsBuildArgument));
    
    private static readonly string[] MsBuildArgumentPrefixes = 
    [
        "/p:", "-p:", "--property:", "/property:", "-property:",
        "/m:", "-m:", "/maxCpuCount:", "-maxCpuCount:", "--maxCpuCount:"
    ];
    
    private static bool IsMsBuildArgument(string token)
        => MsBuildArgumentPrefixes.Any(prefix => token.StartsWith(prefix));
    
    private string FetchInlineRunSettingsFromParse(ParseResult parseResult)
    {
        var inlineSettings = new StringBuilder();
        var inlineSettingsOption = parseResult.GetValue(InlineRunSettingsOption);
        
        // Get all unmatched tokens that are not MSBuild arguments
        var unmatchedNonMsBuildTokens = parseResult.UnmatchedTokens.Where(t => !IsMsBuildArgument(t)).ToList();

        if ((inlineSettingsOption is not null &&
            inlineSettingsOption.Length > 0) ||
            unmatchedNonMsBuildTokens.Count > 0)
        {
            inlineSettings.Append(" -- ");
            inlineSettings.Append(string.Join(" ", inlineSettingsOption ?? []));
            if (unmatchedNonMsBuildTokens.Count > 0)
            {
                inlineSettings.Append(' ');
                inlineSettings.Append(string.Join(" ", unmatchedNonMsBuildTokens));
            }
        }

        return inlineSettings.ToString().Replace("\"", "\\\"");
    } 
}
