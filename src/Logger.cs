using Spectre.Console;

namespace dotnet.test.rerun
{
    public class Logger
    {
        private readonly IAnsiConsole AnsiConsole;

        public Logger()
        {
            AnsiConsole = Spectre.Console.AnsiConsole.Console;
        }

        /// <summary>
        /// The default log level
        /// </summary>
        public LogLevel Level = LogLevel.Verbose;

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
        /// Abstracts from the logging library
        /// </summary>
        /// <param name="msg">The MSG.</param>
        private void Write(string msg) => AnsiConsole.MarkupLine($"  {msg}");

        /// <summary>
        /// Escapes Spectre Console markup
        /// Markup is enclosed in [] and escaped as [[]]
        /// This is a very naive operation that will likely need extending in the future
        /// </summary>
        /// <param name="msg">message to be escaped</param>
        /// <returns>escaped message</returns>
        private string EscapeMarkup(string msg) => msg?.Replace("[", "[[").Replace("]", "]]");
    }

    public enum LogLevel
    {
        /// <summary>
        /// The debug
        /// </summary>
        Debug,

        /// <summary>
        /// The verbose
        /// </summary>
        Verbose,

        /// <summary>
        /// The information
        /// </summary>
        Information,

        /// <summary>
        /// The warning
        /// </summary>
        Warning,

        /// <summary>
        /// The error
        /// </summary>
        Error
    }
}