using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using dotnet.test.rerun;
using dotnet.test.rerun.Analyzers;
using dotnet.test.rerun.Cli;
using dotnet.test.rerun.Configuration;
using dotnet.test.rerun.Execution;
using dotnet.test.rerun.Domain;
using dotnet.test.rerun.DotNetRunner;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Logging;
using AwesomeAssertions;
using NSubstitute;
using Xunit;

namespace dotnet_test_rerun.UnitTest.Cli;

public class RerunRootCommandTests
{
    [Fact]
    public async Task InvokeCommand_TestsOnce_NoFailures()
    {
        // Arrange
        var logger = new Logger();
        var dotNetTestRunner = CreateTestRunner();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New("results-directory");
        var command = new RerunRootCommand(logger, new RerunCliDefinition(),
            new RerunOrchestrator(logger, dotNetTestRunner, dotNetCoverageRunner, fileSystem, testResultsAnalyzer));

        // Act
        var parseResult = command.Parse("path --filter filter --settings settings --logger logger " +
                                  "--results-directory results-directory --rerunMaxAttempts 2 --loglevel Debug");
        await parseResult.InvokeAsync();

        // Assert
        await dotNetTestRunner.Received(1).Test(Arg.Any<RerunOptions>(), Arg.Is<RunRequest>(r => r.ResultsDirectory == directoryInfo.FullName), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void RerunCommand_ShouldAllowUnmatchedTokens()
    {
        // Arrange
        var logger = new Logger();
        var dotNetTestRunner = CreateTestRunner();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        
        // Act
        var command = new RerunRootCommand(logger, new RerunCliDefinition(),
            new RerunOrchestrator(logger, dotNetTestRunner, dotNetCoverageRunner, fileSystem, testResultsAnalyzer));

        // Assert
        command.TreatUnmatchedTokensAsErrors.Should().BeFalse();
    }

    [Theory]
    [InlineData("-m:3")]
    [InlineData("-maxCpuCount:3")]
    [InlineData("/m:3")]
    [InlineData("/maxCpuCount:3")]
    [InlineData("--maxCpuCount:3")]
    public async Task InvokeCommand_WithMsBuildArguments_ShouldPassThrough(string msbuildArg)
    {
        // Arrange
        var logger = new Logger();
        var dotNetTestRunner = CreateTestRunner();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New("results-directory");
        var command = new RerunRootCommand(logger, new RerunCliDefinition(),
            new RerunOrchestrator(logger, dotNetTestRunner, dotNetCoverageRunner, fileSystem, testResultsAnalyzer));

        // Act
        var parseResult = command.Parse($"path --no-build {msbuildArg} --results-directory results-directory --loglevel Debug");
        var result = await parseResult.InvokeAsync();

        // Assert
        result.Should().Be(0);
        await dotNetTestRunner.Received(1).Test(Arg.Is<RerunOptions>(o => o.MsBuildArguments.Contains(msbuildArg)), Arg.Any<RunRequest>(), Arg.Any<CancellationToken>());
        await dotNetTestRunner.Received(1).Test(Arg.Any<RerunOptions>(), Arg.Is<RunRequest>(r => r.ResultsDirectory == directoryInfo.FullName), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeCommand_WithMultipleMsBuildArguments_ShouldPassAllThrough()
    {
        // Arrange
        var logger = new Logger();
        var dotNetTestRunner = CreateTestRunner();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New("results-directory");
        var command = new RerunRootCommand(logger, new RerunCliDefinition(),
            new RerunOrchestrator(logger, dotNetTestRunner, dotNetCoverageRunner, fileSystem, testResultsAnalyzer));

        // Act
        var parseResult = command.Parse("path --no-build /p:MyProperty=Value -m:3 --results-directory results-directory --loglevel Debug");
        var result = await parseResult.InvokeAsync();

        // Assert
        result.Should().Be(0);
        await dotNetTestRunner.Received(1).Test(Arg.Is<RerunOptions>(o => o.MsBuildArguments.Contains("/p:MyProperty=Value")), Arg.Any<RunRequest>(), Arg.Any<CancellationToken>());
        await dotNetTestRunner.Received(1).Test(Arg.Is<RerunOptions>(o => o.MsBuildArguments.Contains("-m:3")), Arg.Any<RunRequest>(), Arg.Any<CancellationToken>());
        await dotNetTestRunner.Received(1).Test(Arg.Any<RerunOptions>(), Arg.Is<RunRequest>(r => r.ResultsDirectory == directoryInfo.FullName), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(RerunOutcome.Passed, 0)]
    [InlineData(RerunOutcome.PassedAfterRerun, 0)]
    [InlineData(RerunOutcome.Failed, 1)]
    [InlineData(RerunOutcome.ThresholdExceeded, 1)]
    [InlineData(RerunOutcome.NoTestResults, 1)]
    public void ToExitCode_MapsOutcome(RerunOutcome outcome, int exitCode)
        => RerunRootCommand.ToExitCode(outcome).Should().Be(exitCode);

    private static IDotNetTestRunner CreateTestRunner()
    {
        var dotNetTestRunner = Substitute.For<IDotNetTestRunner>();
        dotNetTestRunner.Test(Arg.Any<RerunOptions>(), Arg.Any<RunRequest>(), Arg.Any<CancellationToken>())
            .Returns(new TestRunResult(ErrorCode.Success, 0));
        return dotNetTestRunner;
    }
}
