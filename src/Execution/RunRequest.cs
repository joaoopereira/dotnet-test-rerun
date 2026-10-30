namespace dotnet.test.rerun.Execution;

/// <summary>
/// What a single dotnet test invocation runs: the filter, the target framework and where results go.
/// </summary>
public sealed record RunRequest(string? Filter, string? Framework, string ResultsDirectory);
