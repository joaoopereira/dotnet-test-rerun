using System.Diagnostics;
using System.IO.Abstractions;

namespace dotnet.test.rerun
{
    public class dotnet
    {
        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        private readonly Logger Log;

        /// <summary>
        /// The process start information
        /// </summary>
        private readonly ProcessStartInfo ProcessStartInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineRunner"/> class.
        /// </summary>
        /// <param name="workingDirectory">The working directory.</param>
        public dotnet(Logger logger, IDirectoryInfo? workingDirectory = null)
        {
            ProcessStartInfo = new()
            {
                FileName = "dotnet",
                WorkingDirectory = workingDirectory?.FullName,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            Log = logger;
        }

        /// <summary>
        /// Runs dotnet tool with provided arguments
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        public void Run(string arguments)
        {
            Log.Debug($"Forking {arguments}");
            ProcessStartInfo.Arguments = arguments;

            using var ps = Process.Start(ProcessStartInfo);
            ps.OutputDataReceived += (sender, args) => Log.Verbose(args.Data);
            ps.ErrorDataReceived += (sender, args) => Log.Error(args.Data);
            ps.BeginOutputReadLine();
            ps.BeginErrorReadLine();

            ps.WaitForExit();

            if (ps.ExitCode != 0 && ps.ExitCode != 1)
            {
                Log.Error($"Exit code {ps.ExitCode}.");

                throw new RerunException($"dotnet {arguments} did not finished successfully.");
            }
        }

        public void Run(string dll, string filter, string settings, string logger, string resultsDirectory)
        {
            Run($"test {dll} --filter \"{filter}\" --settings \"{settings}\" --logger {logger} --results-directory {resultsDirectory}");
        }
    }
}