using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO.Abstractions;
using dotnet.test.rerun.Analyzers;
using dotnet.test.rerun.DotNetRunner;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Logging;
using dotnet.test.rerun.RerunCommand;
using FluentAssertions;
using Moq;
using Xunit;

namespace dotnet_test_rerun.UnitTest.DotNetTestRunner;

public class ProcessExecutionTests
{
    [Fact]
    public void GetOutput_ShouldReturnSetValue()
    {
        // Arrange
        var logger = new Logger();
        ProcessExecution processExecution = new ProcessExecution(logger)
        {
            Output = "Output"
        };

        // Act
        var output = processExecution.GetOutput();

        // Assert
        output.Should().Be(processExecution.Output);
    }
    
    [Fact]
    public void GetError_ShouldReturnSetValue()
    {
        // Arrange
        var logger = new Logger();
        ProcessExecution processExecution = new ProcessExecution(logger)
        {
            Error = "Error"
        };

        // Act
        var error = processExecution.GetError();

        // Assert
        error.Should().Be(processExecution.Error);
    }
}