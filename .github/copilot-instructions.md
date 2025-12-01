# Copilot Instructions for dotnet-test-rerun

## Repository Overview

**Purpose**: dotnet-test-rerun is a wrapper for `dotnet test` that automatically reruns failed tests until they pass or reach a maximum number of attempts. Useful for handling intermittent test failures due to external factors like network, database availability, or race conditions.

**Language**: C# (.NET)  
**Project Type**: .NET Global Tool (CLI application) distributed via NuGet and Docker  
**Size**: ~1,700 lines of C# code across main project and test suites  
**Target Frameworks**: .NET 8.0 and .NET 9.0 (multi-targeting)  
**SDK Version**: .NET 9.0.X (specified in global.json)

## Build & Test Process

### Prerequisites
- .NET SDK 9.0.X (check with `dotnet --version`)
- The project uses `global.json` to enforce SDK version

### Build Commands

**ALWAYS follow this sequence**:

1. **Restore dependencies** (first time or after clean):
   ```bash
   dotnet restore
   ```
   - Takes ~17-18 seconds on first run
   - Automatically triggered by `dotnet build` if not run explicitly
   - Restores NuGet packages for all projects in solution

2. **Build the solution**:
   ```bash
   dotnet build --configuration Release
   ```
   - Takes ~13-14 seconds
   - Builds for both net8.0 and net9.0 target frameworks
   - Should complete with 0 warnings and 0 errors
   - Output: `src/bin/Release/net{8.0|9.0}/test-rerun.dll`
   - Can use `dotnet build` without configuration (defaults to Debug)

3. **Clean build artifacts**:
   ```bash
   dotnet clean
   ```
   - Takes ~2 seconds
   - Removes bin/ and obj/ directories

### Testing Commands

**Run all tests**:
```bash
dotnet test --configuration Release
```
- Takes ~3-4 minutes total
- Runs both UnitTests (159 tests, ~1s) and IntegrationTests (27 tests, ~3m14s)
- Integration tests are SLOW - this is normal
- All tests should pass (186 total)

**Run with coverage** (used in CI):
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov /p:CoverletOutput=coverage/lcov.info --configuration Release
```
- Generates coverage reports at:
  - `test/dotnet-test-rerun.UnitTests/coverage/lcov.info`
  - `test/dotnet-test-rerun.IntegrationTests/coverage/lcov.info`

**Run specific test project**:
```bash
dotnet test test/dotnet-test-rerun.UnitTests/dotnet-test-rerun.UnitTests.csproj
```

### Build Behavior Notes

- `dotnet build` automatically runs restore if needed
- The build should complete with no warnings or errors
- The build system uses Husky for git hooks (automatically installed on restore)
- Do NOT use `dotnet build --no-restore` on first build or after clean
- The `--no-restore` flag is safe only after an explicit `dotnet restore`

## Repository Structure

### Solution Structure
```
dotnet-test-rerun.sln          # Main solution file (4 projects)
├── src/                        # Main application
│   └── dotnet-test-rerun.csproj
└── test/                       # Test projects
    ├── dotnet-test-rerun.Common/
    ├── dotnet-test-rerun.UnitTests/
    └── dotnet-test-rerun.IntegrationTests/
