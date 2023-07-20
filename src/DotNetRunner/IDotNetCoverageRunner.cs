using dotnet.test.rerun.RerunCommand;

namespace dotnet.test.rerun.DotNetRunner;

public interface IDotNetCoverageRunner
{
    public Task Merge(RerunCommandConfiguration config, string resultsDirectory, DateTime startDate);
}