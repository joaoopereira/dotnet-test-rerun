using System.CommandLine;
using System.CommandLine.Parsing;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Logging;
using dotnet.test.rerun.Cli;
using dotnet.test.rerun.Configuration;
using AwesomeAssertions;
using Xunit;

namespace dotnet_test_rerun.UnitTest.Cli;

public class RerunCliDefinitionTests
{
    private readonly RerunCliDefinition _cli = new ();
    private Command Command = new ("test-rerun");

    [Theory]
    [InlineData("--filter", "Run tests that match the given expression.")]
    [InlineData("--settings", "The run settings file to use when running tests.")]
    [InlineData("--logger", "Specifies a logger for test results.")]
    [InlineData("--results-directory",
        "The directory where the test results will be placed.\nThe specified directory will be created if it does not exist.")]
    [InlineData("--rerunMaxAttempts", "Maximum # of attempts.")]
    [InlineData("--rerunMaxFailedTests", "Maximum # of failed tests to rerun. If exceeded, tests will not be rerun.")]
    [InlineData("--rerunFailedThreshold", "Maximum percentage of failed tests allowed to trigger a rerun. If exceeded, tests will not be rerun.")]
    [InlineData("--loglevel", "Log Level")]
    [InlineData("--logPassedTests", "Logs each passed test as it completes during execution. This can produce a lot of output.")]
    [InlineData("--logTestResults", "Logs the result (passed/failed) of each individual test, independently of the dotnet test verbosity.")]
    public void RerunCliDefinition_AddTo_ShouldConfigureOptions(string optionName, string description)
    {
        //Act
        _cli.AddTo(Command);

        //Assert
        var option = Command.Options.FirstOrDefault(opt => opt.Name == optionName || opt.Aliases.Contains(optionName));
        option.Should().NotBeNull();
        option!.Description.Should().Be(description);
    }

    [Fact]
    public void RerunCliDefinition_ToOptions_ShouldGetValuesFromInvocationContext()
    {
        //Arrange
        _cli.AddTo(Command); 
        var result = Command.Parse("path --filter filter --settings settings --logger logger " +
                                               "--results-directory results-directory --rerunMaxAttempts 4 --rerunMaxFailedTests 10 --rerunFailedThreshold 20 --loglevel Debug");

        //Act
        var options = _cli.ToOptions(result);

        //Assert
        options.Path.Should().Be("path");
        options.Filter.Should().Be("filter");
        options.Settings.Should().Be("settings");
        options.Loggers.Should().HaveCount(1);
        options.Loggers.ElementAt(0).Should().Be("logger");
        options.ResultsDirectory.Should().Be("results-directory");
        options.RerunMaxAttempts.Should().Be(4);
        options.RerunMaxFailedTests.Should().Be(10);
        options.RerunFailedThreshold.Should().Be(20);
        options.LogLevel.Should().Be(LogLevel.Debug);
    }

    [Fact]
    public void RerunCliDefinition_ToOptions_WithMultipleLogger_ShouldGetValuesFromInvocationContext()
    {
        //Arrange
        _cli.AddTo(Command); 
        var result = Command.Parse("path --filter filter --settings settings --logger logger --logger trx " +
                                               "--results-directory results-directory --rerunMaxAttempts 4 --loglevel Debug");

        //Act
        var options = _cli.ToOptions(result);

        //Assert
        options.Path.Should().Be("path");
        options.Filter.Should().Be("filter");
        options.Settings.Should().Be("settings");
        options.Loggers.Should().HaveCount(2);
        options.Loggers.ElementAt(0).Should().Be("logger");
        options.Loggers.ElementAt(1).Should().Be("trx");
        options.ResultsDirectory.Should().Be("results-directory");
        options.RerunMaxAttempts.Should().Be(4);
        options.LogLevel.Should().Be(LogLevel.Debug);
    }

