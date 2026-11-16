---
layout: default
title: Upgrading from 4.x
nav_order: 8
---

# Upgrading from 4.x
{: .no_toc }

Version 5.0 has two breaking changes: reports are written to one folder per run, and the tool needs the .NET 8 runtime or later. Most setups only need to change the pattern that publishes test reports in CI.

<details open markdown="block">
  <summary>Contents</summary>
  {: .text-delta }
- TOC
{:toc}
</details>

## Checklist

1. Install the .NET 8 runtime or later on the machine that runs `test-rerun`.
2. In CI, publish TRX, JUnit and coverage files with a recursive pattern, such as `TestResults/**/*.trx`.
3. If you added extra quotes or backslashes to arguments so they survived the tool, remove them.
4. Check that `--loglevel` is one of `Debug`, `Verbose`, `Information`, `Warning` or `Error`.

## Breaking changes

### Reports are written to `attempt-N` folders

In 4.x, every run wrote its reports directly to `--results-directory`. In 5.0, each run writes to its own subfolder: `attempt-0` for the first run, `attempt-1` for the first rerun, and so on. See [Results folders](how-it-works#results-folders).

Change CI steps that read reports to a recursive pattern:

| CI system | 4.x | 5.0 |
|---|---|---|
| GitHub Actions, GitLab CI | `TestResults/*.trx` | `TestResults/**/*.trx` |
| Azure DevOps `PublishTestResults@2` | `testResultsFiles: '*.trx'` | `testResultsFiles: '**/*.trx'` |

- `attempt-N` folders left by a previous run in the same results directory are deleted when the tool starts. Copy them elsewhere first if you need to keep them.
- `--deleteReports` deletes the TRX files in the `attempt-N` folders.
- The merged coverage file from `--mergeCoverageFormat` is still written to the results directory itself, not to an `attempt-N` folder.

### The tool needs .NET 8 or later

The tool now targets `net8.0`, `net9.0` and `net10.0`. It no longer installs on a machine that only has the .NET 6 runtime. Projects under test can still target `net6.0` or older, as long as their SDK and runtime are installed.

## Behaviour changes

These are not breaking for documented usage, but you may notice them.

- **Arguments are passed as they are.** The tool hands each argument to `dotnet test` as a separate argument, and the runtime quotes it for the current OS. Values with spaces, such as a `--filter` or inline run settings after `--`, no longer need extra escaping. If you added `\"` or nested quotes to work around 4.x, remove them.
- **Unknown `--loglevel` values are an error.** 4.x silently fell back to the default.
- **`--rerunFailedThreshold` applies to the first run only.** In 4.x it was checked again after each rerun, where the failure percentage is always high because only failed tests run, so later reruns were often skipped.
- **`--delay` only waits before a rerun.** It no longer waits when all tests passed or when no rerun is needed.
- **Failures are detected from the exit code and the TRX files.** Reruns now work when `dotnet test` prints its output in another language. A `trx` logger is always added if your `--logger` options have none.
- **Several target frameworks.** An attempt fails if any target framework failed, not only the last one. Reruns still run every target framework of the project.
- **Long filters are split.** A rerun filter longer than 8000 characters is run as several `dotnet test` calls within the same attempt, to stay under the Windows command-line limit.
- **Docker images are based on the .NET SDK**, so `dotnet test` works inside them. Images up to `5.0.0-alpha.0` could not run tests. See [Docker](docker).

## New in 5.0

- `--generateTestCommand` prints a `dotnet test` command that runs only the tests that still fail at the end. See [Reproducing failed tests](configuration#reproducing-failed-tests).
