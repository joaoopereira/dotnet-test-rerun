namespace dotnet.test.rerun.Execution;

public static class FilterComposer
{
    /// <summary>
    /// Restricts the user's original filter, if any, to the failed tests.
    /// </summary>
    public static string Combine(string? originalFilter, string failedTestsFilter)
        => string.IsNullOrWhiteSpace(originalFilter)
            ? failedTestsFilter
            : $"({originalFilter})&({failedTestsFilter})";
}
