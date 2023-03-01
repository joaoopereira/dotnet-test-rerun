using System.Diagnostics;
using dotnet.test.rerun.Logging;
using dotnet.test.rerun.RerunCommand;

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
        /// <param name="config">The config.</param>
        /// <param name="resultsDirectory">The results directory.</param>
        public void Test(RerunCommandConfiguration config, string resultsDirectory)
        {
            string arguments = config.GetArgumentList();

            if (string.IsNullOrEmpty(resultsDirectory) is false)
                arguments = string.Concat(arguments, $" --results-directory {resultsDirectory}");

            Run(arguments);
        }

        /// <summary>
        /// Runs dotnet test with the specified arguments.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        private void Run(string arguments)
        {
            Log.Debug($"working directory: {ProcessStartInfo.WorkingDirectory}");
            Log.Debug($"forking {arguments}");
            ProcessStartInfo.Arguments = arguments;

            using var ps = Process.Start(ProcessStartInfo);
            ps.OutputDataReceived += (sender, args) =>
            {
                Log.Verbose(args.Data);
                Output += $"\n{args.Data}";
            };
            ps.ErrorDataReceived += (sender, args) =>
            {
                Log.Error(args.Data);
                Error += $"\n{args.Data}";
            };
            ps.BeginOutputReadLine();
            ps.BeginErrorReadLine();

            ps.WaitForExit();
            ExitCode = ps.ExitCode;

            HandleProcessEnd();
        }

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