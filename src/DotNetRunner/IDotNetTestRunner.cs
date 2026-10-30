using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Configuration;
using dotnet.test.rerun.Execution;

namespace dotnet.test.rerun.DotNetRunner;

public interface IDotNetTestRunner
{
    public Task Test(RerunOptions options, RunRequest request, CancellationToken cancellationToken = default);

    public ErrorCode GetErrorCode();
}