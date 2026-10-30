# CLAUDE.md

`dotnet-test-rerun` wraps `dotnet test`. It reads the TRX results of failed tests, builds a `--filter` for them and reruns them up to `--rerunMaxAttempts` times.

The full reference (layout, CI, dependencies) is in `.github/copilot-instructions.md`. Some numbers there are out of date: the SDK is 10.x and tests target `net10.0`. This file adds what is useful for working fast.

## Setup in a cloud container

`dotnet` is not preinstalled, and `builds.dotnet.microsoft.com` is blocked (so `dotnet-install.sh` fails). Run this once per session (~1 min):

```bash
DEBIAN_FRONTEND=noninteractive apt-get install -y -qq dotnet-sdk-10.0 dotnet-runtime-8.0 >/dev/null
# The Ubuntu feed ships SDK 10.0.1xx, but global.json pins 10.0.302. Rewrite the pin locally only:
sed -i 's/"10.0.302"/"10.0.100"/' global.json && git update-index --skip-worktree global.json
```

- `.NET 8` runtime is needed because the integration fixtures target `net8.0`.
- Never commit the `global.json` change. Before `git checkout` to a branch with a different `global.json`, run `git update-index --no-skip-worktree global.json && git checkout global.json`.
- There is no .NET 9 runtime in the feed. The integration test `DotnetTestRerun_FailingXUnit_WithMultipleFrameworks_Fails` always fails locally for that reason (26/27 is the expected local result). CI runs it.
- The clone is shallow (50 commits). Run `git fetch --unshallow origin` before comparing branches, otherwise ahead/behind counts are wrong.

## Fast commands

```bash
dotnet test test/dotnet-test-rerun.UnitTests/dotnet-test-rerun.UnitTests.csproj      # ~20s, run always
dotnet test test/dotnet-test-rerun.IntegrationTests/dotnet-test-rerun.IntegrationTests.csproj  # ~6min, run before pushing logic changes
```

The Husky pre-commit hook runs the unit tests on every `git commit` (~30s). The commit-msg hook enforces conventional commits: `type(scope): subject`, at most 90 characters, no trailing period.

## Where things live

| Change | File |
|---|---|
| New CLI option | `src/Cli/RerunCliDefinition.cs` + `src/Configuration/RerunOptions.cs` (see skill `add-cli-option`) |
| Arguments passed to `dotnet test` / `dotnet-coverage` | `src/Execution/DotNetTestArguments.cs`, `CoverageMergeArguments.cs` (argument lists, quoted by the runtime) |
| Rerun loop, end-of-run behaviour | `src/Execution/RerunOrchestrator.cs` (returns a `RerunOutcome`; `Cli/RerunRootCommand.cs` maps it to the exit code) |
| TRX parsing and filter building | `src/Analyzers/TestResultsAnalyzer.cs` |
| Exit-code and failure detection from `dotnet test` output | `src/DotNetRunner/DotNetTestRunner.cs` |
| Console output (Spectre markup, escaped) | `src/Logging/Logger.cs` |
| Fixture projects for integration tests | `test/dotnet-test-rerun.IntegrationTests/Fixtures/` |

## Filter gotchas (frequent bug source)

- `dotnet test --filter` goes through MSBuild (`VSTestTestCaseFilter`). A literal `"` in the value breaks parsing, even doubled. Encode it as `%22`; MSBuild decodes it before VSTest.
- `( ) ? { }` must be backslash-escaped (`EscapeAll`). Spaces in a FullyQualifiedName are escaped as `\ `.
- NUnit parameterised tests use `FullyQualifiedName~Name(args)`. xUnit uses `FullyQualifiedName=Name&DisplayName~args`. MSTest uses only `FullyQualifiedName=`.
- xUnit's TRX `TestMethod.Name` omits Theory parameters. NUnit and MSTest include them.
- When a user-supplied `--filter` exists, failed tests are combined as `(original)&(failed)` via `FilterComposer.Combine`.

## Release and issue workflow (observed practice)

- Fix or feature PRs are often opened by Copilot as drafts on `copilot/*` branches.
- To let a reporter try a PR, add the `preview` label. `.github/workflows/preview.yml` publishes `X.Y.Z-pr.<PR>.<run>` to NuGet and comments on the PR. Then share that version on the issue. See `RELEASE.md`.
- Never commit version bumps (`dotnet r bump`) on PR branches. Version and CHANGELOG only change on `main`.
- Older PRs may still contain `chore(release): X.Y.Z-alpha.N` commits from the previous process. Drop them when rebasing; the tags keep the original commits.
- Never run `dotnet r bump:live` or merge PRs without the maintainer asking.
- Skills: `triage-issues` (review issues, PRs, preview packages and feedback), `add-cli-option`, `verify-end-to-end`.
