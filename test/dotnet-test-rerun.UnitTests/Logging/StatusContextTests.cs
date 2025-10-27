using dotnet.test.rerun.Logging;
using FluentAssertions;
using NSubstitute;
using Spectre.Console;
using Spectre.Console.Testing;
using Xunit;
using StatusContext = dotnet.test.rerun.Logging.StatusContext;

namespace dotnet_test_rerun.UnitTest.Logging;

public class StatusContextTests
{
    [Fact]
    public void StatusContextTests_WithNullContext_Status_ShouldThrowException()
    {
        // Arrange
        var statusContext = new StatusContext(null!);
        
        // Act
        var act = () => statusContext.Status("test");

        // Assert
        act.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'context')");
    }
}