using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Logging;
using dotnet.test.rerun.RerunCommand;
using AwesomeAssertions;
using Xunit;

namespace dotnet_test_rerun.UnitTest.RerunCommand;

public class RerunCommandConfigurationUnitTests
{
    private readonly RerunCommandConfiguration _configuration = new ();
    private Command Command = new ("test-rerun");

    [Theory]
    [InlineData("--filter", "Run tests that match the given expression.")]
    [InlineData("--settings", "The run settings file to use when running tests.")]
    [InlineData("--logger", "Specifies a logger for test results.")]
    [InlineData("--results-directory",
        "The directory where the test results will be placed.\nThe specified directory will be created if it does not exist.")]
    [InlineData("--rerunMaxAttempts", "Maximum # of attempts.")]
    [InlineData("--rerunMaxFailedTests", "Maximum # of failed tests to rerun. If exceeded, tests will not be rerun.")]
    [InlineData("--loglevel", "Log Level")]
    public void RerunCommandConfiguration_Set_ShouldConfigureOptions(string optionName, string description)
    {
        //Act
        _configuration.Set(Command);

        //Assert
        var option = Command.Options.FirstOrDefault(opt => opt.Name == optionName || opt.Aliases.Contains(optionName));
        option.Should().NotBeNull();
        option!.Description.Should().Be(description);
    }

    [Fact]
    public void RerunCommandConfiguration_GetValues_ShouldGetValuesFromInvocationContext()
    {
        //Arrange
        _configuration.Set(Command); 
        var result = Command.Parse("path --filter filter --settings settings --logger logger " +
                                               "--results-directory results-directory --rerunMaxAttempts 4 --rerunMaxFailedTests 10 --loglevel Debug");

        //Act
        _configuration.GetValues(result);

        //Assert
        _configuration.Path.Should().Be("path");
        _configuration.Filter.Should().Be("filter");
        _configuration.Settings.Should().Be("settings");
        _configuration.Logger.Should().HaveCount(1);
        _configuration.Logger.ElementAt(0).Should().Be("logger");
        _configuration.ResultsDirectory.Should().Be("results-directory");
        _configuration.RerunMaxAttempts.Should().Be(4);
        _configuration.RerunMaxFailedTests.Should().Be(10);
        _configuration.LogLevel.Should().Be(LogLevel.Debug);
    }

    [Fact]
    public void RerunCommandConfiguration_GetValues_WithMultipleLogger_ShouldGetValuesFromInvocationContext()
    {
        //Arrange
        _configuration.Set(Command); 
        var result = Command.Parse("path --filter filter --settings settings --logger logger --logger trx " +
                                               "--results-directory results-directory --rerunMaxAttempts 4 --loglevel Debug");

        //Act
        _configuration.GetValues(result);

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
        var result = Command.Parse("path --settings settings");

        //Act
        _configuration.GetValues(result);

        //Assert
        _configuration.Path.Should().Be("path");
        _configuration.Filter.Should().BeNull();
        _configuration.Settings.Should().Be("settings");
        _configuration.Logger.Should().HaveCount(1);
        _configuration.Logger.ElementAt(0).Should().Be("trx");
        _configuration.ResultsDirectory.Should().Be(".");
        _configuration.RerunMaxAttempts.Should().Be(3);
        _configuration.RerunMaxFailedTests.Should().Be(-1);
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
        var result = Command.Parse("path --filter filter --settings settings --logger logger " +
                                               "--results-directory results-directory --rerunMaxAttempts 4 --loglevel Debug " +
                                               "--configuration release --verbosity minimal");
        _configuration.GetValues(result);

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
        var result = Command.Parse("path /p:MyCustomProperty=123");
        _configuration.GetValues(result);
        
        //Act
        var args = _configuration.GetTestArgumentList("");

        //Assert
        args.Should().Be("test path --logger \"trx\" /p:MyCustomProperty=123");
    }
    
    [Theory]
    [InlineData("-maxCpuCount:3", "-maxCpuCount:3")]
    [InlineData("-m:3", "-m:3")]
    [InlineData("/maxCpuCount:3", "/maxCpuCount:3")]
    [InlineData("/m:3", "/m:3")]
    [InlineData("--maxCpuCount:3", "--maxCpuCount:3")]
    public void RerunCommandConfiguration_GetArguments_WithMsBuildMaxCpuCount(string input, string expected)
    {
        //Arrange
        _configuration.Set(Command); 
        var result = Command.Parse($"path {input}");
        _configuration.GetValues(result);
        
        //Act
        var args = _configuration.GetTestArgumentList("");

        //Assert
        args.Should().Be($"test path --logger \"trx\" {expected}");
    }
    