```

### Source Code Layout (src/)
```
src/
├── Analyzers/              # Test result analysis (TRX parsing)
│   ├── ITestResultsAnalyzer.cs
│   └── TestResultsAnalyzer.cs
├── Domain/                 # Domain models
│   ├── TestFilter.cs
│   └── TestFilterCollection.cs
├── DotNetRunner/          # Process execution and dotnet test wrapping
│   ├── DotNetCoverageRunner.cs
│   ├── DotNetTestRunner.cs
│   ├── IDotNetCoverageRunner.cs
│   ├── IDotNetTestRunner.cs
│   ├── IProcessExecution.cs
│   └── ProcessExecution.cs
├── Enums/                 # Enumerations
│   ├── CoverageFormat.cs
│   ├── ErrorCode.cs
│   └── LoggerVerbosity.cs
├── Extensions/            # Utility extensions
│   └── StringExtensions.cs
├── Logging/              # Console output via Spectre.Console
│   ├── ILogger.cs
│   ├── Logger.cs
│   └── StatusContext.cs
├── RerunCommand/         # Main command implementation
│   ├── RerunCommand.cs
│   ├── RerunCommandConfiguration.cs
│   └── RerunException.cs
├── Program.cs            # Entry point with DI setup
├── dotnet-test-rerun.csproj
├── dockerfile            # Docker image build
└── docker-bake.hcl      # Docker build configuration
```

### Test Projects
- **dotnet-test-rerun.Common**: Shared test utilities
- **dotnet-test-rerun.UnitTests**: 159 fast unit tests (~1s)
- **dotnet-test-rerun.IntegrationTests**: 27 integration tests (~3m14s)
  - Contains test fixtures in `Fixtures/` for NUnit, XUnit, and MSTest examples

### Key Configuration Files

**Root directory files**:
- `global.json` - SDK version pinning (9.0.X) and custom scripts
- `dotnet-test-rerun.sln` - Solution file
- `.gitignore` - Standard .NET gitignore
- `README.md` - User documentation
- `RELEASE.md` - Release process documentation
- `CHANGELOG.md` - Version history

**Configuration directories**:
- `.config/dotnet-tools.json` - Local tools: versionize, husky, run-script
- `.husky/` - Git hooks for commit message linting
  - `commit-msg` - Hook that runs commit-lint.csx
  - `csx/commit-lint.csx` - Conventional commits validator
  - `task-runner.json` - Husky task definitions
- `.vscode/` - VSCode launch and task configurations
- `.devcontainer/` - Dev container configuration
- `.github/workflows/` - CI/CD pipelines

## CI/CD & Validation Pipeline

### GitHub Actions Workflows

**1. CI Pipeline** (`.github/workflows/ci.yml`)
- **Trigger**: PRs (opened, reopened, edited, synchronized), ignores `**.md` changes
- **Jobs**:
  - `commitlint`: Validates commit messages using conventional commits format
    - Installs: `@commitlint/cli`, `@commitlint/config-conventional`
    - Validates all commits in PR range
  - `build_and_test`: 
    - Runs on ubuntu-latest
    - Uses `global.json` for SDK setup
    - Command: `dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov /p:CoverletOutput=coverage/lcov.info --configuration Release`
    - Uploads coverage to Coveralls (separate flags for UnitTests and IntegrationTests)

**2. CD Pipeline** (`.github/workflows/cd.yml`)
- **Trigger**: Release published or manual workflow_dispatch
- **Jobs**:
  - `build`: Builds in Release configuration
  - `test`: Same as CI with coverage upload
  - `nuget`: Packs and pushes to nuget.org
    - Uses `kzrnm/get-net-sdk-project-versions-action` to extract version
    - Pushes from `src/nupkg/dotnet-test-rerun.{version}.nupkg`
  - `docker`: Builds and pushes Docker images
    - Uses `docker/bake-action` with `src/docker-bake.hcl`
    - Publishes to Docker Hub as `joaoopereira/dotnet-test-rerun`

**3. CodeQL Pipeline** (`.github/workflows/codeql.yml`)
- **Trigger**: Push to main, PRs to main, weekly schedule (Thursday 10:15)
- Scans C# code for security vulnerabilities
- Uses build-mode: none (no explicit build needed)

### Commit Message Requirements

**CRITICAL**: All commits MUST follow conventional commits format or CI will fail.

**Format**: `<type>(<scope>): <subject>` (1-90 characters)

**Valid types**: build, feat, ci, chore, docs, fix, perf, refactor, revert, style, test, wip

**Examples**:
- ✅ `feat(rerun): add delay option between retries`
- ✅ `fix: handle null reference in test filter`
- ✅ `docs: update README with new options`
- ❌ `Fixed a bug` (missing type prefix)
- ❌ `feat: fix` (subject too short, needs 4+ chars after colon)

The commit message validator runs via Husky pre-commit hook locally and via GitHub Actions on PRs.

## Key Dependencies

### Main Project (src/dotnet-test-rerun.csproj)
- **Microsoft.Extensions.DependencyInjection** (9.0.8): DI container
- **Spectre.Console** (0.50.0): Rich terminal UI
- **System.CommandLine** (2.0.0-beta4): CLI argument parsing
- **System.IO.Abstractions** (22.0.16): File system abstraction for testability
- **TrxFileParser** (4.3.0): Parses .trx test result files

### Test Projects
- **xunit** (2.9.3): Test framework
- **NSubstitute** (5.3.0): Mocking library
- **AwesomeAssertions** (8.1.0): Fluent assertions
- **coverlet**: Code coverage (msbuild & collector packages)
- **Spectre.Console.Testing** (0.51.1): Testing utilities for Spectre.Console

## Common Development Scenarios

### Making Code Changes

1. **Before making changes**:
   ```bash
   dotnet restore  # Ensure dependencies are current
   dotnet build --configuration Release  # Verify current state
   dotnet test --configuration Release   # Run tests (takes 3-4 minutes)
   ```

2. **After making changes**:
   ```bash
   dotnet build --configuration Release  # Quick build check
   dotnet test --configuration Release   # Full test suite
   ```

3. **Testing specific changes**:
   ```bash
   # For quick feedback, run only unit tests
   dotnet test test/dotnet-test-rerun.UnitTests/dotnet-test-rerun.UnitTests.csproj
   ```

### Adding New Features

- Main command logic: `src/RerunCommand/RerunCommand.cs`
- Command configuration: `src/RerunCommand/RerunCommandConfiguration.cs`
- Test execution: `src/DotNetRunner/DotNetTestRunner.cs`
- Result parsing: `src/Analyzers/TestResultsAnalyzer.cs`
- Console output: `src/Logging/Logger.cs`

### Testing Approach

- Unit tests go in `test/dotnet-test-rerun.UnitTests/`
- Use NSubstitute for mocking dependencies
- Follow existing test patterns in the codebase
- Integration tests use fixture projects in `test/dotnet-test-rerun.IntegrationTests/Fixtures/`
- Test fixtures include NUnit, XUnit, and MSTest examples

## Docker Build

Build Docker images locally:
```bash
cd src
docker buildx bake -f docker-bake.hcl
```

This builds images for both .NET 8.0 and 9.0 targets.

## Package Release Process

See `RELEASE.md` for detailed release instructions.

**Pre-release**:
```bash
dotnet r bump  # Bumps to alpha version (e.g., 1.4.2-alpha.0)
```

**Production release**:
```bash
dotnet r bump:live  # Bumps to production version (e.g., 1.4.2)
```

Both commands use the `versionize` tool and automatically update CHANGELOG.md and push git tags.

## Important Notes

### Build Behavior
- The build should complete cleanly with no warnings or errors
- If warnings appear, they should be investigated and fixed

### Husky Integration
- Husky is automatically installed during first restore/build
- Sets up git hooks for commit message linting
- The Husky target in the csproj file runs before Restore
- When `HUSKY=0` environment variable is set, hooks are disabled
- When `DockerBuild=true` is set, Husky is skipped

### Test Execution Time
- Unit tests: Fast (~1s for 159 tests)
- Integration tests: Slow (~3m14s for 27 tests)
- Full test suite: ~3-4 minutes (186 total tests)
- This is normal - integration tests build and run actual test projects

### Language Limitation
- The tool only supports English output from `dotnet test`
- Localized output from dotnet test may cause parsing failures

## Troubleshooting

**Problem**: Build fails with missing SDK version  
**Solution**: Check `dotnet --list-sdks` includes 9.0.X, or update global.json

**Problem**: Tests time out  
**Solution**: Integration tests take 3+ minutes, increase timeout or run unit tests only

**Problem**: Commit rejected by CI  
**Solution**: Ensure commit message follows conventional commits format (see above)

**Problem**: Husky errors during build  
**Solution**: Ensure Node.js is installed (required for commitlint in CI), or set `HUSKY=0` to skip

**Problem**: Can't restore packages  
**Solution**: Check network connection, clear NuGet cache: `dotnet nuget locals all --clear`

## Quick Reference

**Trust these instructions** - they have been validated against the actual codebase. Only search for additional information if you encounter errors or missing details not covered here.

**Most common commands**:
- Build: `dotnet build --configuration Release`
- Test: `dotnet test --configuration Release`
- Clean: `dotnet clean`
- Pack: `dotnet pack --configuration Release`
- Run tool locally: `dotnet run --project src/dotnet-test-rerun.csproj -- [args]`
