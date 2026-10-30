using System.Diagnostics;
using System.IO.Abstractions;
using dotnet_test_rerun.Common.Utilities;
using dotnet.test.rerun;
using dotnet.test.rerun.Configuration;
using dotnet.test.rerun.DotNetRunner;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Execution;
using dotnet.test.rerun.Logging;
using AwesomeAssertions;
using NSubstitute;
using Xunit;

namespace dotnet_test_rerun.UnitTest.DotNetTestRunner;

public class DotNetTestRunnerTests
{
    [Theory]
    [InlineData(0, "No errors", "", ErrorCode.Success)]
    [InlineData(1, "", "No test source files were specified.", ErrorCode.WellKnownError)]
    // captured stderr lines are prefixed with a new line
    [InlineData(1, "", "\nNo test source files were specified.", ErrorCode.WellKnownError)]
    [InlineData(1, "FirstLine\nFailed!  - Failed:\nThirdLine", "", ErrorCode.FailedTests)]
    [InlineData(1, "Test host process crashed\nTest Run Aborted.", "", ErrorCode.FailedTests)]
    [InlineData(1, "The active test run was aborted. Reason: Test host process crashed", "", ErrorCode.FailedTests)]
    public async Task Test_ClassifiesHowDotnetTestEnded(int exitCode, string output, string error, ErrorCode expected)
    {
        // Arrange
        var processRunner = Substitute.For<IProcessRunner>();
        var dotNetTestRunner = new dotnet.test.rerun.DotNetRunner.DotNetTestRunner(new Logger(), processRunner, new FileSystem());
        processRunner.RunAsync(Arg.Any<ProcessStartInfo>(), Arg.Any<CancellationToken>())
            .Returns(new ProcessResult(exitCode, output, error));

        // Act
        var result = await dotNetTestRunner.Test(new RerunOptions(), new RunRequest(null, null, "resultsDirectory"));

        // Assert
        result.Should().Be(new TestRunResult(expected, exitCode));
    }

    [Fact]
    public async Task Test_GetErrors_UnknownError()
    {
        // Arrange
        const string errorMessage = "Unexpected error";
        var processRunner = Substitute.For<IProcessRunner>();
        var dotNetTestRunner = new dotnet.test.rerun.DotNetRunner.DotNetTestRunner(new Logger(), processRunner, new FileSystem());
        processRunner.RunAsync(Arg.Any<ProcessStartInfo>(), Arg.Any<CancellationToken>())
            .Returns(new ProcessResult(1, string.Empty, errorMessage));

        // Act
        var act = () => dotNetTestRunner.Test(new RerunOptions(), new RunRequest(null, null, "resultsDirectory"));

        // Assert
        await act.Should().ThrowAsync<RerunException>().WithMessage($"command: dotnet test --logger trx --results-directory resultsDirectory exited with code 1. \n\nError message: {errorMessage}");
    }

    [Fact]
    public async Task Test_PassesArgumentsAsAList()
    {
        // Arrange
        var processRunner = Substitute.For<IProcessRunner>();
        var dotNetTestRunner = new dotnet.test.rerun.DotNetRunner.DotNetTestRunner(new Logger(), processRunner, new FileSystem());
        processRunner.RunAsync(Arg.Any<ProcessStartInfo>(), Arg.Any<CancellationToken>())
            .Returns(new ProcessResult(0, string.Empty, string.Empty));

        // Act
        await dotNetTestRunner.Test(new RerunOptions(), new RunRequest("Name=A B", null, "results"));

        // Assert
        await processRunner.Received(1).RunAsync(
            Arg.Is<ProcessStartInfo>(psi => psi.FileName == "dotnet" &&
                                            psi.ArgumentList.SequenceEqual(new[] { "test", "--filter", "Name=A B", "--logger", "trx", "--results-directory", "results" })),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Test_NonEnglishOutput_WithTrxProduced_TreatedAsFailedTests()
    {
        // Arrange
        var resultsDirectory = TestUtilities.GetTmpDirectory();
        var processRunner = Substitute.For<IProcessRunner>();
        var dotNetTestRunner = new dotnet.test.rerun.DotNetRunner.DotNetTestRunner(new Logger(), processRunner, new FileSystem());
        processRunner.RunAsync(Arg.Any<ProcessStartInfo>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                File.WriteAllText(Path.Combine(resultsDirectory, "run.trx"), "<TestRun />");
                return new ProcessResult(1, "Com falha!  - Com falha:     1, Aprovado:     1", string.Empty);
            });

        // Act
        var result = await dotNetTestRunner.Test(new RerunOptions(), new RunRequest(null, null, resultsDirectory));

        // Assert
        result.Outcome.Should().Be(ErrorCode.FailedTests);
    }

    [Fact]
    public async Task Test_NonEnglishOutput_WithoutTrx_Throws()
    {
        // Arrange
        var resultsDirectory = TestUtilities.GetTmpDirectory();
        var processRunner = Substitute.For<IProcessRunner>();
        var dotNetTestRunner = new dotnet.test.rerun.DotNetRunner.DotNetTestRunner(new Logger(), processRunner, new FileSystem());
        processRunner.RunAsync(Arg.Any<ProcessStartInfo>(), Arg.Any<CancellationToken>())
            .Returns(new ProcessResult(1, "Erro de compilação", string.Empty));

        // Act
        var act = () => dotNetTestRunner.Test(new RerunOptions(), new RunRequest(null, null, resultsDirectory));

        // Assert
        await act.Should().ThrowAsync<RerunException>();
    }
}
