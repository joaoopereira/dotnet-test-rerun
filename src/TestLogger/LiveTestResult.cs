namespace dotnet.test.rerun.TestLogger;

/// <summary>
/// One test result, written by <see cref="LiveTestLogger"/> as a JSON line and read by the tool while dotnet test runs.
/// </summary>
public sealed record LiveTestResult(string Name, bool Passed, string? ErrorMessage, string? StackTrace)
{
    public const string LoggerFriendlyName = "dotnet-test-rerun-live";
    public const string DirectoryParameter = "Directory";
    public const string FileExtension = ".jsonl";

    /// <summary>
    /// Same name as the TRX-based output: the fully qualified name, plus the parameters of the display name
    /// when the framework leaves them out of the fully qualified name (xUnit theories).
    /// </summary>
    public static string BuildName(string fullyQualifiedName, string? displayName)
    {
        var parametersStartIndex = displayName?.IndexOf('(') ?? -1;
        return parametersStartIndex >= 0 && !fullyQualifiedName.Contains('(')
            ? fullyQualifiedName + displayName![parametersStartIndex..]
            : fullyQualifiedName;
    }
}
