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

namespace dotnet_test_rerun.UnitTest.Execution;

public class RerunOrchestratorTests
{
    [Fact]
    public async Task Run_TestsOnce_NoFailures()
    {
        // Arrange
        var logger = new Logger();
        var config = InitialConfigurationSetup();
        var dotNetTestRunner = CreateTestRunner();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new RerunOrchestrator(logger, dotNetTestRunner,
            dotNetCoverageRunner, fileSystem, testResultsAnalyzer);

        // Act
        await command.RunAsync(config);

        // Assert
        await dotNetTestRunner.Received(1).Test(config, Arg.Is<RunRequest>(r => r.ResultsDirectory == directoryInfo.FullName), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Run_TestsOnce_NoFailures_WithLogTestResults_ShouldLogTestResults()
    {
        // Arrange
        var logger = new Logger();
        var config = InitialConfigurationSetup("--logTestResults");
        var dotNetTestRunner = CreateTestRunner();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new RerunOrchestrator(logger, dotNetTestRunner,
            dotNetCoverageRunner, fileSystem, testResultsAnalyzer);
        var trxFile = fileSystem.FileInfo.New("First.trx");
        testResultsAnalyzer.GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>())
            .Returns(new[] { trxFile });

        // Act
        await command.RunAsync(config);

        // Assert
        await dotNetTestRunner.Received(1).Test(config, Arg.Is<RunRequest>(r => r.ResultsDirectory == directoryInfo.FullName), Arg.Any<CancellationToken>());
        testResultsAnalyzer.Received(1).GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>());
        testResultsAnalyzer.Received(1).LogTestResults(Arg.Is<IFileInfo[]>(files => files[0] == trxFile));
    }

    [Fact]
    public async Task Run_TestsOnce_NoFailures_WithoutLogTestResults_ShouldNotLogTestResults()
    {
        // Arrange
        var logger = new Logger();
        var config = InitialConfigurationSetup();
        var dotNetTestRunner = CreateTestRunner();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new RerunOrchestrator(logger, dotNetTestRunner,
            dotNetCoverageRunner, fileSystem, testResultsAnalyzer);

        // Act
        await command.RunAsync(config);

        // Assert
        await dotNetTestRunner.Received(1).Test(config, Arg.Is<RunRequest>(r => r.ResultsDirectory == directoryInfo.FullName), Arg.Any<CancellationToken>());
        testResultsAnalyzer.DidNotReceive().LogTestResults(Arg.Any<IFileInfo[]>());
    }

