using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using System.Text;
using dotnet_test_rerun.IntegrationTests.Utilities;
using dotnet.test.rerun.Analyzers;
using dotnet.test.rerun.DotNetTestRunner;
using dotnet.test.rerun.Logging;
using dotnet.test.rerun.RerunCommand;
using FluentAssertions;
using Spectre.Console;
using Spectre.Console.Testing;

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
        // Act
        var output = await RunDotNetTestRerunAndCollectOutputMessage("XUnitExample");

        // Assert
        output.Should().Contain("Passed!", Exactly.Once());
        output.Should().NotContainAny(new string[] {"Failed!", "Rerun attempt"});
    }

    [Fact]
    public async Task DotnetTestRerun_RunMSTestExample_Success()
    {
        // Act
        var output = await RunDotNetTestRerunAndCollectOutputMessage("MSTestExample");

        // Assert
        output.Should().Contain("Passed!", Exactly.Once());
        output.Should().NotContainAny(new string[] {"Failed!", "Rerun attempt"});
    }

    [Fact]
    public async Task DotnetTestRerun_RunNUnitExample_Success()
    {
        // Act
        var output = await RunDotNetTestRerunAndCollectOutputMessage("NUnitTestExample");

        // Assert
        output.Should().Contain("Passed!", Exactly.Once());
        output.Should().NotContainAny(new string[] {"Failed!", "Rerun attempt"});
    }

    [Fact]
    public async Task DotnetTestRerun_FailingXUnit_Fails()
    {
        // Act
        var output = await RunDotNetTestRerunAndCollectOutputMessage("FailingXUnitExample");

        // Assert
        output.Should().NotContain("Passed!");
        output.Should().Contain("Failed!", Exactly.Times(4));
        output.Should().Contain("Rerun filter: FullyQualifiedName~FailingXUnitExample.SimpleTest.SimpleStringCompare",
            Exactly.Thrice());        
        output.Should().Contain("Passed:     4",
            Exactly.Once());
    }

    private async Task<string> RunDotNetTestRerunAndCollectOutputMessage(string proj)
    {
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        var logger = new Logger();
        logger.SetLogLevel(LogLevel.Debug);

        Command command = new Command("test-rerun");
        RerunCommandConfiguration rerunCommandConfiguration = new RerunCommandConfiguration();
        rerunCommandConfiguration.Set(command);

        ParseResult result =
            new Parser(command).Parse($"{_dir}\\{proj} --rerunMaxAttempts 3 --results-directory {_dir}");
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