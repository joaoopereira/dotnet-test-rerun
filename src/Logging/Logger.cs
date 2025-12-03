using System.CommandLine.Parsing;
using Spectre.Console;
using Spectre.Console.Rendering;

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
        /// Logs the progress of an operation, single line with a spinner
        /// </summary>
        /// <param name="msg">initial message to print</param>
        public void Status(string msg, Action<StatusContext> action = null!)
        {
            AnsiConsole.Status().Start(msg, ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                if (action != null)
                {
                    action(new StatusContext(ctx));
                }
            });
        }

        /// <summary>
        /// renders a renderable Spectre object (currently we use it for trees)
        /// This abstracts from the logging library (up to a point)
        /// </summary>
        /// <param name="renderable"></param>
        public void Render(IRenderable renderable)
        {
            AnsiConsole.Write(renderable);
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
        /// Escapes Spectre Console markup
        /// Markup is enclosed in [] and escaped as [[]]
        /// This is a very naive operation that will likely need extending in the future
        /// </summary>
        /// <param name="msg">message to be escaped</param>
        /// <returns>escaped message</returns>
        private string EscapeMarkup(string msg) => msg?.Replace("[", "[[").Replace("]", "]]") ?? string.Empty;

        /// <summary>
        /// LogLevel Parser
        /// </summary>
        public static Func<ArgumentResult, LogLevel> ParseLogLevel = argResult =>
        {
            var loglevel = LogLevel.Verbose;
            string loglevelStr = "verbose";
            if (argResult.Tokens.Any())
            {
                loglevelStr = argResult.Tokens[0].Value;
            }

            if (Enum.TryParse(typeof(LogLevel), loglevelStr, ignoreCase: true, out var loglevelObj))
            {
                loglevel = (LogLevel)loglevelObj!;
            }
            return loglevel;
        };
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