using dotnet.test.rerun.Configuration;

namespace dotnet.test.rerun.DotNetRunner;

public interface IDotNetCoverageRunner
{
    /// <summary>
    /// Merges the coverage reports found in <paramref name="searchDirectories"/> into <paramref name="outputDirectory"/>.
    /// </summary>
    public Task Merge(RerunOptions options, IEnumerable<string> searchDirectories, string outputDirectory, CancellationToken cancellationToken = default);
}