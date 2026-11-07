using System.CommandLine;
using dotnet.test.rerun.Cli;
using dotnet.test.rerun.Configuration;
using dotnet.test.rerun.Execution;
using AwesomeAssertions;
using Xunit;

namespace dotnet_test_rerun.UnitTest.Execution;

public class FailedTestsCommandTests
{
    [Fact]
    public void Build_KeepsSettingsConfigurationAndFramework()
    {
        //Arrange
        var options = Parse("path --settings settings --logger logger -c Release -f net8.0 " +
                            "--results-directory results-directory --no-build --rerunMaxAttempts 2 --generateTestCommand");

        //Act
        var command = FailedTestsCommand.Build(options, "FullyQualifiedName=Tests.TestA | FullyQualifiedName=Tests.TestB");

        //Assert
        command.Should().Be("dotnet test path --filter \"FullyQualifiedName=Tests.TestA | FullyQualifiedName=Tests.TestB\" " +
                            "--settings settings -c Release --framework net8.0");
    }

    [Fact]
    public void Build_WithOriginalFilter_CombinesFilters()
    {
        //Arrange
        var options = Parse("--filter TestCategory=Integration");

        //Act
        var command = FailedTestsCommand.Build(options, "FullyQualifiedName=Tests.TestA");

        //Assert
        command.Should().Be("dotnet test --filter \"(TestCategory=Integration)&(FullyQualifiedName=Tests.TestA)\"");
    }

    [Fact]
    public void Build_WithSingleTest_DoesNotQuoteFilter()
    {
        //Act
        var command = FailedTestsCommand.Build(new RerunOptions { Path = "path" }, "FullyQualifiedName=Tests.TestA");

        //Assert
        command.Should().Be("dotnet test path --filter FullyQualifiedName=Tests.TestA");
    }

    private static RerunOptions Parse(string commandLine)
    {
        var cli = new RerunCliDefinition();
        var command = new Command("test-rerun");
        cli.AddTo(command);
        return cli.ToOptions(command.Parse(commandLine));
    }
}
