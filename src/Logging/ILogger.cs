using System.CommandLine.Parsing;
using System.CommandLine;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace dotnet.test.rerun.Logging
{
    public interface ILogger
    {
        /// <summary>
        /// The default log level
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// Log the message with Debug verbosity
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public void Debug(string msg);

        /// <summary>
        /// Log the message with Verbose verbosity
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public void Verbose(string msg);

        /// <summary>
        /// Log the message with Information verbosity
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public void Information(string msg);

        /// <summary>
        /// Log the message with Warning verbosity
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public void Warning(string msg);

        /// <summary>
        /// Log the message with Error verbosity
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public void Error(string msg);

        /// <summary>
        /// Log the Exception
        /// </summary>
        /// <param name="e">an exception</param>
        public void Exception(Exception e);

        /// <summary>
        /// Logs the progress of an operation, single line with a spinner
        /// </summary>
        /// <param name="msg">initial message to print</param>
        public void Status(string msg, Action<StatusContext> action = null);

        /// <summary>
        /// renders a renderable Spectre object (currently we use it for trees)
        /// This abstracts from the logging library (up to a point)
        /// </summary>
        /// <param name="renderable"></param>
        public void Render(IRenderable renderable);

    }
}