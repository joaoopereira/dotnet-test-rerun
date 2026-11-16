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

    [Theory]
    [InlineData(ErrorCode.Success, 0)]
    [InlineData(ErrorCode.FailedTests, 1)]
    public async Task RunAsync_ReturnsTheExitCodeOfTheOutcome(ErrorCode outcome, int expectedExitCode)
    {
        // Arrange
        var dotNetTestRunner = Substitute.For<IDotNetTestRunner>();
        dotNetTestRunner.Test(Arg.Any<RerunOptions>(), Arg.Any<RunRequest>(), Arg.Any<CancellationToken>())
            .Returns(new TestRunResult(outcome, (int)outcome));
        var command = CreateCommand(Substitute.For<ILogger>(), dotNetTestRunner);

        // Act
        var exitCode = await command.RunAsync(new[] { "path", "--rerunMaxAttempts", "1" });

        // Assert
        exitCode.Should().Be(expectedExitCode);
    }

    [Fact]
    public async Task RunAsync_WhenCancelled_WarnsAndReturns130()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var command = CreateCommand(logger, ThrowingTestRunner(new OperationCanceledException()));

        // Act
        var exitCode = await command.RunAsync(new[] { "path" });

        // Assert
        exitCode.Should().Be(130);
        logger.Received(1).Warning("Test run cancelled.");
    }

    [Fact]
    public async Task RunAsync_WhenRerunExceptionIsThrown_LogsOnlyTheMessageAndReturns1()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var command = CreateCommand(logger, ThrowingTestRunner(new RerunException("dotnet test exited with code 2")));

        // Act
        var exitCode = await command.RunAsync(new[] { "path" });

        // Assert
        exitCode.Should().Be(1);
        logger.Received(1).Error("dotnet test exited with code 2");
        logger.DidNotReceive().Exception(Arg.Any<Exception>());
    }

    [Fact]
    public async Task RunAsync_WhenUnexpectedExceptionIsThrown_LogsTheExceptionAndReturns1()
    {
        // Arrange
        var logger = Substitute.For<ILogger>();
        var exception = new InvalidOperationException("unexpected");
        var command = CreateCommand(logger, ThrowingTestRunner(exception));

        // Act
        var exitCode = await command.RunAsync(new[] { "path" });

        // Assert
        exitCode.Should().Be(1);
        logger.Received(1).Exception(exception);
    }

    private static RerunRootCommand CreateCommand(ILogger logger, IDotNetTestRunner dotNetTestRunner)
        => new(logger, new RerunCliDefinition(),
            new RerunOrchestrator(logger, dotNetTestRunner, Substitute.For<IDotNetCoverageRunner>(),
                new FileSystem(), Substitute.For<ITestResultsAnalyzer>()));

    private static IDotNetTestRunner ThrowingTestRunner(Exception exception)
    {
        var dotNetTestRunner = Substitute.For<IDotNetTestRunner>();
        dotNetTestRunner.Test(Arg.Any<RerunOptions>(), Arg.Any<RunRequest>(), Arg.Any<CancellationToken>())
            .Returns<TestRunResult>(_ => throw exception);
        return dotNetTestRunner;
    }

    private static IDotNetTestRunner CreateTestRunner()
    {
        var dotNetTestRunner = Substitute.For<IDotNetTestRunner>();
        dotNetTestRunner.Test(Arg.Any<RerunOptions>(), Arg.Any<RunRequest>(), Arg.Any<CancellationToken>())
            .Returns(new TestRunResult(ErrorCode.Success, 0));
        return dotNetTestRunner;
    }
}
