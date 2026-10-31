---
layout: default
title: Troubleshooting
nav_order: 8
---

# Troubleshooting
{: .no_toc }

<details open markdown="block">
  <summary>Contents</summary>
  {: .text-delta }
- TOC
{:toc}
</details>

Run with `--loglevel Debug` first. It prints each `dotnet test` command the tool runs, which you can copy and run yourself.

## The tests fail but nothing is rerun

- **`No trx file found in …`**: `dotnet test` failed without writing a TRX file. Check that the run reached the tests (a build error stops before them) and that no run settings file disables the TRX logger.
- **`Failed tests (…) exceeded the maximum threshold`**: `--rerunMaxFailedTests` or `--rerunFailedThreshold` stopped the reruns on purpose.
- **`command: dotnet test … exited with code …`**: `dotnet test` failed for a reason other than failed tests, such as a build or restore error. Its error message follows.

## A rerun runs no tests, or the wrong ones

The rerun filter is printed as `Rerun filter: …`. Run `dotnet test` with that filter yourself to check which tests it selects.

Known causes:

- Test names with characters that `dotnet test --filter` cannot express. Please [open an issue](https://github.com/joaoopereira/dotnet-test-rerun/issues) with the test name and the printed filter.
- A custom `--filter` that excludes the failed tests. Your filter is kept on reruns: `(your filter)&(failed tests)`.

## An option has no effect

Arguments the tool does not recognise are passed to `dotnet test` as inline run settings, after `--`, without an error. Check the spelling: the tool's own options are case-sensitive camelCase (`--rerunMaxAttempts`, not `--rerun-max-attempts`).

## Reports are not found in CI

Since 5.0, each run writes to its own `attempt-N` folder under `--results-directory`. Use a recursive pattern such as `TestResults/**/*.trx`. See [Results folders](how-it-works#results-folders).

## Output of `dotnet test` in another language

The tool detects failed tests from the exit code and the TRX files, so it works with any language. Only one fallback, used when `dotnet test` fails without writing a TRX file, looks for English text in the output. If you see `exited with code 1` instead of a rerun in that case, set `DOTNET_CLI_UI_LANGUAGE=en` for the run.

## `--mergeCoverageFormat` fails

The merge calls `dotnet-coverage`, which must be installed and on the `PATH`:

```bash
dotnet tool install --global dotnet-coverage
```

## Still stuck?

[Open an issue](https://github.com/joaoopereira/dotnet-test-rerun/issues/new) with:

- the tool version (`dotnet tool list --global dotnet-test-rerun`) and `dotnet --version`;
- the operating system;
- the test framework (xUnit, NUnit, MSTest) and its version;
- the command you ran and the output with `--loglevel Debug`.