    [Fact]
    public async Task Run_TestsFailOnFirstRun_PassOnSecond_WithLogTestResults_ShouldLogTestResultsForEachRun()
    {
        // Arrange
        var logger = new Logger();
        var config = InitialConfigurationSetup("--logTestResults");
        var dotNetTestRunner = CreateTestRunner();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new RerunOrchestrator(logger, dotNetTestRunner,
            dotNetCoverageRunner, fileSystem, testResultsAnalyzer);
        var firstTrxFile = fileSystem.FileInfo.New("First.trx");
        var secondTrxFile = fileSystem.FileInfo.New("Second.trx");
        var filterToRerun = "filterToRerun";
        var testFilterCollection = new TestFilterCollection();
        testFilterCollection.Add(new TestFilter(string.Empty, new List<string>() { filterToRerun }));
        ReturnsOutcomes(dotNetTestRunner, ErrorCode.FailedTests, ErrorCode.Success);
        testResultsAnalyzer.GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>())
            .Returns(new[] { firstTrxFile }, new[] { firstTrxFile }, new[] { secondTrxFile });
        testResultsAnalyzer.GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == firstTrxFile))
            .Returns(testFilterCollection);
        testResultsAnalyzer.GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == secondTrxFile))
            .Returns(new TestFilterCollection());

        // Act
        await command.RunAsync(config);

        // Assert
        await dotNetTestRunner.Received(2).Test(config, Arg.Is<RunRequest>(r => r.ResultsDirectory == directoryInfo.FullName), Arg.Any<CancellationToken>());
        testResultsAnalyzer.Received(1).LogTestResults(Arg.Is<IFileInfo[]>(files => files[0] == firstTrxFile));
        testResultsAnalyzer.Received(1).LogTestResults(Arg.Is<IFileInfo[]>(files => files[0] == secondTrxFile));
    }


    [Fact]
    public async Task Run_TestsFail_NoTrxFound()
    {
        // Arrange
        var logger = new Logger();
        var config = InitialConfigurationSetup();
        var dotNetTestRunner = CreateTestRunner();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new RerunOrchestrator(logger, dotNetTestRunner,
            dotNetCoverageRunner, fileSystem, testResultsAnalyzer);
        ReturnsOutcomes(dotNetTestRunner, ErrorCode.FailedTests);
        testResultsAnalyzer.GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>())
            .Returns(Array.Empty<IFileInfo>());

        // Act
        await command.RunAsync(config);

        // Assert
        await dotNetTestRunner.Received(1).Test(config, Arg.Is<RunRequest>(r => r.ResultsDirectory == directoryInfo.FullName), Arg.Any<CancellationToken>());
        testResultsAnalyzer.Received(1).GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>());
    }

    [Fact]
    public async Task Run_TestsFail_NoTestsFoundInTrxToRerun()
    {
        // Arrange
        var logger = new Logger();
        var config = InitialConfigurationSetup();
        var dotNetTestRunner = CreateTestRunner();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new RerunOrchestrator(logger, dotNetTestRunner,
            dotNetCoverageRunner, fileSystem, testResultsAnalyzer);
        var trxFile = fileSystem.FileInfo.New("Second.trx");
        ReturnsOutcomes(dotNetTestRunner, ErrorCode.FailedTests);
        testResultsAnalyzer.GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>())
            .Returns(new[] { trxFile });
        testResultsAnalyzer.GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == trxFile))
            .Returns(new TestFilterCollection());

        // Act
        await command.RunAsync(config);

        // Assert
        await dotNetTestRunner.Received(1).Test(config, Arg.Is<RunRequest>(r => r.ResultsDirectory == directoryInfo.FullName), Arg.Any<CancellationToken>());
        testResultsAnalyzer.Received(1).GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>());
        testResultsAnalyzer.Received(1).GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == trxFile));
    }

    [Fact]
    public async Task Run_TestsFailOnFirstRun_PassOnSecond()
    {
        // Arrange
        var logger = new Logger();
        var config = InitialConfigurationSetup(filter: string.Empty);
        var dotNetTestRunner = CreateTestRunner();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new RerunOrchestrator(logger, dotNetTestRunner,
            dotNetCoverageRunner, fileSystem, testResultsAnalyzer);
        var firstTrxFile = fileSystem.FileInfo.New("First.trx");
        var secondTrxFile = fileSystem.FileInfo.New("Second.trx");
        var filterToRerun = "filterToRerun";
        var testFilterCollection = new TestFilterCollection();
        testFilterCollection.Add(new TestFilter(string.Empty, new List<string>() {filterToRerun }));
        ReturnsOutcomes(dotNetTestRunner, ErrorCode.FailedTests, ErrorCode.Success);
        testResultsAnalyzer.GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>())
            .Returns(new[] { firstTrxFile }, new[] { secondTrxFile });
        testResultsAnalyzer.GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == firstTrxFile))
            .Returns(testFilterCollection);
        testResultsAnalyzer.GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == secondTrxFile))
            .Returns(new TestFilterCollection());

        // Act
        await command.RunAsync(config);

        // Assert       
        await dotNetTestRunner.Received(2).Test(config, Arg.Is<RunRequest>(r => r.ResultsDirectory == directoryInfo.FullName), Arg.Any<CancellationToken>());
        await dotNetTestRunner.Received(1).Test(config, Arg.Is<RunRequest>(r => r.Filter == filterToRerun), Arg.Any<CancellationToken>());
        testResultsAnalyzer.Received(2).GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>());
        testResultsAnalyzer.Received(1).GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == firstTrxFile));
        testResultsAnalyzer.Received(1).GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == secondTrxFile));
    }

    [Fact]
    public async Task Run_TestsFailOnFirstRun_WithPreviousFilterDefined_PassOnSecond()
    {
        // Arrange
        var logger = new Logger();
        var config = InitialConfigurationSetup();
        var dotNetTestRunner = CreateTestRunner();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new RerunOrchestrator(logger, dotNetTestRunner,
            dotNetCoverageRunner, fileSystem, testResultsAnalyzer);
        var firstTrxFile = fileSystem.FileInfo.New("First.trx");
        var secondTrxFile = fileSystem.FileInfo.New("Second.trx");
        var filterToRerun = "filterToRerun";
        var testFilterCollection = new TestFilterCollection();
        testFilterCollection.Add(new TestFilter(string.Empty, new List<string>() {filterToRerun }));
        ReturnsOutcomes(dotNetTestRunner, ErrorCode.FailedTests, ErrorCode.Success);
        testResultsAnalyzer.GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>())
            .Returns(new[] { firstTrxFile }, new[] { secondTrxFile });
        testResultsAnalyzer.GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == firstTrxFile))
            .Returns(testFilterCollection);
        testResultsAnalyzer.GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == secondTrxFile))
            .Returns(new TestFilterCollection());

        // Act
        await command.RunAsync(config);

        // Assert       
        await dotNetTestRunner.Received(2).Test(config, Arg.Is<RunRequest>(r => r.ResultsDirectory == directoryInfo.FullName), Arg.Any<CancellationToken>());
        await dotNetTestRunner.Received(1).Test(config, Arg.Is<RunRequest>(r => r.Filter == string.Concat("(filter)&(", filterToRerun, ")")), Arg.Any<CancellationToken>());
        testResultsAnalyzer.Received(2).GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>());
        testResultsAnalyzer.Received(1).GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == firstTrxFile));
        testResultsAnalyzer.Received(1).GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == secondTrxFile));
    }

    [Fact]
    public async Task Run_TestsFailOnFirstRunWithMultipleTrxFiles_PassOnSecond()
    {
        // Arrange
        var logger = new Logger();
        var config = InitialConfigurationSetup();
        var dotNetTestRunner = CreateTestRunner();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new RerunOrchestrator(logger, dotNetTestRunner,
            dotNetCoverageRunner, fileSystem, testResultsAnalyzer);
        var firstTrxFile = fileSystem.FileInfo.New("First.trx");
        var secondTrxFile = fileSystem.FileInfo.New("Second.trx");
        var testFilterCollection = new TestFilterCollection();
        testFilterCollection.Add(new TestFilter(string.Empty, new List<string>() {"filterToRerun" }));
        ReturnsOutcomes(dotNetTestRunner, ErrorCode.FailedTests, ErrorCode.Success);
        testResultsAnalyzer.GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>())
            .Returns(new[] { firstTrxFile, secondTrxFile }, Array.Empty<IFileInfo>());
        testResultsAnalyzer.GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == firstTrxFile && files[1] == secondTrxFile))
            .Returns(testFilterCollection);

        // Act
        await command.RunAsync(config);

        // Assert
        await dotNetTestRunner.Received(2).Test(config, Arg.Is<RunRequest>(r => r.ResultsDirectory == directoryInfo.FullName), Arg.Any<CancellationToken>());
        testResultsAnalyzer.Received(2).GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>());
        testResultsAnalyzer.Received(1).GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == firstTrxFile && files[1] == secondTrxFile));
    }

    [Fact]
    public async Task Run_TestsFailOnFirstRun_PassOnSecond_WithDeleteReports()
    {
        // Arrange
        var logger = new Logger();
        var config = InitialConfigurationSetup("--deleteReports");
        var dotNetTestRunner = CreateTestRunner();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new RerunOrchestrator(logger, dotNetTestRunner,
            dotNetCoverageRunner, fileSystem, testResultsAnalyzer);
        var firstTrxFile = fileSystem.FileInfo.New("First.trx");
        var secondTrxFile = fileSystem.FileInfo.New("Second.trx");
        var testFilterCollection = new TestFilterCollection();
        testFilterCollection.Add(new TestFilter("net6.0", new List<string>() {"filterToRerun" }));
        ReturnsOutcomes(dotNetTestRunner, ErrorCode.FailedTests, ErrorCode.Success);
        testResultsAnalyzer.GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>())
            .Returns(new []{ firstTrxFile },new [] { secondTrxFile });
        testResultsAnalyzer.GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == firstTrxFile))
            .Returns(testFilterCollection);
        testResultsAnalyzer.GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == secondTrxFile))
            .Returns(new TestFilterCollection());

        // Act
        await command.RunAsync(config);

        // Assert
        await dotNetTestRunner.Received(2).Test(config, Arg.Is<RunRequest>(r => r.ResultsDirectory == directoryInfo.FullName), Arg.Any<CancellationToken>());
        // two attempts, plus listing the last TRX files to delete them
        testResultsAnalyzer.Received(3).GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>());
        testResultsAnalyzer.Received(1).GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == firstTrxFile));
        testResultsAnalyzer.Received(1).GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == secondTrxFile));
        firstTrxFile.Exists.Should().BeFalse();
        secondTrxFile.Exists.Should().BeFalse();
    }

    [Fact]
    public async Task Run_TestsFailOnAllTries_Failure()
    {
        // Arrange
        var logger = new Logger();
        var config = InitialConfigurationSetup();
        var dotNetTestRunner = CreateTestRunner();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new RerunOrchestrator(logger, dotNetTestRunner,
            dotNetCoverageRunner, fileSystem, testResultsAnalyzer);
        var firstTrxFile = fileSystem.FileInfo.New("First.trx");
        var secondTrxFile = fileSystem.FileInfo.New("Second.trx");
        var testFilterCollection = new TestFilterCollection();
        testFilterCollection.Add(new TestFilter("net6.0", new List<string>() {"filterToRerun" }));
        ReturnsOutcomes(dotNetTestRunner, ErrorCode.FailedTests);
        testResultsAnalyzer.GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>())
            .Returns(new [] {firstTrxFile},new [] {secondTrxFile});
        testResultsAnalyzer.GetFailedTestsFilter(Arg.Any<IFileInfo[]>())
            .Returns(testFilterCollection);

        // Act
        await command.RunAsync(config);

        // Assert
        await dotNetTestRunner.Received(3).Test(config, Arg.Is<RunRequest>(r => r.ResultsDirectory == directoryInfo.FullName), Arg.Any<CancellationToken>());
        testResultsAnalyzer.Received(2).GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>());
        testResultsAnalyzer.Received(2).GetFailedTestsFilter(Arg.Any<IFileInfo[]>());
    }

    [Fact]
    public async Task Run_TestsFailOnMultipleFrameworks_OnlyFirstFrameworkStillFails_ExitCodeIsFailure()
    {
        // Arrange
        var logger = new Logger();
        var config = InitialConfigurationSetup();
        var dotNetTestRunner = CreateTestRunner();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var fileSystem = new FileSystem();
        var command = new RerunOrchestrator(logger, dotNetTestRunner,
            Substitute.For<IDotNetCoverageRunner>(), fileSystem, testResultsAnalyzer);
        // in every attempt net8.0 still fails while net9.0, the last invocation, passes
        ReturnsOutcomes(dotNetTestRunner, ErrorCode.FailedTests,
            ErrorCode.FailedTests, ErrorCode.Success,
            ErrorCode.FailedTests, ErrorCode.Success);
        testResultsAnalyzer.GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>())
            .Returns(new[] { fileSystem.FileInfo.New("First.trx") });
        testResultsAnalyzer.GetFailedTestsFilter(Arg.Any<IFileInfo[]>())
            .Returns(MultipleFrameworksFilter());

        // Act
        var outcome = await command.RunAsync(config);

        // Assert
        outcome.Should().Be(RerunOutcome.Failed);
    }

    private static TestFilterCollection MultipleFrameworksFilter()
    {
        var testFilterCollection = new TestFilterCollection();
        testFilterCollection.Add(new TestFilter("net8.0", new List<string> { "FullyQualifiedName=A.Test" }));
        testFilterCollection.Add(new TestFilter("net9.0", new List<string> { "FullyQualifiedName=B.Test" }));
        return testFilterCollection;
    }

    [Fact]
    public async Task Run_WithMergeCoverageFormat_ShouldCallMerge()
    {
        // Arrange
        var logger = new Logger();
        var config = InitialConfigurationSetup("--mergeCoverageFormat cobertura");
        var dotNetTestRunner = CreateTestRunner();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new RerunOrchestrator(logger, dotNetTestRunner,
            dotNetCoverageRunner, fileSystem, testResultsAnalyzer);
        ReturnsOutcomes(dotNetTestRunner, ErrorCode.Success);

        // Act
        await command.RunAsync(config);

        // Assert
        await dotNetCoverageRunner.Received(1).Merge(config, directoryInfo.FullName, Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Run_WithMergeCoverageFormat_WaitsForMergeToComplete()
    {
        // Arrange
        var logger = new Logger();
        var config = InitialConfigurationSetup("--mergeCoverageFormat cobertura");
        var dotNetTestRunner = CreateTestRunner();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new RerunOrchestrator(logger, dotNetTestRunner,
            dotNetCoverageRunner, fileSystem, testResultsAnalyzer);
        var merge = new TaskCompletionSource();
        ReturnsOutcomes(dotNetTestRunner, ErrorCode.Success);
        dotNetCoverageRunner.Merge(config, directoryInfo.FullName, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(merge.Task);

        // Act
        var run = command.RunAsync(config);

        // Assert
        run.IsCompleted.Should().BeFalse();
        merge.SetResult();
        await run;
    }

    [Fact]
    public async Task Run_WithMergeCoverageFormat_MergeFails_Throws()
    {
        // Arrange
        var logger = new Logger();
        var config = InitialConfigurationSetup("--mergeCoverageFormat cobertura");
        var dotNetTestRunner = CreateTestRunner();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new RerunOrchestrator(logger, dotNetTestRunner,
            dotNetCoverageRunner, fileSystem, testResultsAnalyzer);
        ReturnsOutcomes(dotNetTestRunner, ErrorCode.Success);
        dotNetCoverageRunner.Merge(config, directoryInfo.FullName, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new RerunException("merge failed")));

        // Act
        var act = () => command.RunAsync(config);

        // Assert
        await act.Should().ThrowAsync<RerunException>().WithMessage("merge failed");
    }

   [Fact]
    public async Task Run_TestsFailExceedingThreshold_SkipsRerun()
    {
        // Arrange
        var logger = new Logger();
        var config = InitialConfigurationSetup(extraParams: "--rerunMaxFailedTests 2");
        var dotNetTestRunner = CreateTestRunner();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new RerunOrchestrator(logger, dotNetTestRunner,
            dotNetCoverageRunner, fileSystem, testResultsAnalyzer);
        var firstTrxFile = fileSystem.FileInfo.New("First.trx");
        var testFilterCollection = new TestFilterCollection();
        // Add 3 failed tests, which exceeds the threshold of 2
        testFilterCollection.Add(new TestFilter("net6.0", new List<string>() { "test1", "test2", "test3" }));
        ReturnsOutcomes(dotNetTestRunner, ErrorCode.FailedTests);
        testResultsAnalyzer.GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>())
            .Returns(new [] { firstTrxFile });
        testResultsAnalyzer.GetFailedTestsFilter(Arg.Any<IFileInfo[]>())
            .Returns(testFilterCollection);

        // Act
        await command.RunAsync(config);

        // Assert
        // Should only run the initial test, not any reruns
        await dotNetTestRunner.Received(1).Test(config, Arg.Is<RunRequest>(r => r.ResultsDirectory == directoryInfo.FullName), Arg.Any<CancellationToken>());
        testResultsAnalyzer.Received(1).GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>());
        testResultsAnalyzer.Received(1).GetFailedTestsFilter(Arg.Any<IFileInfo[]>());
    }

    [Fact]
    public async Task Run_TestsFailWithinThreshold_RunsRerun()
    {
        // Arrange
        var logger = new Logger();
        var config = InitialConfigurationSetup(extraParams: "--rerunMaxFailedTests 5");
        var dotNetTestRunner = CreateTestRunner();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new RerunOrchestrator(logger, dotNetTestRunner,
            dotNetCoverageRunner, fileSystem, testResultsAnalyzer);
        var firstTrxFile = fileSystem.FileInfo.New("First.trx");
        var secondTrxFile = fileSystem.FileInfo.New("Second.trx");
        var testFilterCollection = new TestFilterCollection();
        // Add 3 failed tests, which is within the threshold of 5
        testFilterCollection.Add(new TestFilter("net6.0", new List<string>() { "test1", "test2", "test3" }));
        ReturnsOutcomes(dotNetTestRunner, ErrorCode.FailedTests, ErrorCode.Success);
        testResultsAnalyzer.GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>())
            .Returns(new [] { firstTrxFile }, new [] { secondTrxFile });
        testResultsAnalyzer.GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == firstTrxFile))
            .Returns(testFilterCollection);
        testResultsAnalyzer.GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == secondTrxFile))
            .Returns(new TestFilterCollection());

        // Act
        await command.RunAsync(config);

        // Assert
        // Should run initial test plus one rerun
        await dotNetTestRunner.Received(2).Test(config, Arg.Is<RunRequest>(r => r.ResultsDirectory == directoryInfo.FullName), Arg.Any<CancellationToken>());
        testResultsAnalyzer.Received(2).GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>());
    }

    [Fact]
    public async Task Run_TestsFailWithThresholdDisabled_RunsRerun()
    {
        // Arrange
        var logger = new Logger();
        // Default value of -1 means no threshold
        var config = InitialConfigurationSetup();
        var dotNetTestRunner = CreateTestRunner();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new RerunOrchestrator(logger, dotNetTestRunner,
            dotNetCoverageRunner, fileSystem, testResultsAnalyzer);
        var firstTrxFile = fileSystem.FileInfo.New("First.trx");
        var secondTrxFile = fileSystem.FileInfo.New("Second.trx");
        var testFilterCollection = new TestFilterCollection();
        // Add 100 failed tests - should still rerun because threshold is disabled
        var manyTests = Enumerable.Range(1, 100).Select(i => $"test{i}").ToList();
        testFilterCollection.Add(new TestFilter("net6.0", manyTests));
        ReturnsOutcomes(dotNetTestRunner, ErrorCode.FailedTests, ErrorCode.Success);
        testResultsAnalyzer.GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>())
            .Returns(new [] { firstTrxFile }, new [] { secondTrxFile });
        testResultsAnalyzer.GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == firstTrxFile))
            .Returns(testFilterCollection);
        testResultsAnalyzer.GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == secondTrxFile))
            .Returns(new TestFilterCollection());

        // Act
        await command.RunAsync(config);

        // Assert
        // Should run initial test plus one rerun even with many failures
        await dotNetTestRunner.Received(2).Test(config, Arg.Is<RunRequest>(r => r.ResultsDirectory == directoryInfo.FullName), Arg.Any<CancellationToken>());
        testResultsAnalyzer.Received(2).GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>());
    }

    [Fact]
    public async Task Run_TestsFailExceedingPercentageThreshold_SkipsRerun()
    {
        // Arrange
        var logger = new Logger();
        var config = InitialConfigurationSetup(extraParams: "--rerunFailedThreshold 20");
        var dotNetTestRunner = CreateTestRunner();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new RerunOrchestrator(logger, dotNetTestRunner,
            dotNetCoverageRunner, fileSystem, testResultsAnalyzer);
        var firstTrxFile = fileSystem.FileInfo.New("First.trx");
        // 35 failed out of 100 total tests (35%), which exceeds the threshold of 20%
        var testFilterCollection = new TestFilterCollection();
        testFilterCollection.Add(new TestFilter("net6.0", Enumerable.Range(1, 35).Select(i => $"test{i}").ToList()));
        testFilterCollection.TotalTests = 100;
        ReturnsOutcomes(dotNetTestRunner, ErrorCode.FailedTests);
        testResultsAnalyzer.GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>())
            .Returns(new[] { firstTrxFile });
        testResultsAnalyzer.GetFailedTestsFilter(Arg.Any<IFileInfo[]>())
            .Returns(testFilterCollection);

        // Act
        await command.RunAsync(config);

        // Assert
        // Should only run the initial test, not any reruns
        await dotNetTestRunner.Received(1).Test(config, Arg.Is<RunRequest>(r => r.ResultsDirectory == directoryInfo.FullName), Arg.Any<CancellationToken>());
        testResultsAnalyzer.Received(1).GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>());
        testResultsAnalyzer.Received(1).GetFailedTestsFilter(Arg.Any<IFileInfo[]>());
    }

    [Fact]
    public async Task Run_TestsFailWithinPercentageThreshold_RunsRerun()
    {
        // Arrange
        var logger = new Logger();
        var config = InitialConfigurationSetup(extraParams: "--rerunFailedThreshold 10");
        var dotNetTestRunner = CreateTestRunner();
        var dotNetCoverageRunner = Substitute.For<IDotNetCoverageRunner>();
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var directoryInfo = fileSystem.DirectoryInfo.New(config.ResultsDirectory);
        var command = new RerunOrchestrator(logger, dotNetTestRunner,
            dotNetCoverageRunner, fileSystem, testResultsAnalyzer);
        var firstTrxFile = fileSystem.FileInfo.New("First.trx");
        var secondTrxFile = fileSystem.FileInfo.New("Second.trx");
        // 8 failed out of 100 total tests (8%), which is within the threshold of 10%
        var testFilterCollection = new TestFilterCollection();
        testFilterCollection.Add(new TestFilter("net6.0", Enumerable.Range(1, 8).Select(i => $"test{i}").ToList()));
        testFilterCollection.TotalTests = 100;
        ReturnsOutcomes(dotNetTestRunner, ErrorCode.FailedTests, ErrorCode.Success);
        testResultsAnalyzer.GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>())
            .Returns(new[] { firstTrxFile }, new[] { secondTrxFile });
        testResultsAnalyzer.GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == firstTrxFile))
            .Returns(testFilterCollection);
        testResultsAnalyzer.GetFailedTestsFilter(Arg.Is<IFileInfo[]>(files => files[0] == secondTrxFile))
            .Returns(new TestFilterCollection());

        // Act
        await command.RunAsync(config);

        // Assert
        // Should run initial test plus one rerun
        await dotNetTestRunner.Received(2).Test(config, Arg.Is<RunRequest>(r => r.ResultsDirectory == directoryInfo.FullName), Arg.Any<CancellationToken>());
        testResultsAnalyzer.Received(2).GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>());
    }
    [Theory]
    [InlineData("", new[] { ErrorCode.Success }, 1, RerunOutcome.Passed)]
    [InlineData("", new[] { ErrorCode.FailedTests, ErrorCode.Success }, 1, RerunOutcome.PassedAfterRerun)]
    [InlineData("", new[] { ErrorCode.FailedTests }, 1, RerunOutcome.Failed)]
    [InlineData("--rerunMaxFailedTests 1", new[] { ErrorCode.FailedTests }, 2, RerunOutcome.ThresholdExceeded)]
    [InlineData("--rerunFailedThreshold 10", new[] { ErrorCode.FailedTests }, 2, RerunOutcome.ThresholdExceeded)]
    [InlineData("", new[] { ErrorCode.FailedTests }, 0, RerunOutcome.NoTestResults)]
    public async Task RunAsync_ReturnsOutcome(string extraParams, ErrorCode[] outcomes, int failedTests, RerunOutcome expected)
    {
        // Arrange
        var config = InitialConfigurationSetup(extraParams);
        var dotNetTestRunner = CreateTestRunner();
        ReturnsOutcomes(dotNetTestRunner, outcomes);
        var fileSystem = new FileSystem();
        var testResultsAnalyzer = Substitute.For<ITestResultsAnalyzer>();
        var command = new RerunOrchestrator(new Logger(), dotNetTestRunner,
            Substitute.For<IDotNetCoverageRunner>(), fileSystem, testResultsAnalyzer);
        var failedTestsFilter = new TestFilterCollection { TotalTests = 4 };
        failedTestsFilter.Add(new TestFilter("net8.0", Enumerable.Range(1, failedTests).Select(i => $"test{i}").ToList()));
        testResultsAnalyzer.GetTrxFiles(Arg.Any<IDirectoryInfo>(), Arg.Any<DateTime>())
            .Returns(failedTests == 0 ? Array.Empty<IFileInfo>() : new[] { fileSystem.FileInfo.New("First.trx") });
        testResultsAnalyzer.GetFailedTestsFilter(Arg.Any<IFileInfo[]>())
            .Returns(failedTestsFilter, new TestFilterCollection());

        // Act
        var outcome = await command.RunAsync(config);

        // Assert
        outcome.Should().Be(expected);
    }

    private static IDotNetTestRunner CreateTestRunner()
    {
        var dotNetTestRunner = Substitute.For<IDotNetTestRunner>();
        ReturnsOutcomes(dotNetTestRunner, ErrorCode.Success);
        return dotNetTestRunner;
    }

    private static void ReturnsOutcomes(IDotNetTestRunner dotNetTestRunner, params ErrorCode[] outcomes)
    {
        var results = outcomes
            .Select(outcome => Task.FromResult(new TestRunResult(outcome, outcome == ErrorCode.Success ? 0 : 1)))
            .ToArray();
        dotNetTestRunner.Test(Arg.Any<RerunOptions>(), Arg.Any<RunRequest>(), Arg.Any<CancellationToken>())
            .Returns(results[0], results.Skip(1).ToArray());
    }

    private static RerunOptions InitialConfigurationSetup(string extraParams = "", string filter = "--filter filter ")
    {
        var cli = new RerunCliDefinition();
        var command = new Command("test-rerun");
        cli.AddTo(command);
        var result = command.Parse($"path {filter}--settings settings --logger logger " +
                                               $"--results-directory results-directory --rerunMaxAttempts 2 --loglevel Debug {extraParams}");
        return cli.ToOptions(result);
    }
}