    [Fact]
    public void RerunCommandConfiguration_GetArguments_WithMultipleMsBuildArguments()
    {
        //Arrange
        _configuration.Set(Command); 
        var result = Command.Parse("path /p:MyCustomProperty=123 -m:3");
        _configuration.GetValues(result);
        
        //Act
        var args = _configuration.GetTestArgumentList("");

        //Assert
        args.Should().Be("test path --logger \"trx\" /p:MyCustomProperty=123 -m:3");
    }
    
    [Fact]
    public void RerunCommandConfiguration_InvalidVerbosity()
    {
        //Arrange
        _configuration.Set(Command); 
        var result = Command.Parse("path --settings settings --verbosity test");

        //Act
        var act = () => _configuration.GetValues(result);

        //Assert
        act.Should().Throw<InvalidOperationException>().WithMessage(
            "Cannot parse argument 'test' for option '--verbosity' as expected type 'dotnet.test.rerun.Enums.LoggerVerbosity'.*");
    }
    
    [Theory]
    [InlineData("filter")]
    [InlineData("filter|secondFilter")]
    [InlineData("filter|secondFilter|")]
    public void RerunCommandConfiguration_AppendFailedTests_WithOriginalFilter(string originalFilter)
    {
        //Arrange
        _configuration.Set(Command); 
        var result = Command.Parse($"path --filter {originalFilter}");
        _configuration.GetValues(result);
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
        var result = Command.Parse($"path");
        _configuration.GetValues(result);
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
        var result = Command.Parse($"path {formatArg}");

        //Act
        _configuration.GetValues(result);

        //Assert
        _configuration.MergeCoverageFormat.Should().Be(expected);
    }
    
    [Fact]
    public void RerunCommandConfiguration_GetArguments_GivenInlineRunSettings()
    {
        //Arrange
        _configuration.Set(Command); 
        var result = Command.Parse("path --filter filter --settings settings --logger logger " +
                                               "--results-directory results-directory --rerunMaxAttempts 4 --loglevel Debug " +
                                               "--configuration release --verbosity minimal --inlineRunSettings MSTest.DeploymentEnabled=false MSTest.MapInconclusiveToFailed=True");
        _configuration.GetValues(result);

        //Act
        var args = _configuration.GetTestArgumentList("results-directory");

        //Assert
        args.Should().Be("test path --filter \"filter\" --settings \"settings\" --logger \"logger\" -c \"release\" -v \"Minimal\" --results-directory \"results-directory\" -- MSTest.DeploymentEnabled=false MSTest.MapInconclusiveToFailed=True");
    }
    
    [Fact]
    public void RerunCommandConfiguration_GetArguments_WithOneEnvironmentValue()
    {
        //Arrange
        _configuration.Set(Command); 
        var result = Command.Parse("path --filter filter --settings settings --logger logger " +
                                               "--results-directory results-directory --rerunMaxAttempts 4 --loglevel Debug " +
                                               "--configuration release --verbosity minimal --environment var=test");
        _configuration.GetValues(result);

        //Act
        var args = _configuration.GetTestArgumentList("results-directory");

        //Assert
        args.Should().Be("test path --filter \"filter\" --settings \"settings\" --logger \"logger\" -c \"release\" -v \"Minimal\" -e \"var=test\" --results-directory \"results-directory\"");
    }
    
    [Fact]
    public void RerunCommandConfiguration_GetArguments_WithSeveralEnvironmentValues()
    {
        //Arrange
        _configuration.Set(Command); 
        var result = Command.Parse("path --filter filter --settings settings --logger logger " +
                                               "--results-directory results-directory --rerunMaxAttempts 4 -e var2=test2 --loglevel Debug " +
                                               "--configuration release --verbosity minimal -e var=test");
        _configuration.GetValues(result);

        //Act
        var args = _configuration.GetTestArgumentList("results-directory");

        //Assert
        args.Should().Be("test path --filter \"filter\" --settings \"settings\" --logger \"logger\" -c \"release\" -v \"Minimal\" -e \"var2=test2\" -e \"var=test\" --results-directory \"results-directory\"");
    }

}