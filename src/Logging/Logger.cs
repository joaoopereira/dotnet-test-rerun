using Spectre.Console;

namespace dotnet.test.rerun.Logging
{
    public class Logger : ILogger
    {
        private readonly IAnsiConsole AnsiConsole;

        public Logger(IAnsiConsole? console = null)
        {
            AnsiConsole = console ?? Spectre.Console.AnsiConsole.Console;
        }

        /// <summary>
        /// The default log level
        /// </summary>
        LogLevel Level = LogLevel.Verbose;
        public void SetLogLevel(LogLevel logLevel)
        {
            Level = logLevel;
        }

        /// <summary>
        /// Log the message with Debug verbosity
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public void Debug(string msg)
        {
            if (Level <= LogLevel.Debug)
                Write($"[gray]{EscapeMarkup(msg)}[/]");
        }

        /// <summary>
        /// Log the message with Verbose verbosity
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public void Verbose(string msg)
        {
            if (Level <= LogLevel.Verbose)
                Write($"[white]{EscapeMarkup(msg)}[/]");
        }

        /// <summary>
        /// Log the message with Information verbosity
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public void Information(string msg)
        {
            if (Level <= LogLevel.Information)
                Write($"[green]{EscapeMarkup(msg)}[/]");
        }

        /// <summary>
        /// Log the message with Warning verbosity
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public void Warning(string msg)
        {
            if (Level <= LogLevel.Warning)
                Write($"[yellow]{EscapeMarkup(msg)}[/]");
        }

        /// <summary>
        /// Log the message with Error verbosity
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public void Error(string msg)
        {
            if (Level <= LogLevel.Error)
                Write($"[red]{EscapeMarkup(msg)}[/]");
        }

        /// <summary>
        /// Log the Exception
        /// </summary>
        /// <param name="e">an exception</param>
        public void Exception(Exception e)
        {
            AnsiConsole.WriteException(e);
        }

        /// <summary>
        /// Logs the result of an individual test, independently of the dotnet test verbosity.
        /// </summary>
        /// <param name="testName">The fully qualified name of the test.</param>
        /// <param name="passed">Whether the test passed.</param>
        /// <param name="errorMessage">The error message, when the test failed.</param>
        /// <param name="stackTrace">The stack trace, when the test failed.</param>
        public void TestResult(string testName, bool passed, string? errorMessage = null, string? stackTrace = null)
        {
            if (Level > LogLevel.Information)
                return;

            if (passed)
            {
                Write($"[green]✓ {EscapeMarkup(testName)}[/]");
                return;
            }

            Write($"[red]✗ {EscapeMarkup(testName)}[/]");
            if (!string.IsNullOrWhiteSpace(errorMessage))
                Write($"[red]    {EscapeMarkup(errorMessage)}[/]");
            if (!string.IsNullOrWhiteSpace(stackTrace))
                Write($"[grey]    {EscapeMarkup(stackTrace)}[/]");
        }

        /// <summary>
        /// Abstracts from the logging library
        /// </summary>
        /// <param name="msg">The MSG.</param>
        private void Write(string msg)
        {
            if(AnsiConsole.Profile.Out.IsTerminal)
            {
                AnsiConsole.MarkupLine($"  {msg}");
            }
            else
            {
                Console.WriteLine(msg.RemoveMarkup());
            }
            
        }

        /// <summary>
        /// Escapes Spectre Console markup, so text with [ and ] is printed as is
        /// </summary>
        /// <param name="msg">message to be escaped</param>
        /// <returns>escaped message</returns>
        private static string EscapeMarkup(string? msg) => Markup.Escape(msg ?? string.Empty);
    }

    public enum LogLevel
    {
        Debug,
        Verbose,
        Information,
        Warning,
        Error
    }
}