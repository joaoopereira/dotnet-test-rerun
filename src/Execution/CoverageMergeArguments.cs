using dotnet.test.rerun.Configuration;
using dotnet.test.rerun.Enums;

namespace dotnet.test.rerun.Execution;

/// <summary>
/// Builds the dotnet-coverage merge arguments as a list, so the runtime quotes each one for the current OS.
/// </summary>
public static class CoverageMergeArguments
{
    public static IReadOnlyList<string> Build(RerunOptions options, IEnumerable<string> coverageFiles, string resultsDirectory)
    {
        var format = options.MergeCoverageFormat
                     ?? throw new ArgumentOutOfRangeException(nameof(options.MergeCoverageFormat), null, null);

        var args = new List<string>
        {
            "merge",
            "-o", Path.Combine(resultsDirectory, $"merged.{GetExtension(format)}"),
            "-f", format.ToString().ToLowerInvariant()
        };
        args.AddRange(coverageFiles);
        return args;
    }

    private static string GetExtension(CoverageFormat format)
        => format switch
        {
            CoverageFormat.Cobertura => "cobertura.xml",
            CoverageFormat.Xml => "xml",
            CoverageFormat.Coverage => "coverage",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
}
