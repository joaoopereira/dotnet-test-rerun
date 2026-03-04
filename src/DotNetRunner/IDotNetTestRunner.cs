using dotnet.test.rerun.Enums;
using dotnet.test.rerun.RerunCommand;

namespace dotnet.test.rerun.DotNetRunner;

public interface IDotNetTestRunner
{
    public Task Test(RerunCommandConfiguration config, string resultsDirectory);

    public ErrorCode GetErrorCode();
    
    public string GetLastTestOutput();
}