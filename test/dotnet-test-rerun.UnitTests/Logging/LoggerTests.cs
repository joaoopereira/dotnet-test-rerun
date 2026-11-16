using dotnet.test.rerun.Logging;
using AwesomeAssertions;
using Spectre.Console;
using Spectre.Console.Testing;
using Xunit;

namespace dotnet_test_rerun.UnitTest.Logging;

[Collection("Sequential")]
public class LoggerTests
{
        [Theory]
        [InlineData(LogLevel.Debug, "test message")]
        [InlineData(LogLevel.Verbose, "")]
        [InlineData(LogLevel.Information, "")]
        [InlineData(LogLevel.Warning, "")]
        [InlineData(LogLevel.Error, "")]
        public void LoggerTests_Debug_OnlyShowMessageIfLower(LogLevel logLevel, string expectedMessage)
        {
            // Arrange
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            var logger = new Logger();
            logger.SetLogLevel(logLevel);
            var message = "test message";

            // Act
            logger.Debug(message);

            // Assert
            stringWriter.ToString().Trim().Should().Be(expectedMessage);
        }
        
        [Theory]
        [InlineData(LogLevel.Debug, "test message")]
        [InlineData(LogLevel.Verbose, "test message")]
        [InlineData(LogLevel.Information, "")]
        [InlineData(LogLevel.Warning, "")]
        [InlineData(LogLevel.Error, "")]
        public void LoggerTests_Verbose_OnlyShowMessageIfLower(LogLevel logLevel, string expectedMessage)
        {
            // Arrange
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            var logger = new Logger();
            logger.SetLogLevel(logLevel);
            var message = "test message";

            // Act
            logger.Verbose(message);

            // Assert
            stringWriter.ToString().Trim().Should().Be(expectedMessage);
        }
        
        [Theory]
        [InlineData(LogLevel.Debug, "test message")]
        [InlineData(LogLevel.Verbose, "test message")]
        [InlineData(LogLevel.Information, "test message")]
        [InlineData(LogLevel.Warning, "")]
        [InlineData(LogLevel.Error, "")]
        public void LoggerTests_Information_OnlyShowMessageIfLower(LogLevel logLevel, string expectedMessage)
        {
            // Arrange
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            var logger = new Logger();
            logger.SetLogLevel(logLevel);
            var message = "test message";

            // Act
            logger.Information(message);

            // Assert
            stringWriter.ToString().Trim().Should().Be(expectedMessage);
        }
        
        [Theory]
        [InlineData(LogLevel.Debug, "test message")]
        [InlineData(LogLevel.Verbose, "test message")]
        [InlineData(LogLevel.Information, "test message")]
        [InlineData(LogLevel.Warning, "test message")]
        [InlineData(LogLevel.Error, "")]
        public void LoggerTests_Warning_OnlyShowMessageIfLower(LogLevel logLevel, string expectedMessage)
        {
            // Arrange
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            var logger = new Logger();
            logger.SetLogLevel(logLevel);
            var message = "test message";

            // Act
            logger.Warning(message);

            // Assert
            stringWriter.ToString().Trim().Should().Be(expectedMessage);
        }
        
        [Theory]
        [InlineData(LogLevel.Debug, "test message")]
        [InlineData(LogLevel.Verbose, "test message")]
        [InlineData(LogLevel.Information, "test message")]
        [InlineData(LogLevel.Warning, "test message")]
        [InlineData(LogLevel.Error, "test message")]
        public void LoggerTests_Error_OnlyShowMessageIfLower(LogLevel logLevel, string expectedMessage)
        {
            // Arrange
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            var logger = new Logger();
            logger.SetLogLevel(logLevel);
            var message = "test message";

            // Act
            logger.Error(message);

            // Assert
            stringWriter.ToString().Trim().Should().Be(expectedMessage);
        }

        [Fact]
        public void LoggerTests_Exception_ShouldWrite()
        {
            // Arrange
            var testConsole = new TestConsole();
            var logger = new Logger(testConsole);
            var exception = new Exception("test exception");

            // Act
            logger.Exception(exception);

            // Assert
            testConsole.Output.Should().Contain(exception.Message);
        }

        


        [Theory]
        [InlineData(LogLevel.Debug, "✓ test.name")]
        [InlineData(LogLevel.Verbose, "✓ test.name")]
        [InlineData(LogLevel.Information, "✓ test.name")]
        [InlineData(LogLevel.Warning, "")]
        [InlineData(LogLevel.Error, "")]
        public void LoggerTests_TestResult_Passed_OnlyShowMessageIfLower(LogLevel logLevel, string expectedMessage)
        {
            // Arrange
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            var logger = new Logger();
            logger.SetLogLevel(logLevel);

            // Act
            logger.TestResult("test.name", true);

            // Assert
            stringWriter.ToString().Trim().Should().Be(expectedMessage);
        }

        [Fact]
        public void LoggerTests_TestResult_Failed_ShouldWriteNameAndErrorDetails()
        {
            // Arrange
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            var logger = new Logger();

            // Act
            logger.TestResult("test.name", false, "boom happened", "at test.name() in file.cs:line 1");

            // Assert
            var output = stringWriter.ToString();
            output.Should().Contain("✗ test.name");
            output.Should().Contain("boom happened");
            output.Should().Contain("at test.name() in file.cs:line 1");
        }

        [Fact]
        public void LoggerTests_TestResult_Failed_WithoutErrorDetails_ShouldOnlyWriteName()
        {
            // Arrange
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            var logger = new Logger();

            // Act
            logger.TestResult("test.name", false);

            // Assert
            stringWriter.ToString().Trim().Should().Be("✗ test.name");
        }

        [Theory]
        [InlineData("value [x] and [/]")]
        [InlineData("[red]not markup[/]")]
        [InlineData("array[0]")]
        public void LoggerTests_MessagesWithBrackets_ArePrintedAsIs(string message)
        {
            // Arrange
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            var logger = new Logger();

            // Act
            logger.Error(message);

            // Assert
            stringWriter.ToString().Trim().Should().Be(message);
        }

        [Theory]
        [InlineData("value [x] and [/]")]
        [InlineData("[red]not markup[/]")]
        public void LoggerTests_OnATerminal_WritesMarkupAndKeepsBracketsAsIs(string message)
        {
            // Arrange
            var testConsole = new TestConsole();
            testConsole.Profile.Out = new TerminalOutput(testConsole.Profile.Out);
            // A terminal output resets the width, and the StringWriter behind it has none
            testConsole.Profile.Width = 120;
            var logger = new Logger(testConsole);

            // Act
            logger.Error(message);
            logger.TestResult("Tests.Method(\"[a]\")", false, message, "at [b]");

            // Assert
            testConsole.Lines.Should().Equal(
                $"  {message}",
                "  ✗ Tests.Method(\"[a]\")",
                $"      {message}",
                "      at [b]");
        }

        /// <summary>
        /// Reports itself as a terminal, which a redirected test run never is, so the markup path is taken.
        /// </summary>
        private sealed class TerminalOutput(IAnsiConsoleOutput output) : IAnsiConsoleOutput
        {
            public TextWriter Writer => output.Writer;
            public bool IsTerminal => true;
            public int Width => output.Width;
            public int Height => output.Height;
            public void SetEncoding(System.Text.Encoding encoding) => output.SetEncoding(encoding);
        }
}
