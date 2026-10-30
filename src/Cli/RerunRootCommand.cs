using System.CommandLine;
using dotnet.test.rerun.Enums;
using dotnet.test.rerun.Execution;
using dotnet.test.rerun.Logging;

namespace dotnet.test.rerun.Cli;

public class RerunRootCommand : RootCommand
{
    public RerunRootCommand(ILogger logger,
        RerunCliDefinition cliDefinition,
        RerunOrchestrator orchestrator) :
        base("wrapper of dotnet test command with the extra option to automatic rerun failed tests")
    {
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

    public static int ToExitCode(RerunOutcome outcome)
        => outcome is RerunOutcome.Passed or RerunOutcome.PassedAfterRerun ? 0 : 1;
}
