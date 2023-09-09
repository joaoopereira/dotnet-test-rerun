using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using dotnet.test.rerun.Analyzers;
using dotnet.test.rerun.DotNetRunner;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Logging;
using dotnet.test.rerun.RerunCommand;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace dotnet_test_rerun.UnitTest.RerunCommand;

public class RerunCommandTests
{
    [Fact]
    public async Task Run_TestsOnce_NoFailures()
    {
        // Arrange
        var logger = new Logger();
        var config = new RerunCommandConfiguration();
        InitialConfigurationSetup(config);
        var dotNetTestRunner = Substitute.For<IDotNetTestRunner>();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new dotnet.test.rerun.RerunCommand.RerunCommand(logger, config, dotNetTestRunner,
            dotNetCoverageRunner, fileSystem, testResultsAnalyzer);

        dotNetTestRunner.Test(config, directoryInfo.FullName)
            .Returns(Task.CompletedTask);

        // Act
        await command.Run();

        // Assert
        await dotNetTestRunner.Received(1).Test(config, directoryInfo.FullName);
    }


    [Fact]
    public async Task InvokeCommand_TestsOnce_NoFailures()
    {
        // Arrange
        var logger = new Logger();
        var config = new RerunCommandConfiguration();
        var cmd = new Command("test-rerun");
        config.Set(cmd);
        var dotNetTestRunner = Substitute.For<IDotNetTestRunner>();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New("results-directory");
        var command = new dotnet.test.rerun.RerunCommand.RerunCommand(logger, config, dotNetTestRunner,
            dotNetCoverageRunner, fileSystem, testResultsAnalyzer);

        dotNetTestRunner.Test(config, directoryInfo.FullName)
            .Returns(Task.CompletedTask);

        // Act
        await command.InvokeAsync("path --filter filter --settings settings --logger logger " +
                                  "--results-directory results-directory --rerunMaxAttempts 2 --loglevel Debug");

        // Assert
        await dotNetTestRunner.Received(1).Test(config, directoryInfo.FullName);
    }

    [Fact]
    public async Task Run_TestsFail_NoTrxFound()
    {
        // Arrange
        var logger = new Logger();
        var config = new RerunCommandConfiguration();
        InitialConfigurationSetup(config);
        var dotNetTestRunner = Substitute.For<IDotNetTestRunner>();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new dotnet.test.rerun.RerunCommand.RerunCommand(logger, config, dotNetTestRunner,
            dotNetCoverageRunner, fileSystem, testResultsAnalyzer);

        dotNetTestRunner.Test(config, directoryInfo.FullName)
            .Returns(Task.CompletedTask);
        dotNetTestRunner.GetErrorCode()
            .Returns(ErrorCode.FailedTests);
        testResultsAnalyzer.GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>())
            .Returns(Array.Empty<IFileInfo>());

        // Act
        await command.Run();

