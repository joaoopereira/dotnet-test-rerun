---
layout: default
title: Upgrading from 4.x
nav_order: 8
---

# Upgrading from 4.x
{: .no_toc }

Version 5.0 has one breaking change: the tool needs the .NET 8 or .NET 10 runtime. Report files are still written directly to `--results-directory`, as in 4.x, so CI steps that publish them do not change.

<details open markdown="block">
  <summary>Contents</summary>
  {: .text-delta }
- TOC
{:toc}
</details>

## Checklist

1. Make sure the machine that runs `test-rerun` has the .NET 8 or .NET 10 runtime.
2. If you use the `-net9` or `-dotnet9` Docker images, switch to `-net8` or `-net10`.
3. If you added extra quotes or backslashes to arguments so they survived the tool, remove them.
4. Check that `--loglevel` is one of `Debug`, `Verbose`, `Information`, `Warning` or `Error`.

## Breaking change

### The tool needs .NET 8 or .NET 10

The tool now targets `net8.0` and `net10.0`. .NET 6 and .NET 9 are out of support, so it no longer runs on a machine whose only runtime is .NET 6 or .NET 9. Install the .NET 8 or .NET 10 runtime next to it. Projects under test can still target `net6.0`, `net9.0` or older frameworks, as long as their SDK and runtime are installed.

The `<version>-net9` and `<version>-dotnet9` Docker images are no longer published. Use `<version>-net8` or `<version>-net10`. See [Docker](docker).

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
