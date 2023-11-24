using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Logging;
using dotnet.test.rerun.RerunCommand;
using FluentAssertions;
using Xunit;

namespace dotnet_test_rerun.UnitTest.RerunCommand;

public class RerunCommandConfigurationUnitTests
{
    private readonly RerunCommandConfiguration _configuration = new RerunCommandConfiguration();
    private Command Command = new Command("test-rerun");

    [Theory]
    [InlineData("--filter", "Run tests that match the given expression.", false)]
    [InlineData("--settings", "The run settings file to use when running tests.", false)]
    [InlineData("--logger", "Specifies a logger for test results.", false)]
    [InlineData("--results-directory",
        "The directory where the test results will be placed.\nThe specified directory will be created if it does not exist.",
        false)]
    [InlineData("--rerunMaxAttempts", "Maximum # of attempts.", false)]
    [InlineData("--loglevel", "Log Level", false)]
    public void RerunCommandConfiguration_Set_ShouldConfigureOptions(string optionName, string description, bool isRequired)
    {
        //Act
        _configuration.Set(Command);

        //Assert
        var option = Command.Children.FirstOrDefault(x => x is Option opt && opt.HasAlias(optionName)) as Option;
        option.Should().NotBeNull();
        option!.Description.Should().Be(description);
        option!.IsRequired.Should().Be(isRequired);
    }

    [Fact]
    public void RerunCommandConfiguration_GetValues_ShouldGetValuesFromInvocationContext()
    {
        //Arrange
        _configuration.Set(Command); 
        var result = new Parser(Command).Parse("path --filter filter --settings settings --logger logger " +
                                               "--results-directory results-directory --rerunMaxAttempts 4 --loglevel Debug");
        var context = new InvocationContext(result);

        //Act
        _configuration.GetValues(context);

        //Assert
        _configuration.Path.Should().Be("path");
        _configuration.Filter.Should().Be("filter");
        _configuration.Settings.Should().Be("settings");
        _configuration.Logger.Should().HaveCount(1);
        _configuration.Logger.ElementAt(0).Should().Be("logger");
        _configuration.ResultsDirectory.Should().Be("results-directory");
        _configuration.RerunMaxAttempts.Should().Be(4);
        _configuration.LogLevel.Should().Be(LogLevel.Debug);
    }

    [Fact]
    public void RerunCommandConfiguration_GetValues_WithMultipleLogger_ShouldGetValuesFromInvocationContext()
    {
        //Arrange
        _configuration.Set(Command); 
        var result = new Parser(Command).Parse("path --filter filter --settings settings --logger logger --logger trx " +
                                               "--results-directory results-directory --rerunMaxAttempts 4 --loglevel Debug");
        var context = new InvocationContext(result);

        //Act
        _configuration.GetValues(context);

        //Assert
        _configuration.Path.Should().Be("path");
        _configuration.Filter.Should().Be("filter");
        _configuration.Settings.Should().Be("settings");
        _configuration.Logger.Should().HaveCount(2);
        _configuration.Logger.ElementAt(0).Should().Be("logger");
        _configuration.Logger.ElementAt(1).Should().Be("trx");
        _configuration.ResultsDirectory.Should().Be("results-directory");
        _configuration.RerunMaxAttempts.Should().Be(4);
        _configuration.LogLevel.Should().Be(LogLevel.Debug);
    }

    [Fact]
    public void RerunCommandConfiguration_GetValues_ShouldGetDefaultValues()
    {
        //Arrange
        _configuration.Set(Command); 
        var result = new Parser(Command).Parse("path --settings settings");
        var context = new InvocationContext(result);

        //Act
        _configuration.GetValues(context);

        //Assert
        _configuration.Path.Should().Be("path");
        _configuration.Filter.Should().BeNull();
        _configuration.Settings.Should().Be("settings");
        _configuration.Logger.Should().HaveCount(1);
        _configuration.Logger.ElementAt(0).Should().Be("trx");
        _configuration.ResultsDirectory.Should().Be(".");
        _configuration.RerunMaxAttempts.Should().Be(3);
        _configuration.LogLevel.Should().Be(LogLevel.Verbose);
        _configuration.NoBuild.Should().BeFalse();
        _configuration.NoRestore.Should().BeFalse();
        _configuration.Delay.Should().Be(0);
        _configuration.Verbosity.Should().BeNull();
        _configuration.Configuration.Should().BeNull();
        _configuration.MergeCoverageFormat.Should().BeNull();
    }

