using dotnet.test.rerun.Enums;
using dotnet.test.rerun.RerunCommand;

namespace dotnet.test.rerun.DotNetTestRunner;

public interface IDotNetTestRunner
{
    public Task Test(RerunCommandConfiguration config, string resultsDirectory);

    public ErrorCode GetErrorCode();
}