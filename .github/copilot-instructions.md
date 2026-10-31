# Copilot Instructions for dotnet-test-rerun

## Repository overview

dotnet-test-rerun is a .NET global tool (`test-rerun`) that wraps `dotnet test`. It reads the TRX results of failed tests, builds a `--filter` for them and reruns them up to `--rerunMaxAttempts` times. It is distributed on NuGet and as Docker images.

- **Language**: C#
- **Target frameworks**: `net8.0`, `net9.0`, `net10.0` (`src/dotnet-test-rerun.csproj`)
- **SDK**: .NET 10, pinned in `global.json`
- **User documentation**: `README.md` and the `docs/` folder (GitHub Pages, see `docs/README.md`)

## Build and test

```bash
dotnet build
dotnet test test/dotnet-test-rerun.UnitTests/dotnet-test-rerun.UnitTests.csproj          # fast, run on every change
dotnet test test/dotnet-test-rerun.IntegrationTests/dotnet-test-rerun.IntegrationTests.csproj  # several minutes
```

- The integration tests run the real tool against the projects in `test/dotnet-test-rerun.IntegrationTests/Fixtures/`. Some fixtures target `net8.0` and `net9.0`, so those runtimes must be installed.
- CI (`.github/workflows/ci.yml`) runs all tests with coverage:
  `dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov /p:CoverletOutput=coverage/lcov.info --configuration Release`
- The build should have no warnings.
- Run the tool from source: `dotnet run --project src/dotnet-test-rerun.csproj --framework net10.0 -- <args>`

## Source layout

```
src/
├── Analyzers/        # TRX parsing (TestResultsAnalyzer) and filter strategies per framework (Filters/)
├── Cli/              # Options (RerunCliDefinition) and exit code mapping (RerunRootCommand)
├── Configuration/    # RerunOptions, immutable parsed options
├── Domain/           # TestFilter, TestFilterCollection
├── DotNetRunner/     # Runs dotnet test / dotnet-coverage and classifies how a run ended
├── Enums/            # ErrorCode, RerunOutcome, CoverageFormat, LoggerVerbosity
├── Execution/        # Rerun loop (RerunOrchestrator), argument lists, filter batching and composition
├── Extensions/
├── Logging/          # Console output with Spectre.Console
├── Program.cs        # DI setup, top-level exception handling, exit codes
└── RerunException.cs # Error reported to the user without a stack trace
test/
├── dotnet-test-rerun.Common/            # Shared test utilities
├── dotnet-test-rerun.UnitTests/         # xUnit + NSubstitute + AwesomeAssertions
└── dotnet-test-rerun.IntegrationTests/  # Fixtures/ holds the test projects the tool runs
Dockerfile, docker-bake.hcl              # Docker images (repository root)
```

## Where to make changes

| Change | File |
|---|---|
| New CLI option | `src/Cli/RerunCliDefinition.cs` + `src/Configuration/RerunOptions.cs` |
| Arguments passed to `dotnet test` / `dotnet-coverage` | `src/Execution/DotNetTestArguments.cs`, `CoverageMergeArguments.cs` |
| Rerun loop, end-of-run behaviour | `src/Execution/RerunOrchestrator.cs` |
| TRX parsing and filter building | `src/Analyzers/TestResultsAnalyzer.cs`, `src/Analyzers/Filters/` |
| Failure detection from `dotnet test` | `src/DotNetRunner/DotNetTestRunner.cs` |
| Console output | `src/Logging/Logger.cs` |

When an option or behaviour changes, update `docs/usage.md` (the command reference) and, if needed, `docs/configuration.md`, `docs/how-it-works.md` and `README.md`.

## Commit messages

Conventional Commits, enforced by a Husky commit-msg hook and by CI:

- Format: `type(scope): subject`, at most 90 characters, no trailing period.
- Types: `build`, `feat`, `ci`, `chore`, `docs`, `fix`, `perf`, `refactor`, `revert`, `style`, `test`.

The Husky pre-commit hook runs the unit tests. Set `HUSKY=0` to disable the hooks.

## Workflows

- `ci.yml`: pull requests (ignores changes to `**.md`, `docs/**`, `.github/**`). Commit lint, build, all tests, Coveralls.
- `cd.yml`: on a published release. Tests, NuGet push, Docker images via `docker buildx bake`.
- `preview.yml`: on pull requests with the `preview` label. Publishes `X.Y.Z-pr.<PR>.<run>` to NuGet.
- `codeql.yml`: code scanning.

## Releases

See `RELEASE.md`. Never bump versions or edit `CHANGELOG.md` on pull request branches: `versionize` (`dotnet r bump`, `dotnet r bump:live`) generates them on `main`.
