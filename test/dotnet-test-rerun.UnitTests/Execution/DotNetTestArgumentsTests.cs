using System.CommandLine;
using dotnet.test.rerun.Cli;
using dotnet.test.rerun.Configuration;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Execution;
using AwesomeAssertions;
using Xunit;

namespace dotnet_test_rerun.UnitTest.Execution;

public class DotNetTestArgumentsTests
{
    [Fact]
    public void Build_WithArguments()
    {
        //Arrange
        var options = Parse("path --filter filter --settings settings --logger logger " +
                            "--results-directory results-directory --rerunMaxAttempts 4 --loglevel Debug " +
                            "--configuration release --verbosity minimal");

        //Act
        var args = Build(options, "results-directory");

        //Assert
        args.Should().Be("test path --filter filter --settings settings --logger logger --logger trx -c release -v Minimal --results-directory results-directory");
    }

    [Fact]
    public void Build_UsesRequestFilterAndFramework()
    {
        //Arrange
        var options = Parse("path --filter original --framework net8.0");

        //Act
        var args = DotNetTestArguments.Build(options, new RunRequest("rerun-filter", "net9.0", "results"));

        //Assert
        args.Should().Equal("test", "path", "--filter", "rerun-filter", "--logger", "trx",
            "--framework", "net9.0", "--results-directory", "results");
    }

    [Fact]
    public void Build_WithLogPassedTests_ShouldAppendConsoleLogger()
    {
        //Act
        var args = Build(Parse("path --logPassedTests"));

        //Assert
        args.Should().Be("test path --logger trx --logger console;verbosity=detailed");
    }

    [Fact]
    public void Build_WithoutLogPassedTests_ShouldNotAppendConsoleLogger()
    {
        //Act
        var args = Build(Parse("path"));

        //Assert
        args.Should().Be("test path --logger trx");
    }

    [Theory]
    [InlineData("--logger junit", "test path --logger junit --logger trx")]
    [InlineData("--logger \"trx;LogFileName=results.trx\"", "test path --logger trx;LogFileName=results.trx")]
    [InlineData("--logger TRX --logger junit", "test path --logger TRX --logger junit")]
    public void Build_AlwaysIncludesTrxLogger(string loggers, string expected)
    {
        //Act
        var args = Build(Parse($"path {loggers}"));

        //Assert
        args.Should().Be(expected);
    }

    [Fact]
    public void Build_WithLogTestResults_ShouldNotModifyArguments()
    {
        //Act
        var args = Build(Parse("path --logTestResults"));

        //Assert
        args.Should().Be("test path --logger trx");
    }

    [Fact]
    public void Build_WithLogPassedTestsAndLogTestResults_ShouldNotAppendConsoleLogger()
    {
        //Act
        var args = Build(Parse("path --logPassedTests --logTestResults"));

        //Assert
        args.Should().Be("test path --logger trx");
    }

    [Fact]
    public void Build_WithNoArguments()
    {
        //Act
        var args = Build(new RerunOptions());

        //Assert
        args.Should().Be("test --logger trx");
    }

    [Fact]
    public void Build_WithPArguments()
    {
        //Act
        var args = Build(Parse("path /p:MyCustomProperty=123"));

        //Assert
        args.Should().Be("test path --logger trx /p:MyCustomProperty=123");
    }

    [Theory]
    [InlineData("-maxCpuCount:3")]
    [InlineData("-m:3")]
    [InlineData("/maxCpuCount:3")]
    [InlineData("/m:3")]
    [InlineData("--maxCpuCount:3")]
    public void Build_WithMsBuildMaxCpuCount(string input)
    {
        //Act
        var args = Build(Parse($"path {input}"));

        //Assert
        args.Should().Be($"test path --logger trx {input}");
    }

    [Fact]
    public void Build_WithMultipleMsBuildArguments()
    {
        //Act
        var args = Build(Parse("path /p:MyCustomProperty=123 -m:3"));

        //Assert
        args.Should().Be("test path --logger trx /p:MyCustomProperty=123 -m:3");
    }

    [Fact]
    public void Build_GivenInlineRunSettings()
    {
        //Arrange
        var options = Parse("path --filter filter --settings settings --logger logger " +
                            "--results-directory results-directory --rerunMaxAttempts 4 --loglevel Debug " +
                            "--configuration release --verbosity minimal --inlineRunSettings MSTest.DeploymentEnabled=false MSTest.MapInconclusiveToFailed=True");

        //Act
        var args = Build(options, "results-directory");

        //Assert
        args.Should().Be("test path --filter filter --settings settings --logger logger --logger trx -c release -v Minimal --results-directory results-directory -- MSTest.DeploymentEnabled=false MSTest.MapInconclusiveToFailed=True");
    }

