using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO.Abstractions;
using dotnet_test_rerun.IntegrationTests.Utilities;
using dotnet.test.rerun.Analyzers;
using dotnet.test.rerun.DotNetTestRunner;
using dotnet.test.rerun.Logging;
using dotnet.test.rerun.RerunCommand;
using FluentAssertions;

namespace dotnet_test_rerun.IntegrationTests;

public class DotNetTestRerunTests
{
    private static string _dir = TestUtilities.GetTmpDirectory();
    private static readonly IFileSystem FileSystem = new FileSystem();

    public DotNetTestRerunTests()
    {
        TestUtilities.CopyFixture(string.Empty, new DirectoryInfo(_dir));
    }

    [Fact]
    public async Task DotnetTestRerun_RunXUnitExample_Success()
    {
        // Arrange
        Environment.ExitCode = 0;

        // Act
        var output = await RunDotNetTestRerunAndCollectOutputMessage("XUnitExample");

        // Assert
        output.Should().Contain("Passed!", Exactly.Once());
        output.Should().NotContainAny(new string[] {"Failed!", "Rerun attempt"});
        Environment.ExitCode.Should().Be(0);
    }

    [Fact]
    public async Task DotnetTestRerun_RunMSTestExample_Success()
    {
        // Arrange
        Environment.ExitCode = 0;

        // Act
        var output = await RunDotNetTestRerunAndCollectOutputMessage("MSTestExample");

        // Assert
        output.Should().Contain("Passed!", Exactly.Once());
        output.Should().NotContainAny(new string[] {"Failed!", "Rerun attempt"});
        Environment.ExitCode.Should().Be(0);
    }

    [Fact]
    public async Task DotnetTestRerun_RunMSTestExample_RunningProcess_Success()
    {        
        // Arrange
        Environment.ExitCode = 0;

        // Arrange
        Process process = new Process();
        process.StartInfo.FileName = "test-rerun";
        process.StartInfo.Arguments = $"{_dir}\\MSTestExample --rerunMaxAttempts 3 --results-directory {_dir}";
        
        // Act
        process.Start();

        // Assert
        await process.WaitForExitAsync();
        process.ExitCode.Should().Be(0);
    }

    [Fact]
    public async Task DotnetTestRerun_FailingMSTest_Fails()
    {
        // Arrange
        Environment.ExitCode = 0;

        // Act
        var output = await RunDotNetTestRerunAndCollectOutputMessage("FailingMSTestExample");

        // Assert
        output.Should().NotContain("Passed!");
        output.Should().Contain("Failed!", Exactly.Times(4));
        output.Should().Contain("Rerun filter: FullyQualifiedName~SimpleNumberFailCompare",
            Exactly.Thrice());        
        output.Should().Contain("Failed:     2, Passed:     6",
            Exactly.Once());
        Environment.ExitCode.Should().Be(1);
    }

    [Fact]
    public async Task DotnetTestRerun_RunNUnitExample_Success()
    {
        // Arrange
        Environment.ExitCode = 0;
        
        // Act
        var output = await RunDotNetTestRerunAndCollectOutputMessage("NUnitTestExample");

        // Assert
        output.Should().Contain("Passed!", Exactly.Once());
        output.Should().NotContainAny(new string[] {"Failed!", "Rerun attempt"});
        Environment.ExitCode.Should().Be(0);
    }

    [Fact]
    public async Task DotnetTestRerun_RunNUnitExample_WithDeleteFiles()
    {
        // Arrange
        var testDir = TestUtilities.GetTmpDirectory();
        TestUtilities.CopyFixture(string.Empty, new DirectoryInfo(testDir));
        
        // Act
        var output = await RunDotNetTestRerunAndCollectOutputMessage("NUnitTestExample", "--deleteReports", testDir);

        // Assert
        output.Should().Contain("Passed!", Exactly.Once());
        output.Should().NotContainAny(new string[] {"Failed!", "Rerun attempt"});

        var files = FileSystem.Directory.EnumerateFiles(testDir, "*trx");
        files.Should().HaveCount(0);
    }

