using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
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
        _configuration.TrxLogger.Should().Be("logger");
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
        _configuration.TrxLogger.Should().Be("trx");
        _configuration.ResultsDirectory.Should().Be(".");
        _configuration.RerunMaxAttempts.Should().Be(3);
        _configuration.LogLevel.Should().Be(LogLevel.Verbose);
        _configuration.NoBuild.Should().BeFalse();
        _configuration.NoRestore.Should().BeFalse();
        _configuration.Delay.Should().Be(0);
    }

    [Fact]
    public void RerunCommandConfiguration_GetArguments_WithArguments()
    {
        //Arrange
        _configuration.Set(Command); 
        var result = new Parser(Command).Parse("path --filter filter --settings settings --logger logger " +
                                               "--results-directory results-directory --rerunMaxAttempts 4 --loglevel Debug");
        var context = new InvocationContext(result);
        _configuration.GetValues(context);

        //Act
        var args = _configuration.GetArgumentList();

        //Assert
        args.Should().Be("test path --filter \"filter\" --settings \"settings\" --logger \"logger\"");
    }

    [Fact]
    public void RerunCommandConfiguration_GetArguments_WithNoArguments()
    {
        //Arrange
        _configuration.Set(Command); 

        //Act
        var args = _configuration.GetArgumentList();

        //Assert
        args.Should().Be("test ");
    }
}