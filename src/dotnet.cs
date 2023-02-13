using System.Diagnostics;
using System.IO.Abstractions;
using dotnet.test.rerun.Logging;

namespace dotnet.test.rerun
{
    public class dotnet
    {
        public ErrorCode ErrorCode;
        private string Output;
        private string Error;
        private int ExitCode;
        private readonly ILogger Log;
        private readonly ProcessStartInfo ProcessStartInfo;
        private string[] WellKnownErrors = new[] { "No test source files were specified." };

        public dotnet(ILogger logger)
        {
            ProcessStartInfo = new()
            {
                FileName = "dotnet",
                WorkingDirectory = "/",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            Log = logger;
        }

        /// <summary>
        /// Runs dotnet test with the specified arguments
        /// </summary>
        /// <param name="dll">The DLL.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="resultsDirectory">The results directory.</param>
        public void Test(string dll, string filter, string settings, string logger, string resultsDirectory)
        {
            Run($"test {dll} --filter \"{filter}\" --settings \"{settings}\" --logger {logger} --results-directory {resultsDirectory}");
        }

        /// <summary>
        /// Runs dotnet test with the specified arguments.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        private void Run(string arguments) => Log.Status("running dotnet", ctx =>
        {
            Log.Debug($"working directory: {ProcessStartInfo.WorkingDirectory}");
            Log.Debug($"forking {arguments}");
            ProcessStartInfo.Arguments = arguments;

            using var ps = Process.Start(ProcessStartInfo);
            Output = ps.StandardOutput.ReadToEnd();
            Error = ps.StandardError.ReadLine() ?? string.Empty;

            ps.WaitForExit();
            Log.Verbose(Output);
            ExitCode = ps.ExitCode;

            HandleProcessEnd();
        });

        /// <summary>
        /// Handles the process end.
        /// </summary>
        /// <exception cref="dotnet.test.rerun.RerunException">command:\n\n\t\tdotnet {ProcessStartInfo.Arguments}</exception>
        private void HandleProcessEnd()
        {
            if (ExitCode != 0)
            {
                if (IsWellKnownError())
                {
                    ErrorCode = ErrorCode.WellKnownError;
                    Log.Warning(Error);
                }
                else if (HaveFailedTests())
                {
                    ErrorCode = ErrorCode.FailedTests;
                }
                else
                {
                    ErrorCode = ErrorCode.Error;
                    Log.Verbose(Error);
                    Log.Verbose($"Exit code {ExitCode}.");
                    throw new RerunException($"command:\ndotnet {ProcessStartInfo.Arguments}");
                }
            }
        }

        /// <summary>
        /// Determines whether [is well known error] [the specified exit code].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is well known error] [the specified exit code]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsWellKnownError() => ExitCode == 1 && WellKnownErrors.Contains(Error);

        /// <summary>
        /// Check if the output of dotnet test have in the last line failed tests
        /// </summary>
        /// <returns></returns>
        private bool HaveFailedTests() => ExitCode == 1 && Output.Split("\n")[^2].StartsWith("Failed!  - Failed:");
    }

    public enum ErrorCode
    {
        Success = 0,
        Error = 1,
        FailedTests = 2,
        WellKnownError = 99
    }
}