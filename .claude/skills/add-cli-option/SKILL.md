---
name: add-cli-option
description: Checklist for adding a new command-line option/flag to dotnet-test-rerun (config, wiring, forwarding to dotnet test or not, tests, docs). Use when a feature needs a new --option.
---

# Add a CLI option

Reference implementations: `--logPassedTests` (forwarded to `dotnet test` as an extra logger) and `--generateTestCommand` (tool-only, never forwarded).

## 1. `src/RerunCommand/RerunCommandConfiguration.cs`

1. Add a property in `#region Properties`: `public bool MyFlag { get; internal set; }`.
2. Add the option in `#region Options`:
   ```csharp
   private readonly Option<bool> MyFlagOption = new("--myFlag")
   {
       Description = "One sentence, ends with a period.",
       Arity = ArgumentArity.Zero   // for boolean flags
   };
   ```
   Use camelCase names for tool-specific options (`--rerunMaxAttempts`, `--logPassedTests`). Use dotnet's own names for options passed through (`--no-build`, `--results-directory`).
3. Register it in `Set(Command cmd)`: `cmd.Options.Add(MyFlagOption);`.
4. Read it in `GetValues(ParseResult)`: `MyFlag = parseResult.GetValue(MyFlagOption);`.
5. Decide about forwarding:
   - If `dotnet test` must receive it, add it to `GetTestArgumentList` with `AddArguments(...)`.
   - If it is tool-only, do **not** touch `GetTestArgumentList`, and add a test proving it is not forwarded.

## 2. Behaviour

The rerun loop and end-of-run logic live in `src/RerunCommand/RerunCommand.cs`. Anything that needs TRX files must run before `DeleteFiles()`, because `--deleteReports` removes them.

## 3. Tests (`test/dotnet-test-rerun.UnitTests/RerunCommand/`)

- `RerunCommandConfigurationTests.cs`:
  - Add an `[InlineData("--myFlag", "<exact description>")]` to `RerunCommandConfiguration_Set_ShouldConfigureOptions`.
  - Test the default value and the value when passed.
  - Test the `GetTestArgumentList` output.
- `RerunCommandTests.cs`:
  - Use `InitialConfigurationSetup(config, extraParams: "--myFlag", filter: string.Empty)`.
  - Use `Substitute.For<ILogger>()` when you need to assert log output (`logger.Received(1).Information("...")`).
  - `GetErrorCode().Returns(ErrorCode.FailedTests)` makes every attempt fail. `.Returns(ErrorCode.FailedTests, ErrorCode.Success)` makes the run pass on the second attempt.

## 4. Docs (all three)

- `README.md`: the options table in the matching section (e.g. "Logging & Output").
- `docs/usage.md`: a `**\`--myFlag\`**` bullet block with an example.
- `docs/configuration.md`: a `###` section when the option needs explanation.

## 5. Validate

Run the unit tests, then `verify-end-to-end` with a fixture project. Commit as `feat: ...` with `Closes #N`.
