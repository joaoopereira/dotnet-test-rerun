using dotnet.test.rerun.Configuration;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Execution;

namespace dotnet.test.rerun.DotNetRunner;

public sealed record TestRunResult(ErrorCode Outcome, int ExitCode);

public interface IDotNetTestRunner
{
    public Task<TestRunResult> Test(RerunOptions options, RunRequest request, CancellationToken cancellationToken = default);
}
