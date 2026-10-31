---
name: add-cli-option
description: Checklist for adding a new command-line option/flag to dotnet-test-rerun (config, wiring, forwarding to dotnet test or not, tests, docs). Use when a feature needs a new --option.
---

# Add a CLI option

Reference implementations: `--logPassedTests` (forwarded to `dotnet test` as an extra logger) and `--logTestResults` (tool-only, never forwarded).

## 1. Options and parsing

1. `src/Configuration/RerunOptions.cs`: add an init-only property with the same default as the CLI: `public bool MyFlag { get; init; }`.
2. `src/Cli/RerunCliDefinition.cs`: add the option in `#region Options`:
   ```csharp
   private readonly Option<bool> MyFlagOption = new("--myFlag")
   {
       Description = "One sentence, ends with a period.",
       Arity = ArgumentArity.Zero   // for boolean flags
   };
   ```
   Use camelCase names for tool-specific options (`--rerunMaxAttempts`, `--logPassedTests`). Use dotnet's own names for options passed through (`--no-build`, `--results-directory`).
3. Register it in `AddTo(Command cmd)`: `cmd.Options.Add(MyFlagOption);`.
4. Map it in `ToOptions(ParseResult)`: `MyFlag = parseResult.GetValue(MyFlagOption),`.
5. Decide about forwarding:
   - If `dotnet test` must receive it, add it to `src/Execution/DotNetTestArguments.cs` (`AddOption`/`AddFlag`; each value is one list item, never pre-quoted).
   - If it is tool-only, do **not** touch `DotNetTestArguments`, and add a test proving it is not forwarded.

## 2. Behaviour

The rerun loop and end-of-run logic live in `src/Execution/RerunOrchestrator.cs`. Anything that needs TRX files must run before the `--deleteReports` block, which removes them.

## 3. Tests (`test/dotnet-test-rerun.UnitTests/`)

- `Cli/RerunCliDefinitionTests.cs`:
  - Add an `[InlineData("--myFlag", "<exact description>")]` to `RerunCliDefinition_AddTo_ShouldConfigureOptions`.
  - Test the default value and the value when passed. `RerunCliDefinition_ToOptions_DefaultsMatchRerunOptionsDefaults` fails if the `RerunOptions` default differs from the CLI default.
- `Execution/DotNetTestArgumentsTests.cs`: test the `DotNetTestArguments.Build` output.
- `Execution/RerunOrchestratorTests.cs`:
  - Use `var config = InitialConfigurationSetup(extraParams: "--myFlag", filter: string.Empty);` and `command.RunAsync(config)`.
  - Use `Substitute.For<ILogger>()` when you need to assert log output (`logger.Received(1).Information("...")`).
  - `ReturnsOutcomes(dotNetTestRunner, ErrorCode.FailedTests)` makes every attempt fail. `ReturnsOutcomes(dotNetTestRunner, ErrorCode.FailedTests, ErrorCode.Success)` makes the run pass on the second attempt.

## 4. Docs

- `docs/usage.md` (command reference): a row in the "Rerun options" table (tool-only) or the "Options passed to `dotnet test`" table. This is the only full list of options.
- `docs/configuration.md` (Guides): a `##` section only when the option needs advice or an example.
- `README.md`: the "Main options" table, only if most users need the option.

## 5. Validate

Run the unit tests, then `verify-end-to-end` with a fixture project. Commit as `feat: ...` with `Closes #N`.