    [Fact]
    public void Build_WithOneEnvironmentValue()
    {
        //Arrange
        var options = Parse("path --filter filter --settings settings --logger logger " +
                            "--results-directory results-directory --rerunMaxAttempts 4 --loglevel Debug " +
                            "--configuration release --verbosity minimal --environment var=test");

        //Act
        var args = Build(options, "results-directory");

        //Assert
        args.Should().Be("test path --filter filter --settings settings --logger logger --logger trx -c release -v Minimal -e var=test --results-directory results-directory");
    }

    [Fact]
    public void Build_WithSeveralEnvironmentValues()
    {
        //Arrange
        var options = Parse("path --filter filter --settings settings --logger logger " +
                            "--results-directory results-directory --rerunMaxAttempts 4 -e var2=test2 --loglevel Debug " +
                            "--configuration release --verbosity minimal -e var=test");

        //Act
        var args = Build(options, "results-directory");

        //Assert
        args.Should().Be("test path --filter filter --settings settings --logger logger --logger trx -c release -v Minimal -e var2=test2 -e var=test --results-directory results-directory");
    }

    [Theory]
    [InlineData("FullyQualifiedName=My.Namespace.MyClass.My\\ Test")]
    [InlineData("FullyQualifiedName~Tests.Addresses.TestAddress\\(%2220a Manor Gardens\\)")]
    [InlineData("(Category=Integration)&(FullyQualifiedName=A.B|FullyQualifiedName=C.D)")]
    [InlineData("FullyQualifiedName=Path\\With\\Trailing\\")]
    public void Build_KeepsFilterAsASingleArgumentUnchanged(string filter)
    {
        //Act
        var args = DotNetTestArguments.Build(new RerunOptions(), new RunRequest(filter, null, string.Empty));

        //Assert
        args.Should().Equal("test", "--filter", filter, "--logger", "trx");
    }

    [Fact]
    public void Build_KeepsInlineRunSettingsWithQuotesAndSpacesAsSingleArguments()
    {
        //Arrange
        var options = new RerunOptions
        {
            InlineRunSettings = new[] { "TestRunParameters.Parameter(name=\"url\", value=\"http://a b\")" }
        };

        //Act
        var args = DotNetTestArguments.Build(options, new RunRequest(null, null, string.Empty));

        //Assert
        args.Should().Equal("test", "--logger", "trx", "--",
            "TestRunParameters.Parameter(name=\"url\", value=\"http://a b\")");
    }

    [Theory]
    [InlineData(CoverageFormat.Cobertura, "merged.cobertura.xml", "cobertura")]
    [InlineData(CoverageFormat.Xml, "merged.xml", "xml")]
    [InlineData(CoverageFormat.Coverage, "merged.coverage", "coverage")]
    public void CoverageMergeArguments_Build(CoverageFormat format, string outputFile, string formatArgument)
    {
        //Act
        var args = CoverageMergeArguments.Build(new RerunOptions { MergeCoverageFormat = format },
            new[] { "/r/a.coverage", "/r/b.coverage" }, "/r");

        //Assert
        args.Should().Equal("merge", "-o", Path.Combine("/r", outputFile), "-f", formatArgument,
            "/r/a.coverage", "/r/b.coverage");
    }

    [Theory]
    [InlineData(null, "failed", "failed")]
    [InlineData("", "failed", "failed")]
    [InlineData("filter", "failed", "(filter)&(failed)")]
    [InlineData("filter|secondFilter", "failed", "(filter|secondFilter)&(failed)")]
    public void FilterComposer_Combine(string? original, string failed, string expected)
        => FilterComposer.Combine(original, failed).Should().Be(expected);

    [Fact]
    public void CommandLineDisplay_QuotesOnlyArgumentsThatNeedIt()
        => CommandLineDisplay.Render("dotnet", new[] { "test", "--filter", "A=B c", "--logger", "console;verbosity=detailed", "" })
            .Should().Be("dotnet test --filter \"A=B c\" --logger \"console;verbosity=detailed\" \"\"");

    private static RerunOptions Parse(string commandLine)
    {
        var cli = new RerunCliDefinition();
        var command = new RootCommand { TreatUnmatchedTokensAsErrors = false };
        cli.AddTo(command);
        return cli.ToOptions(command.Parse(commandLine));
    }

    private static string Build(RerunOptions options, string resultsDirectory = "")
        => string.Join(' ', DotNetTestArguments.Build(options, new RunRequest(options.Filter, options.Framework, resultsDirectory)));
}
