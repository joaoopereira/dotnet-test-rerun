using dotnet.test.rerun.DotNetRunner;
using dotnet.test.rerun.Logging;
using FluentAssertions;
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