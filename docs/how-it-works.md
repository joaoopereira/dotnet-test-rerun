---
layout: default
title: How it works
nav_order: 4
---

# How it works
{: .no_toc }

<details open markdown="block">
  <summary>Contents</summary>
  {: .text-delta }
- TOC
{:toc}
</details>

## The rerun loop

1. **First run.** `dotnet test` runs with your arguments and writes its results to `attempt-0`.
2. **Check the result.** If `dotnet test` exits with `0`, the tool stops with exit code `0`.
3. **Find the failed tests.** The tool reads the TRX files of the last run and builds a `--filter` with one clause per failed test.
4. **Check the limits.** If more tests failed than `--rerunMaxFailedTests`, or (after the first run only) a higher percentage than `--rerunFailedThreshold`, the tool stops without rerunning and exits with `1`.
5. **Rerun.** After `--delay` seconds, `dotnet test` runs again with that filter and writes to `attempt-1`.
6. **Repeat** steps 3 to 5 until every test passes or `--rerunMaxAttempts` reruns are done.

Each rerun only contains the tests that failed in the run before it, so it gets smaller each time.

Finally, the tool merges the coverage files (with `--mergeCoverageFormat`) and deletes the TRX files (with `--deleteReports`).

## How a failed run is detected

The tool decides from the exit code of `dotnet test` and from the TRX files, not from the console text, so it works with any `dotnet test` language:

- Exit code `0`: the run passed.
- Non-zero exit code and a new TRX file: tests failed. The TRX file says which ones.
- Non-zero exit code and no TRX file: the tool looks for `Failed:`, `Test host process crashed` or `Test Run Aborted` in the output. This fallback only works with English output. If none is found, it is treated as an error (for example, a build error) and the tool exits with `1` without rerunning.

## The rerun filter

The failed tests are turned into a [`dotnet test --filter`](https://learn.microsoft.com/dotnet/core/testing/selective-unit-tests) expression. The form depends on the test framework:

| Framework | Clause for a failed test |
|---|---|
| xUnit | `FullyQualifiedName=Namespace.Class.Method`, plus `&DisplayName~(args)` for a failed Theory case |
| NUnit | `FullyQualifiedName=Namespace.Class.Method`, or `FullyQualifiedName~Namespace.Class.Method(args)` for a parameterised test |
| MSTest | `FullyQualifiedName=Namespace.Class.Method` |

Characters that have a meaning in filters (`( ) ? { }`, spaces) are escaped, and quotes are written as `%22`, so tests with string arguments can be rerun.

If you passed `--filter`, it is kept: the rerun uses `(your filter)&(failed tests)`.

### Several target frameworks or projects

The failed tests are grouped by the target framework stored in the TRX file. Each group is rerun with its own `dotnet test` call. A rerun attempt fails if any of these calls still has failing tests.

### Long filters

When many tests fail, the filter is split into batches of at most 8,000 characters, and each batch is a separate `dotnet test` call within the same attempt. This keeps the command line under the Windows limit. If you would rather not rerun at all when many tests fail, set `--rerunMaxFailedTests` or `--rerunFailedThreshold`.

### Test host crashes

If the test run was aborted (for example, the test host crashed) and the TRX file lists no failed tests, the tool cannot know which test caused it. It reruns every test class of that run instead.

## Results folders

Each run writes to its own folder under `--results-directory` (default: the current directory):

```text
TestResults/
├── attempt-0/              # first run
├── attempt-1/              # first rerun (all its dotnet test calls)
├── attempt-2/
└── merged.cobertura.xml    # with --mergeCoverageFormat Cobertura
```

- The final result of a test is in the `attempt-N` folder with the highest `N` that contains it.
- `attempt-N` folders from a previous run in the same results directory are deleted when the tool starts.
- In CI, publish the reports with a recursive pattern, such as `TestResults/**/*.trx`. See [CI integration](ci).

{: .note }
> **Upgrading from 4.x:** reports used to be written directly to the results directory. Change CI steps that read TRX or JUnit files to a recursive pattern such as `TestResults/**/*.trx`.

## Exit codes

| Code | Meaning |
|---|---|
| `0` | All tests passed, on the first run or after reruns. |
| `1` | Tests still failed, reruns were skipped because of a limit, no TRX file was found, or `dotnet test` failed for another reason. |
| `130` | The run was cancelled. |
