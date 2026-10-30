using System.Diagnostics;

namespace dotnet.test.rerun.DotNetRunner;

public interface IProcessExecution
{
    public Task<Process?> Start(ProcessStartInfo processStartInfo);
    public void FetchOutput(Process process);
    public void FetchError(Process process);
    public Task<int> End(Process process, CancellationToken cancellationToken = default);
    public string GetOutput();
    public string GetError();
}