        // Assert
        await dotNetTestRunner.Received(1).Test(config, directoryInfo.FullName);
        dotNetTestRunner.Received(2).GetErrorCode();
        testResultsAnalyzer.Received(1).GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>());
    }

    [Fact]
    public async Task Run_TestsFail_NoTestsFoundInTrxToRerun()
    {
        // Arrange
        var logger = new Logger();
        var config = new RerunCommandConfiguration();
        InitialConfigurationSetup(config);
        var dotNetTestRunner = Substitute.For<IDotNetTestRunner>();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new dotnet.test.rerun.RerunCommand.RerunCommand(logger, config, dotNetTestRunner,
            dotNetCoverageRunner, fileSystem, testResultsAnalyzer);
        var trxFile = fileSystem.FileInfo.New("Second.trx");

        dotNetTestRunner.Test(config, directoryInfo.FullName)
            .Returns(Task.CompletedTask);
        dotNetTestRunner.GetErrorCode()
            .Returns(ErrorCode.FailedTests);
        testResultsAnalyzer.GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>())
            .Returns(new[] { trxFile });
        testResultsAnalyzer.GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == trxFile))
            .Returns(String.Empty);

        // Act
        await command.Run();

        // Assert
        await dotNetTestRunner.Received(1).Test(config, directoryInfo.FullName);
        dotNetTestRunner.Received(2).GetErrorCode();
        testResultsAnalyzer.Received(1).GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>());
        testResultsAnalyzer.Received(1).GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == trxFile));
    }

    [Fact]
    public async Task Run_TestsFailOnFirstRun_PassOnSecond()
    {
        // Arrange
        var logger = new Logger();
        var config = new RerunCommandConfiguration();
        InitialConfigurationSetup(config);
        var dotNetTestRunner = Substitute.For<IDotNetTestRunner>();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new dotnet.test.rerun.RerunCommand.RerunCommand(logger, config, dotNetTestRunner,
            dotNetCoverageRunner, fileSystem, testResultsAnalyzer);
        var firstTrxFile = fileSystem.FileInfo.New("First.trx");
        var secondTrxFile = fileSystem.FileInfo.New("Second.trx");

        dotNetTestRunner.Test(config, directoryInfo.FullName)
            .Returns(Task.CompletedTask);
        dotNetTestRunner.GetErrorCode()
            .Returns(ErrorCode.FailedTests, ErrorCode.Success);
        testResultsAnalyzer.GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>())
            .Returns(new[] { firstTrxFile }, new[] { secondTrxFile });
        testResultsAnalyzer.GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == firstTrxFile))
            .Returns("filterToRerun");
        testResultsAnalyzer.GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == secondTrxFile))
            .Returns(string.Empty);

        // Act
        await command.Run();

        // Assert       
        await dotNetTestRunner.Received(2).Test(config, directoryInfo.FullName);
        dotNetTestRunner.Received(2).GetErrorCode();
        testResultsAnalyzer.Received(2).GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>());
        testResultsAnalyzer.Received(1).GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == firstTrxFile));
        testResultsAnalyzer.Received(1).GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == secondTrxFile));
    }

    [Fact]
    public async Task Run_TestsFailOnFirstRunWithMultipleTrxFiles_PassOnSecond()
    {
        // Arrange
        var logger = new Logger();
        var config = new RerunCommandConfiguration();
        InitialConfigurationSetup(config);
        var dotNetTestRunner = Substitute.For<IDotNetTestRunner>();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new dotnet.test.rerun.RerunCommand.RerunCommand(logger, config, dotNetTestRunner,
            dotNetCoverageRunner, fileSystem, testResultsAnalyzer);
        var firstTrxFile = fileSystem.FileInfo.New("First.trx");
        var secondTrxFile = fileSystem.FileInfo.New("Second.trx");

        dotNetTestRunner.Test(config, directoryInfo.FullName)
            .Returns(Task.CompletedTask);
        dotNetTestRunner.GetErrorCode()
            .Returns(ErrorCode.FailedTests, ErrorCode.Success);
        testResultsAnalyzer.GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>())
            .Returns(new[] { firstTrxFile, secondTrxFile }, Array.Empty<IFileInfo>());
        testResultsAnalyzer.GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == firstTrxFile && files[1] == secondTrxFile))
            .Returns("filterToRerun");

        // Act
        await command.Run();

        // Assert
        await dotNetTestRunner.Received(2).Test(config, directoryInfo.FullName);
        dotNetTestRunner.Received(2).GetErrorCode();
        testResultsAnalyzer.Received(2).GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>());
        testResultsAnalyzer.Received(1).GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == firstTrxFile && files[1] == secondTrxFile));
    }

    [Fact]
    public async Task Run_TestsFailOnFirstRun_PassOnSecond_WithDeleteReports()
    {
        // Arrange
        var logger = new Logger();
        var config = new RerunCommandConfiguration();
        InitialConfigurationSetup(config, "--deleteReports");
        var dotNetTestRunner = Substitute.For<IDotNetTestRunner>();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new dotnet.test.rerun.RerunCommand.RerunCommand(logger, config, dotNetTestRunner,
            dotNetCoverageRunner, fileSystem, testResultsAnalyzer);
        var firstTrxFile = fileSystem.FileInfo.New("First.trx");
        var secondTrxFile = fileSystem.FileInfo.New("Second.trx");
        
        dotNetTestRunner.Test(config, directoryInfo.FullName)
            .Returns(Task.CompletedTask);
        dotNetTestRunner.GetErrorCode()
            .Returns(ErrorCode.FailedTests, ErrorCode.Success);
        testResultsAnalyzer.GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>())
            .Returns(new []{ firstTrxFile },new [] { secondTrxFile });
        testResultsAnalyzer.GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == firstTrxFile))
            .Returns("filterToRerun");
        testResultsAnalyzer.GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == secondTrxFile))
            .Returns(string.Empty);
        testResultsAnalyzer.GetReportFiles()
            .Returns(new HashSet<string>() {firstTrxFile.FullName, secondTrxFile.FullName});

        // Act
        await command.Run();

        // Assert
        await dotNetTestRunner.Received(2).Test(config, directoryInfo.FullName);
        dotNetTestRunner.Received(2).GetErrorCode();
        testResultsAnalyzer.Received(2).GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>());
        testResultsAnalyzer.Received(1).GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == firstTrxFile));
        testResultsAnalyzer.Received(1).GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == secondTrxFile));
        testResultsAnalyzer.Received().GetReportFiles();
        firstTrxFile.Exists.Should().BeFalse();
        secondTrxFile.Exists.Should().BeFalse();
    }

    [Fact]
    public async Task Run_TestsFailOnAllTries_Failure()
    {
        // Arrange
        var logger = new Logger();
        var config = new RerunCommandConfiguration();
        InitialConfigurationSetup(config);
        var dotNetTestRunner = Substitute.For<IDotNetTestRunner>();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new dotnet.test.rerun.RerunCommand.RerunCommand(logger, config, dotNetTestRunner,
            dotNetCoverageRunner, fileSystem, testResultsAnalyzer);
        var firstTrxFile = fileSystem.FileInfo.New("First.trx");
        var secondTrxFile = fileSystem.FileInfo.New("Second.trx");
        
        dotNetTestRunner.Test(config, directoryInfo.FullName)
            .Returns(Task.CompletedTask);
        dotNetTestRunner.GetErrorCode()
            .Returns(ErrorCode.FailedTests);
        testResultsAnalyzer.GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>())
            .Returns(new [] {firstTrxFile},new [] {secondTrxFile});
        testResultsAnalyzer.GetFailedTestsFilter(Arg.Any<IFileInfo[]>())
            .Returns("filterToRerun");

        // Act
        await command.Run();

        // Assert
        await dotNetTestRunner.Received(3).Test(config, directoryInfo.FullName);
        dotNetTestRunner.Received(2).GetErrorCode();
        testResultsAnalyzer.Received(2).GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>());
        testResultsAnalyzer.Received(2).GetFailedTestsFilter(Arg.Any<IFileInfo[]>());
    }

    private void InitialConfigurationSetup(RerunCommandConfiguration configuration, string extraParams = "")
    {
        var command = new Command("test-rerun");
        configuration.Set(command);
        var result = new Parser(command).Parse($"path --filter filter --settings settings --logger logger " +
                                               $"--results-directory results-directory --rerunMaxAttempts 2 --loglevel Debug {extraParams}");
        var context = new InvocationContext(result);
        configuration.GetValues(context);
    }
}