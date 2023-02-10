using System.Diagnostics;
using System.IO.Abstractions;

namespace dotnet.test.rerun
{
    public class dotnet
    {
        private readonly Logger Log;
        private readonly ProcessStartInfo ProcessStartInfo;

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