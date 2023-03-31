using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using dotnet.test.rerun.Analyzers;
using dotnet.test.rerun.DotNetTestRunner;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Logging;
using dotnet.test.rerun.RerunCommand;
using Moq;
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
        var dotNetTestRunner = new Mock<IDotNetTestRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = new Mock<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new dotnet.test.rerun.RerunCommand.RerunCommand(logger, config, dotNetTestRunner.Object,
            fileSystem, testResultsAnalyzer.Object);

        dotNetTestRunner.Setup(x => x.Test(config, directoryInfo.FullName))
            .Returns(Task.CompletedTask);

        // Act
        await command.Run();

        // Assert
        dotNetTestRunner.Verify(x => x.Test(config, directoryInfo.FullName), Times.Once);
    }


    [Fact]
    public async Task InvokeCommand_TestsOnce_NoFailures()
    {
        // Arrange
        var logger = new Logger();
        var config = new RerunCommandConfiguration();
        var cmd = new Command("test-rerun");
        config.Set(cmd);
        var dotNetTestRunner = new Mock<IDotNetTestRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = new Mock<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New("results-directory");
        var command = new dotnet.test.rerun.RerunCommand.RerunCommand(logger, config, dotNetTestRunner.Object,
            fileSystem, testResultsAnalyzer.Object);

        dotNetTestRunner.Setup(x => x.Test(config, directoryInfo.FullName))
            .Returns(Task.CompletedTask);

        // Act
        await command.InvokeAsync("path --filter filter --settings settings --logger logger " +
                                  "--results-directory results-directory --rerunMaxAttempts 2 --loglevel Debug");

        // Assert
        dotNetTestRunner.Verify(x => x.Test(config, directoryInfo.FullName), Times.Once);
    }

    [Fact]
    public async Task Run_TestsFail_NoTrxFound()
    {
        // Arrange
        var logger = new Logger();
        var config = new RerunCommandConfiguration();
        InitialConfigurationSetup(config);
        var dotNetTestRunner = new Mock<IDotNetTestRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = new Mock<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new dotnet.test.rerun.RerunCommand.RerunCommand(logger, config, dotNetTestRunner.Object,
            fileSystem, testResultsAnalyzer.Object);

        dotNetTestRunner.Setup(x => x.Test(config, directoryInfo.FullName))
            .Returns(Task.CompletedTask);
        dotNetTestRunner.Setup(x => x.GetErrorCode())
            .Returns(ErrorCode.FailedTests);
        testResultsAnalyzer.SetupSequence(x => x.GetTrxFile(It.IsAny<IDirectoryInfo>()))
            .Returns((IFileInfo?)null)
            .Returns((IFileInfo?)null);

        // Act
        await command.Run();

        // Assert
        dotNetTestRunner.Verify(x => x.Test(config, directoryInfo.FullName), Times.Once);
        testResultsAnalyzer.Verify(x => x.GetTrxFile(It.IsAny<IDirectoryInfo>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Run_TestsFail_NoNewTrxFound()
    {
        // Arrange
        var logger = new Logger();
        var config = new RerunCommandConfiguration();
        InitialConfigurationSetup(config);
        var dotNetTestRunner = new Mock<IDotNetTestRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = new Mock<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new dotnet.test.rerun.RerunCommand.RerunCommand(logger, config, dotNetTestRunner.Object,
            fileSystem, testResultsAnalyzer.Object);
        var initialTrxFile = fileSystem.FileInfo.New("Initial.trx");

        dotNetTestRunner.Setup(x => x.Test(config, directoryInfo.FullName))
            .Returns(Task.CompletedTask);
        dotNetTestRunner.Setup(x => x.GetErrorCode())
            .Returns(ErrorCode.FailedTests);
        testResultsAnalyzer.SetupSequence(x => x.GetTrxFile(It.IsAny<IDirectoryInfo>()))
            .Returns(initialTrxFile)
            .Returns(initialTrxFile);

        // Act
        await command.Run();

        // Assert
        dotNetTestRunner.Verify(x => x.Test(config, directoryInfo.FullName), Times.Once);
        testResultsAnalyzer.Verify(x => x.GetTrxFile(It.IsAny<IDirectoryInfo>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Run_TestsFail_NoTestsFoundInTrxToRerun()
    {
        // Arrange
        var logger = new Logger();
        var config = new RerunCommandConfiguration();
        InitialConfigurationSetup(config);
        var dotNetTestRunner = new Mock<IDotNetTestRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = new Mock<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new dotnet.test.rerun.RerunCommand.RerunCommand(logger, config, dotNetTestRunner.Object,
            fileSystem, testResultsAnalyzer.Object);
        var firstTrxFile = fileSystem.FileInfo.New("First.trx");
        var secondTrxFile = fileSystem.FileInfo.New("Second.trx");

        dotNetTestRunner.Setup(x => x.Test(config, directoryInfo.FullName))
            .Returns(Task.CompletedTask);
        dotNetTestRunner.Setup(x => x.GetErrorCode())
            .Returns(ErrorCode.FailedTests);
        testResultsAnalyzer.SetupSequence(x => x.GetTrxFile(It.IsAny<IDirectoryInfo>()))
            .Returns(firstTrxFile)
            .Returns(secondTrxFile);
        testResultsAnalyzer.Setup(x => x.GetFailedTestsFilter(secondTrxFile))
            .Returns(String.Empty);

        // Act
        await command.Run();

        // Assert
        dotNetTestRunner.Verify(x => x.Test(config, directoryInfo.FullName), Times.Once);
        testResultsAnalyzer.Verify(x => x.GetTrxFile(It.IsAny<IDirectoryInfo>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Run_TestsFailOnFirstRun_PassOnSecond()
    {
        // Arrange
        var logger = new Logger();
        var config = new RerunCommandConfiguration();
        InitialConfigurationSetup(config);
        var dotNetTestRunner = new Mock<IDotNetTestRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = new Mock<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new dotnet.test.rerun.RerunCommand.RerunCommand(logger, config, dotNetTestRunner.Object,
            fileSystem, testResultsAnalyzer.Object);
        var firstTrxFile = fileSystem.FileInfo.New("First.trx");
        var secondTrxFile = fileSystem.FileInfo.New("Second.trx");

        dotNetTestRunner.SetupSequence(x => x.Test(config, directoryInfo.FullName))
            .Returns(Task.CompletedTask)
            .Returns(Task.CompletedTask);
        dotNetTestRunner.SetupSequence(x => x.GetErrorCode())
            .Returns(ErrorCode.FailedTests)
            .Returns(ErrorCode.Success);
        testResultsAnalyzer.SetupSequence(x => x.GetTrxFile(It.IsAny<IDirectoryInfo>()))
            .Returns(firstTrxFile)
            .Returns(secondTrxFile)
            .Returns(secondTrxFile);
        testResultsAnalyzer.Setup(x => x.GetFailedTestsFilter(secondTrxFile))
            .Returns("filterToRerun");

        // Act
        await command.Run();

        // Assert
        dotNetTestRunner.Verify(x => x.Test(config, directoryInfo.FullName), Times.Exactly(2));
        testResultsAnalyzer.Verify(x => x.GetTrxFile(It.IsAny<IDirectoryInfo>()), Times.Exactly(3));
        testResultsAnalyzer.Verify(x => x.GetFailedTestsFilter(secondTrxFile), Times.Exactly(1));
    }

    [Fact]
    public async Task Run_TestsFailOnAllTries_Failure()
    {
        // Arrange
        var logger = new Logger();
        var config = new RerunCommandConfiguration();
        InitialConfigurationSetup(config);
        var dotNetTestRunner = new Mock<IDotNetTestRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = new Mock<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new dotnet.test.rerun.RerunCommand.RerunCommand(logger, config, dotNetTestRunner.Object,
            fileSystem, testResultsAnalyzer.Object);
        var firstTrxFile = fileSystem.FileInfo.New("First.trx");
        var secondTrxFile = fileSystem.FileInfo.New("Second.trx");

        dotNetTestRunner.SetupSequence(x => x.Test(config, directoryInfo.FullName))
            .Returns(Task.CompletedTask)
            .Returns(Task.CompletedTask)
            .Returns(Task.CompletedTask);
        dotNetTestRunner.SetupSequence(x => x.GetErrorCode())
            .Returns(ErrorCode.FailedTests)
            .Returns(ErrorCode.FailedTests)
            .Returns(ErrorCode.FailedTests);
        testResultsAnalyzer.SetupSequence(x => x.GetTrxFile(It.IsAny<IDirectoryInfo>()))
            .Returns((IFileInfo?)null)
            .Returns(firstTrxFile)
            .Returns(secondTrxFile);
        testResultsAnalyzer.SetupSequence(x => x.GetFailedTestsFilter(It.IsAny<IFileInfo>()))
            .Returns("filterToRerun")
            .Returns("filterToRerun");

        // Act
        await command.Run();

        // Assert
        dotNetTestRunner.Verify(x => x.Test(config, directoryInfo.FullName), Times.Exactly(3));
        testResultsAnalyzer.Verify(x => x.GetTrxFile(It.IsAny<IDirectoryInfo>()), Times.Exactly(3));
        testResultsAnalyzer.Verify(x => x.GetFailedTestsFilter(It.IsAny<IFileInfo>()), Times.Exactly(2));
    }

    private void InitialConfigurationSetup(RerunCommandConfiguration configuration)
    {
        var command = new Command("test-rerun");
        configuration.Set(command);
        var result = new Parser(command).Parse("path --filter filter --settings settings --logger logger " +
                                               "--results-directory results-directory --rerunMaxAttempts 2 --loglevel Debug");
        var context = new InvocationContext(result);
        configuration.GetValues(context);
    }
}