    [Fact]
    public void RerunCliDefinition_ToOptions_ShouldGetDefaultValues()
    {
        //Arrange
        _cli.AddTo(Command); 
        var result = Command.Parse("path --settings settings");

        //Act
        var options = _cli.ToOptions(result);

        //Assert
        options.Path.Should().Be("path");
        options.Filter.Should().BeNull();
        options.Settings.Should().Be("settings");
        options.Loggers.Should().HaveCount(1);
        options.Loggers.ElementAt(0).Should().Be("trx");
        options.ResultsDirectory.Should().Be(".");
        options.RerunMaxAttempts.Should().Be(3);
        options.RerunMaxFailedTests.Should().Be(-1);
        options.RerunFailedThreshold.Should().Be(-1);
        options.LogLevel.Should().Be(LogLevel.Verbose);
        options.NoBuild.Should().BeFalse();
        options.NoRestore.Should().BeFalse();
        options.Delay.Should().Be(TimeSpan.Zero);
        options.Verbosity.Should().BeNull();
        options.Configuration.Should().BeNull();
        options.MergeCoverageFormat.Should().BeNull();
        options.LogPassedTests.Should().BeFalse();
        options.LogTestResults.Should().BeFalse();
    }

    [Fact]
    public void RerunCliDefinition_ToOptions_LogPassedTests_ShouldBeTrueWhenPassed()
    {
        //Arrange
        _cli.AddTo(Command);
        var result = Command.Parse("path --logPassedTests");

        //Act
        var options = _cli.ToOptions(result);

        //Assert
        options.LogPassedTests.Should().BeTrue();
    }

    [Fact]
    public void RerunCliDefinition_ToOptions_LogTestResults_ShouldBeTrueWhenPassed()
    {
        //Arrange
        _cli.AddTo(Command);
        var result = Command.Parse("path --logTestResults");

        //Act
        var options = _cli.ToOptions(result);

        //Assert
        options.LogTestResults.Should().BeTrue();
    }

    [Fact]
    public void RerunCliDefinition_InvalidVerbosity()
    {
        //Arrange
        _cli.AddTo(Command); 
        var result = Command.Parse("path --settings settings --verbosity test");

        //Act
        var act = () => _cli.ToOptions(result);

        //Assert
        act.Should().Throw<InvalidOperationException>().WithMessage(
            "Cannot parse argument 'test' for option '--verbosity' as expected type 'dotnet.test.rerun.Enums.LoggerVerbosity'.*");
    }
    
    [Theory]
    [InlineData("", null)]
    [InlineData("Coverage", CoverageFormat.Coverage)]
    [InlineData("Xml", CoverageFormat.Xml)]
    [InlineData("Cobertura", CoverageFormat.Cobertura)]
    [InlineData("coverage", CoverageFormat.Coverage)]
    [InlineData("xml", CoverageFormat.Xml)]
    [InlineData("cobertura", CoverageFormat.Cobertura)]
    public void RerunCliDefinition_ToOptions_MergeCoverageFormat(string formatArg, CoverageFormat? expected)
    {
        if(!string.IsNullOrWhiteSpace(formatArg) )
        {
            formatArg = "--mergeCoverageFormat " + formatArg;
        }

        //Arrange
        _cli.AddTo(Command);
        var result = Command.Parse($"path {formatArg}");

        //Act
        var options = _cli.ToOptions(result);

        //Assert
        options.MergeCoverageFormat.Should().Be(expected);
    }

    [Fact]
    public void RerunCliDefinition_ToOptions_SplitsMsBuildArgumentsAndInlineRunSettings()
    {
        //Arrange
        _cli.AddTo(Command);
        var result = Command.Parse("path /p:MyCustomProperty=123 -m:3 --inlineRunSettings MSTest.DeploymentEnabled=false -- RunConfiguration.TreatNoTestsAsError=true");

        //Act
        var options = _cli.ToOptions(result);

        //Assert
        options.MsBuildArguments.Should().Equal("/p:MyCustomProperty=123", "-m:3");
        options.InlineRunSettings.Should().Equal("MSTest.DeploymentEnabled=false", "RunConfiguration.TreatNoTestsAsError=true");
    }

    [Fact]
    public void RerunCliDefinition_ToOptions_DelayIsInSeconds()
    {
        //Arrange
        _cli.AddTo(Command);
        var result = Command.Parse("path --delay 5");

        //Act
        var options = _cli.ToOptions(result);

        //Assert
        options.Delay.Should().Be(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void RerunCliDefinition_ToOptions_DefaultsMatchRerunOptionsDefaults()
    {
        //Arrange
        _cli.AddTo(Command);
        var result = Command.Parse("");

        //Act
        var options = _cli.ToOptions(result);

        //Assert
        options.Should().BeEquivalentTo(new RerunOptions());
    }
}
