using dotnet.test.rerun.Configuration;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Execution;

namespace dotnet.test.rerun.DotNetRunner;

/// <param name="LiveTestResults">Number of test results printed during the run by --logTestResults.</param>
public sealed record TestRunResult(ErrorCode Outcome, int ExitCode, int LiveTestResults = 0);

public interface IDotNetTestRunner
{
    public Task<TestRunResult> Test(RerunOptions options, RunRequest request, CancellationToken cancellationToken = default);
}