    [Fact]
    public void RerunCommandConfiguration_GetArguments_WithArguments()
    {
        //Arrange
        _configuration.Set(Command); 
        var result = new Parser(Command).Parse("path --filter filter --settings settings --logger logger " +
                                               "--results-directory results-directory --rerunMaxAttempts 4 --loglevel Debug " +
                                               "--configuration release --verbosity minimal");
        var context = new InvocationContext(result);
        _configuration.GetValues(context);

        //Act
        var args = _configuration.GetTestArgumentList("results-directory");

        //Assert
        args.Should().Be("test path --filter \"filter\" --settings \"settings\" --logger \"logger\" -c \"release\" -v \"Minimal\" --results-directory \"results-directory\"");
    }

    [Fact]
    public void RerunCommandConfiguration_GetArguments_WithNoArguments()
    {
        //Arrange
        _configuration.Set(Command); 

        //Act
        var args = _configuration.GetTestArgumentList("");

        //Assert
        args.Should().Be("test");
    }

    [Fact]
    public void RerunCommandConfiguration_GetArguments_WithPArguments()
    {
        //Arrange
        _configuration.Set(Command); 
        var result = new Parser(Command).Parse("path /p:MyCustomProperty=123");
        var context = new InvocationContext(result);
        _configuration.GetValues(context);
        
        //Act
        var args = _configuration.GetTestArgumentList("");

        //Assert
        args.Should().Be("test path --logger \"trx\" /p:MyCustomProperty=123");
    }
    
    [Fact]
    public void RerunCommandConfiguration_InvalidVerbosity()
    {
        //Arrange
        _configuration.Set(Command); 
        var result = new Parser(Command).Parse("path --settings settings --verbosity test");
        var context = new InvocationContext(result);

        //Act
        var act = () => _configuration.GetValues(context);

        //Assert
        act.Should().Throw<InvalidOperationException>().WithMessage(
            "Cannot parse argument 'test' for option '-v' as expected type 'dotnet.test.rerun.Enums.LoggerVerbosity'.");
    }
    
    [Theory]
    [InlineData("filter")]
    [InlineData("filter|secondFilter")]
    [InlineData("filter|secondFilter|")]
    public void RerunCommandConfiguration_AppendFailedTests_WithOriginalFilter(string originalFilter)
    {
        //Arrange
        _configuration.Set(Command); 
        var result = new Parser(Command).Parse($"path --filter {originalFilter}");
        var context = new InvocationContext(result);
        _configuration.GetValues(context);
        var firstTest = "firstTest";

        // Act
        var failedTests = _configuration.AppendFailedTests(firstTest);

        //Assert
        failedTests.Should().Be(string.Concat("(", originalFilter, ")", "&(", firstTest, ")"));
    }
    
    [Fact]
    public void RerunCommandConfiguration_AppendFailedTests_WithNoriginalFilter()
    {
        //Arrange
        _configuration.Set(Command); 
        var result = new Parser(Command).Parse($"path");
        var context = new InvocationContext(result);
        _configuration.GetValues(context);
        var firstTest = "firstTest";

        // Act
        var failedTests = _configuration.AppendFailedTests(firstTest);

        //Assert
        failedTests.Should().Be(firstTest);
    }

    [Theory]
    [InlineData("", null)]
    [InlineData("Coverage", CoverageFormat.Coverage)]
    [InlineData("Xml", CoverageFormat.Xml)]
    [InlineData("Cobertura", CoverageFormat.Cobertura)]
    [InlineData("coverage", CoverageFormat.Coverage)]
    [InlineData("xml", CoverageFormat.Xml)]
    [InlineData("cobertura", CoverageFormat.Cobertura)]
    public void RerunCommandConfiguration_GetValues_MergeCoverageFormat(string formatArg, CoverageFormat? expected)
    {
        if(!string.IsNullOrWhiteSpace(formatArg) )
        {
            formatArg = "--mergeCoverageFormat " + formatArg;
        }

        //Arrange
        _configuration.Set(Command);
        var result = new Parser(Command).Parse($"path {formatArg}");
        var context = new InvocationContext(result);

        //Act
        _configuration.GetValues(context);

        //Assert
        _configuration.MergeCoverageFormat.Should().Be(expected);
    }
    
    [Fact]
    public void RerunCommandConfiguration_GetArguments_GivenInlineRunSettings()
    {
        //Arrange
        _configuration.Set(Command); 
        var result = new Parser(Command).Parse("path --filter filter --settings settings --logger logger " +
                                               "--results-directory results-directory --rerunMaxAttempts 4 --loglevel Debug " +
                                               "--configuration release --verbosity minimal --inlineRunSettings MSTest.DeploymentEnabled=false MSTest.MapInconclusiveToFailed=True");
        var context = new InvocationContext(result);
        _configuration.GetValues(context);

        //Act
        var args = _configuration.GetTestArgumentList("results-directory");

        //Assert
        args.Should().Be("test path --filter \"filter\" --settings \"settings\" --logger \"logger\" -c \"release\" -v \"Minimal\" --results-directory \"results-directory\" -- MSTest.DeploymentEnabled=false MSTest.MapInconclusiveToFailed=True");
    }

}