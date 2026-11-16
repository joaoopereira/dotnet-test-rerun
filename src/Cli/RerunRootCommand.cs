using System.CommandLine;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Execution;
using dotnet.test.rerun.Logging;

namespace dotnet.test.rerun.Cli;

public class RerunRootCommand : RootCommand
{
    /// <summary>
    /// Exit code for a run cancelled with Ctrl+C, as shells report SIGINT (128 + 2).
    /// </summary>
    public const int CancelledExitCode = 130;

    private readonly ILogger Log;

    public RerunRootCommand(ILogger logger,
        RerunCliDefinition cliDefinition,
        RerunOrchestrator orchestrator) :
        base("wrapper of dotnet test command with the extra option to automatic rerun failed tests")
    {
        Log = logger;

        // Allow unmatched tokens (e.g., MSBuild arguments) to pass through
        TreatUnmatchedTokensAsErrors = false;

        cliDefinition.AddTo(this);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var options = cliDefinition.ToOptions(parseResult);
            logger.SetLogLevel(options.LogLevel);
            var outcome = await orchestrator.RunAsync(options, cancellationToken);
            return ToExitCode(outcome);
        });
    }

    /// <summary>
    /// Parses and runs the command, and turns the exceptions that reach the top into exit codes.
    /// </summary>
    public async Task<int> RunAsync(string[] args)
    {
        try
        {
            // Let exceptions reach the handlers below instead of System.CommandLine's default handler,
            // so RerunException is reported as a plain message and cancellation gets its own exit code.
            return await Parse(args).InvokeAsync(new InvocationConfiguration { EnableDefaultExceptionHandler = false });
        }
        catch (OperationCanceledException)
        {
            Log.Warning("Test run cancelled.");
            return CancelledExitCode;
        }
        catch (RerunException exception)
        {
            Log.Error(exception.Message);
            Log.Debug(exception.StackTrace ?? string.Empty);
            return 1;
        }
        catch (Exception exception)
        {
            Log.Exception(exception);
            return 1;
        }
    }

    public static int ToExitCode(RerunOutcome outcome)
        => outcome is RerunOutcome.Passed or RerunOutcome.PassedAfterRerun ? 0 : 1;
}
