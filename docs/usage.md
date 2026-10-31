---
layout: default
title: Command reference
nav_order: 3
---

# Command reference
{: .no_toc }

<details open markdown="block">
  <summary>Contents</summary>
  {: .text-delta }
- TOC
{:toc}
</details>

## Syntax

```bash
test-rerun [<path>] [options] [MSBuild arguments] [-- <inline run settings>]
```

`<path>` is passed to `dotnet test` unchanged: a project, a solution, a directory or a test `.dll`. Without it, `dotnet test` looks for a project or solution in the current directory. Only one path is accepted.

Run `test-rerun --help` to see the options of the installed version.

## Rerun options

These options control the tool itself and are not passed to `dotnet test`.

| Option | Default | Description |
|---|---|---|
| `--rerunMaxAttempts <n>` | `3` | Maximum number of reruns after the first run. With `3`, `dotnet test` runs at most 4 times. |
| `--rerunMaxFailedTests <n>` | `-1` | Do not rerun when more than `n` tests failed. `-1` (or `0`) means no limit. |
| `--rerunFailedThreshold <percent>` | `-1` | Do not rerun when more than this percentage of the tests failed. Accepts decimals (`12.5`). `-1` means no limit; `0` means never rerun. |
| `--delay`, `-d <seconds>` | `0` | Wait this many seconds before each rerun. Not applied when nothing is rerun. |
| `--deleteReports` | off | Delete the TRX files of every run at the end, and the `attempt-N` folders left empty. Files from other loggers (for example JUnit) are kept. |
| `--mergeCoverageFormat <format>` | — | Merge the coverage files of all runs into one file: `Coverage`, `Cobertura` or `Xml`. Requires the [`dotnet-coverage`](https://learn.microsoft.com/dotnet/core/additional-tools/dotnet-coverage) tool. See [Code coverage](configuration#code-coverage). |
| `--loglevel <level>` | `Verbose` | Output of the tool itself: `Debug`, `Verbose`, `Information`, `Warning`, `Error`. `Debug` also prints the `dotnet test` command lines. An unknown value falls back to `Verbose`. |
| `--logTestResults` | off | After each run, print every test with ✓ or ✗, and the error message and stack trace of failed tests. Reads the TRX file, so it does not depend on `--verbosity`. |
| `--logPassedTests` | off | Add a `console;verbosity=detailed` logger so `dotnet test` prints each test as it passes. Ignored when `--logTestResults` is set. Can produce a lot of output. |

## Options passed to `dotnet test`

These options mean the same as in [`dotnet test`](https://learn.microsoft.com/dotnet/core/tools/dotnet-test) and are passed on every run.

| Option | Default | Description |
|---|---|---|
| `--filter <expression>` | — | Run only the tests that match. On reruns it is combined with the failed tests: `(your filter)&(failed tests)`. |
| `--logger`, `-l <logger>` | `trx` | Logger for test results. Repeat for several loggers. A `trx` logger is always added if none of yours is TRX, because the tool reads TRX files. |
| `--results-directory`, `-r <dir>` | `.` | Base folder for results. Each run writes to `<dir>/attempt-N`. See [Results folders](how-it-works#results-folders). |
| `--settings`, `-s <file>` | — | A `.runsettings` file. |
| `--configuration`, `-c <name>` | — | Build configuration, for example `Release`. |
| `--framework`, `-f <tfm>` | — | Target framework, for example `net8.0`. |
| `--no-build` | off | Do not build before testing. Implies `--no-restore`. |
| `--no-restore` | off | Do not restore before building. |
| `--verbosity`, `-v <level>` | — | Verbosity of `dotnet test`: `Quiet`, `Minimal`, `Normal`, `Detailed`, `Diagnostic`. |
| `--blame` | off | Run in blame mode, to find the test that crashes the test host. |
| `--collect <name>` | — | Enable a data collector, for example `"XPlat Code Coverage"` or `"Code Coverage"`. Only one collector. |
| `--environment`, `-e <NAME=value>` | — | Set an environment variable for the test run. Repeat for several variables. |

### MSBuild arguments

Arguments that start with `/p:`, `-p:`, `--property:`, `/property:`, `-property:`, `/m:`, `-m:` or `--maxCpuCount:` (and `/maxCpuCount:`, `-maxCpuCount:`) are passed to `dotnet test` as they are:

```bash
test-rerun MyTests.csproj /p:CollectCoverage=true -m:1
```

### Inline run settings

Everything after `--` is passed to `dotnet test` as [inline run settings](https://learn.microsoft.com/dotnet/core/testing/selective-unit-tests#runsettings-arguments). Put it at the end of the command:

```bash
test-rerun MyTests.csproj --rerunMaxAttempts 2 -- RunConfiguration.MaxCpuCount=1 NUnit.NumberOfTestWorkers=1
```

{: .warning }
Any other argument the tool does not recognise is also treated as an inline run setting. Check the spelling of options: a mistyped option does not fail, it is passed to `dotnet test` after `--`.

## Exit codes

| Code | Meaning |
|---|---|
| `0` | All tests passed, on the first run or after reruns. |
| `1` | Tests still failed after the last rerun, reruns were skipped by `--rerunMaxFailedTests` / `--rerunFailedThreshold`, no TRX file was found, or `dotnet test` (or `dotnet-coverage`) failed for another reason, such as a build error. |
| `130` | The run was cancelled (Ctrl+C). |

## Examples

```bash
# Release build, built in a previous step
test-rerun MySolution.sln --configuration Release --no-build

# Only integration tests; wait 10 s before each rerun; give up if more than 15 fail
test-rerun IntegrationTests.csproj --filter "Category=Integration" --delay 10 --rerunMaxFailedTests 15

# Keep results in ./TestResults and print every test result
test-rerun MyTests.csproj --results-directory TestResults --logTestResults

# JUnit report for GitLab, plus the TRX the tool needs (added automatically)
test-rerun MyTests.csproj --logger "junit;LogFileName=junit.xml"

# Merged Cobertura coverage of all runs
test-rerun MyTests.csproj --collect "XPlat Code Coverage" --mergeCoverageFormat Cobertura

# Environment variables and a run settings file
test-rerun MyTests.csproj -e ASPNETCORE_ENVIRONMENT=Testing --settings ci.runsettings
```
