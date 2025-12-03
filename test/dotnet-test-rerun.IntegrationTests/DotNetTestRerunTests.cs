using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO.Abstractions;
using dotnet_test_rerun.Common.Utilities;
using dotnet.test.rerun.Analyzers;
using dotnet.test.rerun.DotNetRunner;
using dotnet.test.rerun.Logging;
using dotnet.test.rerun.RerunCommand;
using AwesomeAssertions;
using Xunit.Abstractions;

namespace dotnet_test_rerun.IntegrationTests;

public class DotNetTestRerunTests
{
    private static string _dir = TestUtilities.GetTmpDirectory();
    private static readonly IFileSystem FileSystem = new FileSystem();

    public DotNetTestRerunTests(ITestOutputHelper testOutputHelper)
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
        output.Should().NotContainAny(new string[] { "Failed!", "Rerun attempt" });
        Environment.ExitCode.Should().Be(0);
        IsThereACoverageFile().Should().BeFalse();
    }

    [Fact]
    public async Task DotnetTestRerun_RunMSTestExample_Success()
    {
        // Arrange
        Environment.ExitCode = 0;

        // Act
        var output = await RunDotNetTestRerunAndCollectOutputMessage("MSTestExample",
            "--collect \"XPlat Code Coverage\" --configuration \"debug\" --verbosity \"minimal\"");

        // Assert
        output.Should().Contain("Passed!", Exactly.Once());
        output.Should().NotContainAny(new string[] { "Failed!", "Rerun attempt" });
        output.Should().Contain("-c \"debug\" -v \"Minimal\"");
        Environment.ExitCode.Should().Be(0);
        IsThereACoverageFile().Should().BeTrue();
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
        output.Should().Contain("Rerun filter: FullyQualifiedName=MSTestExample.UnitTest1.SimpleNumberFailCompare",
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
        output.Should().NotContainAny(new string[] { "Failed!", "Rerun attempt" });
        Environment.ExitCode.Should().Be(0);
    }

    [Fact]
    public async Task DotnetTestRerun_RunNUnitExample_WithPropertiesActive_Success()
    {
        // Arrange
        Environment.ExitCode = 0;

        // Act
        var output = await RunDotNetTestRerunAndCollectOutputMessage("NUnitTestExample",
            extraArgs:
            $"/p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput={_dir}\\TestResults\\Coverage\\UnitTests-TestResults.opencover.xml");

        // Assert
        output.Should().Contain("Passed!", Exactly.Once());
        output.Should().NotContainAny(new string[] { "Failed!", "Rerun attempt" });
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
        output.Should().NotContainAny(new string[] { "Failed!", "Rerun attempt" });

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
        output.Should().Contain("Rerun filter: FullyQualifiedName=FailingXUnitExample.SimpleTest.SimpleStringCompare",
            Exactly.Thrice());
        output.Should().Contain("Passed:     4",
            Exactly.Once());
        var files = FileSystem.Directory.EnumerateFiles(testDir, "*trx");
        files.Should().HaveCount(4);
        Environment.ExitCode.Should().Be(1);
    }    
    
    [Fact]
    public async Task DotnetTestRerun_FailingXUnit_FailsInAllRetries()
    {
        // Arrange
        Environment.ExitCode = 0;

        // Act
        var output = await RunDotNetTestRerunAndCollectOutputMessage("XUnitExampleFailInAllRetries");

        // Assert
        output.Should().Contain("Failed!", Exactly.Times(4));
        output.Should().Contain("Failed:     2, Passed:     0", Exactly.Times(4));
        Environment.ExitCode.Should().Be(1);
        IsThereACoverageFile().Should().BeFalse();
    }

    [Fact]
    public async Task DotnetTestRerun_FailingXUnit_FailsInAllRetriesWithDisplayName()
    {
        // Arrange
        Environment.ExitCode = 0;

        // Act
        var output = await RunDotNetTestRerunAndCollectOutputMessage("XUnitExampleFailInAllRetriesWithDisplayName");

        // Assert
        output.Should().Contain("Failed!", Exactly.Times(4));
        output.Should().Contain("Failed:     2, Passed:     0", Exactly.Times(4));
        Environment.ExitCode.Should().Be(1);
        IsThereACoverageFile().Should().BeFalse();
    }

    [Fact]
    public async Task DotnetTestRerun_FailingXUnit_PassOnSecond()
    {
        // Arrange
        var testDir = TestUtilities.GetTmpDirectory();
        TestUtilities.CopyFixture(string.Empty, new DirectoryInfo(testDir));
        Environment.ExitCode = 0;

        // Act
        var output = await RunDotNetTestRerunAndCollectOutputMessage("XUnitTestPassOnSecondRunExample", dir: testDir);

        // Assert
        output.Should().Contain("Passed!");
        output.Should().Contain("Failed!", Exactly.Times(1));
        output.Should().Contain("Rerun filter: FullyQualifiedName=XUnitTestPassOnSecondRunExample.SimpleTest",            Exactly.Once());
        output.Should().Contain("Failed:     1, Passed:     1", Exactly.Once());
        output.Should().Contain("Failed:     0, Passed:     1", Exactly.Once());
        var files = FileSystem.Directory.EnumerateFiles(testDir, "*trx");
        files.Should().HaveCount(2);
        Environment.ExitCode.Should().Be(0);
    }

    [InlineData("normal")]
    [InlineData("quiet")]
    [InlineData("minimal")]
    [InlineData("detailed")]
    [Theory]
    public async Task DotnetTestRerun_FailingXUnit_WithVerbosity_Fails(string verbosity)
    {
        // Arrange
        Environment.ExitCode = 0;

        // Arrange
        var testDir = TestUtilities.GetTmpDirectory();
        TestUtilities.CopyFixture(string.Empty, new DirectoryInfo(testDir));
        Environment.ExitCode = 0;

        // Act
        var output = await RunDotNetTestRerunAndCollectOutputMessage("FailingXUnitExample", dir: testDir,
            extraArgs: $"--verbosity {verbosity}");

        // Assert
        output.Should().NotContain("Passed!");
        output.Should().Contain("Rerun filter: FullyQualifiedName=FailingXUnitExample.SimpleTest.SimpleStringCompare",
            Exactly.Thrice());
        output.Should().ContainAny("Passed: 4", "Passed:     4");
        var files = FileSystem.Directory.EnumerateFiles(testDir, "*trx");
        files.Should().HaveCount(4);
        Environment.ExitCode.Should().Be(1);
    }

    [Fact]
    public async Task DotnetTestRerun_FailingXUnit_WithMultipleFrameworks_Fails()
    {
        // Arrange
        Environment.ExitCode = 0;

        // Arrange
        var testDir = TestUtilities.GetTmpDirectory();
        TestUtilities.CopyFixture(string.Empty, new DirectoryInfo(testDir));
        Environment.ExitCode = 0;

        // Act
        var output =
            await RunDotNetTestRerunAndCollectOutputMessage("FailingXUnitWithMultipleFrameworksExample", dir: testDir);

        // Assert
        output.Should().MatchRegex(
            "Rerun filter: FullyQualifiedName=(FailingXUnitExample.)*SimpleTest.SimpleStringCompare",
            Exactly.Thrice());
        output.Should().Contain("Passed:     1", Exactly.Times(4));
        output.Should().Contain("Failed:     1", Exactly.Times(4));
        output.Should().Contain("Passed!", Exactly.Times(4));
        output.Should().Contain("Failed!", Exactly.Times(4));
        var files = FileSystem.Directory.EnumerateFiles(testDir, "*trx");
        files.Should().HaveCount(8);
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
        output.Should().Contain("Failed!", Exactly.Times(4));
        output.Should().Contain(
            "Rerun filter: FullyQualifiedName=FailingXUnitExample.SimpleTest.SimpleFailedNumberCompare",
            Exactly.Thrice());
        output.Should().Contain("Failed:     2, Passed:     5",
            Exactly.Once());
        Environment.ExitCode.Should().Be(1);
    }

    [Fact]
    public async Task DotnetTestRerun_FailingNUnit_PassOnSecond()
    {
        // Arrange
        Environment.ExitCode = 0;

        // Act
        var output = await RunDotNetTestRerunAndCollectOutputMessage("NUnitTestPassOnSecondRunExample");

        // Assert
        output.Should().Contain("Passed!");
        output.Should().Contain("Failed!", Exactly.Times(1));
        output.Should().Contain("Rerun filter: FullyQualifiedName=NUnitTestExample.Tests.SecondSimpleNumberCompare",
            Exactly.Once());
        output.Should().Contain("Failed:     1, Passed:     1",
            Exactly.Once());
        Environment.ExitCode.Should().Be(0);
    }

    [Fact]
    public async Task DotnetTestRerun_FailingNUnit_PassOnSecond_WithCategory()
    {
        // Arrange
        Environment.ExitCode = 0;

        // Act
        var output = await RunDotNetTestRerunAndCollectOutputMessage("NUnitTestPassOnSecondRunExampleWithCategory",
            "--filter TestCategory=FirstCategory|TestCategory=SecondCategory");

        // Assert
        output.Should().Contain("Passed!");
        output.Should().Contain("Failed!", Exactly.Times(1));
        output.Should().Contain(
            "Rerun filter: (TestCategory=FirstCategory|TestCategory=SecondCategory)&(FullyQualifiedName=NUnitTestExample.Tests.SecondSimpleNumberCompare)",
            Exactly.Once());
        output.Should().Contain("Failed:     1, Passed:     1",
            Exactly.Once());
        Environment.ExitCode.Should().Be(0);
    }

    [Fact]
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
        output.Should().Contain("Rerun filter: FullyQualifiedName=FailingXUnitExample.SimpleTest.SimpleStringCompare",
            Exactly.Thrice());
        output.Should().Contain("Passed:     4",
            Exactly.Once());

        var files = FileSystem.Directory.EnumerateFiles(testDir, "*trx");
        files.Should().HaveCount(0);
    }

    [Fact]
    public async Task DotnetTestRerun_FailingNUnit_PassOnSecond_TwoFailingProjects()
    {
        // Arrange
        Environment.ExitCode = 0;

        // Act
        var output = await RunDotNetTestRerunAndCollectOutputMessage("NUnitMultipleFailingProjects");

        // Assert
        output.Should().Contain("Passed!");
        output.Should().Contain("Failed!", Exactly.Times(3));
        output.Should().Contain("Rerun filter:",
            Exactly.Twice());
        output.Should().Contain("Failed:     2, Passed:     1",
            Exactly.Once());
        output.Should().Contain("Failed:     1, Passed:     1",
            Exactly.Twice());
        output.Should().Contain("Failed:     0, Passed:     1",
            Exactly.Twice());
        Environment.ExitCode.Should().Be(0);
    }

    [Fact]
    public async Task DotnetTestRerun_FailingNUnit_PassOnSecond_WithFailingTestWithSameNameAsOnePassed()
    {
        // Arrange
        Environment.ExitCode = 0;

        // Act
        var output = await RunDotNetTestRerunAndCollectOutputMessage("NUnitTestWithTwoTestsWithSameNameExample");

        // Assert
        output.Should().Contain("Passed!");
        output.Should().Contain("Failed!", Exactly.Times(1));
        output.Should().Contain(
            "Rerun filter: FullyQualifiedName=NUnitTestExample.SimpleTest.SecondSimpleNumberCompare",
            Exactly.Once());
        output.Should().Contain("Failed:     1, Passed:     2",
            Exactly.Once());
        output.Should().Contain("Failed:     0, Passed:     1",
            Exactly.Once());
        Environment.ExitCode.Should().Be(0);
    }
    
    [Fact]
    public async Task DotnetTestRerun_FailingNUnit_ParameterizedTests_OnlyRetriesFailedCase()
    {
        // Arrange
        var testDir = TestUtilities.GetTmpDirectory();
        TestUtilities.CopyFixture(string.Empty, new DirectoryInfo(testDir));
        Environment.ExitCode = 0;
        
        // Clean up any previous counter files
        var counterFile = Path.Combine(Path.GetTempPath(), "nunit_parameterized_test_run_counter_2_2.txt");
        var stringParamCounterFile = Path.Combine(Path.GetTempPath(), "nunit_string_param_counter.txt");
        if (File.Exists(counterFile))
        {
            File.Delete(counterFile);
        }
        if (File.Exists(stringParamCounterFile))
        {
            File.Delete(stringParamCounterFile);
        }

        // Act
        var output = await RunDotNetTestRerunAndCollectOutputMessage("NUnitTestParameterizedPassOnSecondRunExample", dir: testDir);

        // Assert
        output.Should().Contain("Passed!");
        // Two failures initially, but we're primarily testing the DataDrivenTest(2,2) retry
        output.Should().Contain("Failed!");
        // First run: 2 failed, 6 passed (8 total tests, 2 fail)
        output.Should().Contain("Failed:     2, Passed:     6",
            Exactly.Once());
        // Second run: At least the DataDrivenTest(2,2) is retried successfully
        output.Should().Contain("Failed:     0, Passed:     1",
            Exactly.Once());
        var files = FileSystem.Directory.EnumerateFiles(testDir, "*trx");
        files.Should().HaveCountGreaterThanOrEqualTo(2);
        
        // Clean up the counter files
        if (File.Exists(counterFile))
        {
            File.Delete(counterFile);
        }
        if (File.Exists(stringParamCounterFile))
        {
            File.Delete(stringParamCounterFile);
        }
    }
    
    [Fact]
    public async Task DotnetTestRerun_FailingNUnit_ParameterizedTests_WithStringParamsAndSpaces_ConstructsFilterCorrectly()
    {
        // Arrange
        var testDir = TestUtilities.GetTmpDirectory();
        TestUtilities.CopyFixture(string.Empty, new DirectoryInfo(testDir));
        Environment.ExitCode = 0;
        
        // Clean up any previous counter files
        var counterFile = Path.Combine(Path.GetTempPath(), "nunit_parameterized_test_run_counter_2_2.txt");
        var stringParamCounterFile = Path.Combine(Path.GetTempPath(), "nunit_string_param_counter.txt");
        if (File.Exists(counterFile))
        {
            File.Delete(counterFile);
        }
        if (File.Exists(stringParamCounterFile))
        {
            File.Delete(stringParamCounterFile);
        }

        // Act
        var output = await RunDotNetTestRerunAndCollectOutputMessage("NUnitTestParameterizedPassOnSecondRunExample", dir: testDir);

        // Assert - Main goal: verify no shell parsing errors occur with string parameters containing spaces
        // The user reported errors like "can't find the assembly 'pays'" due to quotes and spaces
        output.Should().NotContain("can't find the assembly");
        output.Should().NotContain("unrecognized escape sequence");
        
        // Verify tests ran and initial failures were detected
        output.Should().Contain("Failed:     2, Passed:     6",
            Exactly.Once());
        
        // The test completes without shell parsing errors (the main fix)
        // This verifies that removing quotes from the filter prevents the shell from misinterpreting 
        // parameters with spaces like "All pays" as separate command arguments
        var files = FileSystem.Directory.EnumerateFiles(testDir, "*trx");
        files.Should().HaveCountGreaterThanOrEqualTo(2);
        
        // Clean up the counter files
        if (File.Exists(counterFile))
        {
            File.Delete(counterFile);
        }
        if (File.Exists(stringParamCounterFile))
        {
            File.Delete(stringParamCounterFile);
        }
    }
    
    [Fact]
    public async Task DotnetTestRerun_RunOnSolution_WithDifferentNetVersions_WithNoBuild_Sucess()
    {
        Environment.ExitCode = 0;
        var buildTimeoutInSeconds = TimeSpan.FromSeconds(30);
        const string testSolution = "XUnitTwoMultipleProjectsWithDifferentNetVersion";

        //build separately, as we are using --no-build, without no build the behaviour is different
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build {_dir}\\{testSolution}"
            }
        };
        process.Start();
        process.WaitForExit(buildTimeoutInSeconds);

        // Act
        var output = await RunDotNetTestRerunAndCollectOutputMessage(testSolution, "--no-build");

        // Assert
        output.Should().Contain("Failed!", Exactly.Once());
        output.Should().Contain("Passed!");
        output.Should().Contain("Rerun filter: FullyQualifiedName=XUnitTestPassOnSecondRunExample.SimpleTest.TestDecreaseState", Exactly.Once());
        output.Should().Contain("Failed:     1, Passed:     1", Exactly.Once());
        output.Should().Contain("Failed:     0, Passed:     1");
        Environment.ExitCode.Should().Be(0);
    }

    private async Task<string> RunDotNetTestRerunAndCollectOutputMessage(string proj, string extraArgs = "",
        string? dir = null)
    {
        var testDir = dir ?? _dir;
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        var logger = new Logger();
        logger.SetLogLevel(LogLevel.Debug);

        Command command = new Command("test-rerun");
        RerunCommandConfiguration rerunCommandConfiguration = new RerunCommandConfiguration();
        rerunCommandConfiguration.Set(command);

        ParseResult result = command.Parse(
                $"{testDir}\\{proj} --rerunMaxAttempts 3 --results-directory {testDir} {extraArgs}");

        rerunCommandConfiguration.GetValues(result);

        RerunCommand rerunCommand = new RerunCommand(logger,
            rerunCommandConfiguration,
            new DotNetTestRunner(logger, new ProcessExecution(logger)),
            new DotNetCoverageRunner(logger, new ProcessExecution(logger)),
            FileSystem,
            new TestResultsAnalyzer(logger));

        await rerunCommand.Run();

        return stringWriter.ToString().Trim();
    }

    private static bool IsThereACoverageFile()
        => Directory.GetFiles(_dir, "*cobertura.xml", SearchOption.AllDirectories).Any();
}