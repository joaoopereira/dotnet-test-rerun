using dotnet.test.rerun.Configuration;

namespace dotnet.test.rerun.DotNetRunner;

public interface IDotNetCoverageRunner
{
    public Task Merge(RerunOptions options, string resultsDirectory, DateTime startDate, CancellationToken cancellationToken = default);
}