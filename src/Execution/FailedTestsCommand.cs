using dotnet.test.rerun.Configuration;

namespace dotnet.test.rerun.Execution;

/// <summary>
/// Builds a standalone dotnet test command that runs only the failed tests,
/// so it can be copied and executed without dotnet-test-rerun.
/// </summary>
public static class FailedTestsCommand
{
    public static string Build(RerunOptions options, string failedTestsFilter)
    {
        var args = new List<string> { "test" };
        if (!string.IsNullOrWhiteSpace(options.Path))
            args.Add(options.Path);
        args.Add("--filter");
        args.Add(FilterComposer.Combine(options.Filter, failedTestsFilter));
        AddOption(args, "--settings", options.Settings);
        AddOption(args, "-c", options.Configuration);
        AddOption(args, "--framework", options.Framework);
        return CommandLineDisplay.Render("dotnet", args);
    }

    private static void AddOption(List<string> args, string name, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        args.Add(name);
        args.Add(value);
    }
}
