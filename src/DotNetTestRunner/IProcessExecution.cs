using System.Diagnostics;

namespace dotnet.test.rerun.DotNetTestRunner;

public interface IProcessExecution
{
    public Task<Process?> Start(ProcessStartInfo processStartInfo);
    public void FetchOutput(Process process);
    public void FetchError(Process process);
    public Task<int> End(Process process);
    public string GetOutput();
    public string GetError();
}