    [Fact]
    public async Task DotnetTestRerun_FailingXUnit_Fails()
    {
        // Arrange
        Environment.ExitCode = 0;
        
        // Arrange
        var testDir = TestUtilities.GetTmpDirectory();
        TestUtilities.CopyFixture(string.Empty, new DirectoryInfo(testDir));
        Environment.ExitCode = 0;
        
        // Act
        var output = await RunDotNetTestRerunAndCollectOutputMessage("FailingXUnitExample", dir: testDir);

        // Assert
        output.Should().NotContain("Passed!");
        output.Should().Contain("Failed!", Exactly.Times(4));
        output.Should().Contain("Rerun filter: FullyQualifiedName~FailingXUnitExample.SimpleTest.SimpleStringCompare",
            Exactly.Thrice());        
        output.Should().Contain("Passed:     4",
            Exactly.Once());
        var files = FileSystem.Directory.EnumerateFiles(testDir, "*trx");
        files.Should().HaveCount(4);
        Environment.ExitCode.Should().Be(1);
    }

    [Fact]
    public async Task DotnetTestRerun_FailingXUnit_RunningProcess_Fails()
    {
        // Arrange
        Environment.ExitCode = 0;

        // Arrange
        Process process = new Process();
        process.StartInfo.FileName = "test-rerun";
        process.StartInfo.Arguments = $"{_dir}\\FailingXUnitExample --rerunMaxAttempts 3 --results-directory {_dir}";
        
        // Act
        process.Start();

        // Assert
        await process.WaitForExitAsync();
        process.ExitCode.Should().Be(1);
    }

    [Fact]
    public async Task DotnetTestRerun_RunNonExistentXUnitProject_MissingArguments_Fails()
    {
        // Arrange
        Environment.ExitCode = 0;

        // Arrange
        string folderPath = @"C:\path\to\folder";
        
        Process process = new Process();
        process.StartInfo.FileName = "test-rerun";
        process.StartInfo.Arguments = $"{_dir}\\XUnitThatDoesNotExist --rerunMaxAttempts 3 --results-directory {_dir}";
        
        // Act
        process.Start();

        // Assert
        await process.WaitForExitAsync();
        process.ExitCode.Should().Be(1);
    }

    [Fact]
    public async Task DotnetTestRerun_FailingMultipleXUnit_Fails()
    {
        // Arrange
        Environment.ExitCode = 0;

        // Act
        var output = await RunDotNetTestRerunAndCollectOutputMessage("FailingMultipleXUnitExample");

        // Assert
        output.Should().NotContain("Passed!");
        output.Should().Contain("Failed!", Exactly.Times(4));g
        output.Should().Contain("Rerun filter: FullyQualifiedName~FailingXUnitExample.SimpleTest.SimpleFailedNumberCompare",
            Exactly.Thrice());        
        output.Should().Contain("Failed:     2, Passed:     5",
            Exactly.Once());
        Environment.ExitCode.Should().Be(1);
    }

    public async Task DotnetTestRerun_FailingXUnit_WithDeleteFiles()
    {
        // Arrange
        var testDir = TestUtilities.GetTmpDirectory();
        TestUtilities.CopyFixture(string.Empty, new DirectoryInfo(testDir));
        
        // Act
        var output = await RunDotNetTestRerunAndCollectOutputMessage("FailingXUnitExample", "--deleteReports", testDir);

        // Assert
        output.Should().NotContain("Passed!");
        output.Should().Contain("Failed!", Exactly.Times(4));
        output.Should().Contain("Rerun filter: FullyQualifiedName~FailingXUnitExample.SimpleTest.SimpleStringCompare",
            Exactly.Thrice());        
        output.Should().Contain("Passed:     4",
            Exactly.Once());

        var files = FileSystem.Directory.EnumerateFiles(testDir, "*trx");
        files.Should().HaveCount(0);
    }

    private async Task<string> RunDotNetTestRerunAndCollectOutputMessage(string proj, string extraArgs = "", string? dir = null)
    {
        var testDir = dir ?? _dir;
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        var logger = new Logger();
        logger.SetLogLevel(LogLevel.Debug);

        Command command = new Command("test-rerun");
        RerunCommandConfiguration rerunCommandConfiguration = new RerunCommandConfiguration();
        rerunCommandConfiguration.Set(command);

        ParseResult result =
            new Parser(command).Parse($"{testDir}\\{proj} --rerunMaxAttempts 3 --results-directory {testDir} {extraArgs}");
        InvocationContext context = new(result);

        rerunCommandConfiguration.GetValues(context);

        RerunCommand rerunCommand = new RerunCommand(logger,
            rerunCommandConfiguration,
            new DotNetTestRunner(logger, new ProcessExecution(logger)),
            FileSystem,
            new TestResultsAnalyzer(logger));

        await rerunCommand.Run();

        return stringWriter.ToString().Trim();
    }
}