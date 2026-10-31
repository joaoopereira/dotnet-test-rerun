---
layout: default
title: Guides
nav_order: 5
---

# Guides
{: .no_toc }

Practical advice for the options that need more than one line. For the full list of options, see the [command reference](usage).

<details open markdown="block">
  <summary>Contents</summary>
  {: .text-delta }
- TOC
{:toc}
</details>

## Choosing how many reruns

`--rerunMaxAttempts` is the number of reruns after the first run. Start low and raise it only for suites that need it:

| Tests | Suggested setup |
|---|---|
| Unit tests | Usually no reruns. If one is flaky, fix it rather than retry it. |
| Integration tests (database, HTTP APIs, queues) | `--rerunMaxAttempts 3 --delay 5` |
| UI / end-to-end tests | `--rerunMaxAttempts 3` to `5`, with `--blame` while investigating |

Retries hide flakiness. Use `--logTestResults` and keep the `attempt-N` reports, so you can see which tests needed a rerun and fix them.

## Stopping early when too many tests fail

When many tests fail at once, the cause is usually a real regression or a broken environment, and rerunning only makes the pipeline slower. Two options stop the reruns:

```bash
# Do not rerun when more than 10 tests failed
test-rerun MyTests.csproj --rerunMaxFailedTests 10

# Do not rerun when more than 20 % of the tests failed
test-rerun MyTests.csproj --rerunFailedThreshold 20
```

- The limits are checked before every rerun, against the run before it.
- The percentage is the number of failed tests divided by the number of tests in that run's TRX files.

{: .warning }
On the second and later reruns, "the run before it" only contains the rerun tests, so the percentage is usually high (for example, 2 of 3 rerun tests still failing is 67 %). With `--rerunFailedThreshold`, expect the reruns to stop after the first one unless almost all rerun tests pass.

- When a limit stops the reruns, the tool exits with `1`.
- You can use both. The rerun is skipped if either limit is exceeded.

## Waiting between reruns

`--delay <seconds>` waits before each rerun. Use it when a failure is likely to go away with time: a service that is starting, a rate limit, a connection pool that needs to recover.

```bash
test-rerun IntegrationTests.csproj --delay 10
```

The delay is not applied when there is nothing to rerun or a limit stops the reruns.

## Filtering tests

`--filter` uses the normal [`dotnet test` filter syntax](https://learn.microsoft.com/dotnet/core/testing/selective-unit-tests):

```bash
test-rerun MyTests.csproj --filter "Category=Integration"
test-rerun MyTests.csproj --filter "FullyQualifiedName~MyNamespace.Api&Priority=1"
test-rerun MyTests.csproj --filter "Category!=Slow"
```

Your filter is kept on every rerun, so a rerun never runs tests your filter excluded. See [The rerun filter](how-it-works#the-rerun-filter).

## Seeing what happened

| You want to see | Use |
|---|---|
| The result of every test after each run, and the error of failed ones | `--logTestResults` |
| Each test as it passes, live (for example, to find the last test before a hang) | `--logPassedTests` |
| More or less output from `dotnet test` itself | `--verbosity Minimal` … `Diagnostic` |
| The exact `dotnet test` commands the tool runs | `--loglevel Debug` |
| Only warnings and errors from the tool | `--loglevel Warning` |

## Test loggers and reports

The tool needs TRX files to find failed tests, so it adds `--logger trx` when none of your loggers is a TRX logger. You can add any other logger:

```bash
# JUnit for GitLab or Jenkins (needs the JunitXml.TestLogger package in the test project)
test-rerun MyTests.csproj --logger "junit;LogFileName=junit.xml"

# A named TRX file
test-rerun MyTests.csproj --logger "trx;LogFileName=results.trx"
```

Each run writes its reports to its own `attempt-N` folder, so a report from an earlier run is not overwritten. See [Results folders](how-it-works#results-folders).

`--deleteReports` deletes the TRX files of all runs at the end. Use it when you only need the exit code.

## Code coverage

Collect coverage as you would with `dotnet test`:

```bash
test-rerun MyTests.csproj --collect "XPlat Code Coverage"
```

Each run produces its own coverage file, and a rerun only covers the tests that were rerun. To get one file for the whole run, add `--mergeCoverageFormat`. The tool then calls [`dotnet-coverage merge`](https://learn.microsoft.com/dotnet/core/additional-tools/dotnet-coverage) on the `*.coverage` and `*.cobertura*` files of all `attempt-N` folders:

```bash
dotnet tool install --global dotnet-coverage

test-rerun MyTests.csproj \
  --collect "XPlat Code Coverage" \
  --mergeCoverageFormat Cobertura \
  --results-directory TestResults
```

| Format | Output file |
|---|---|
| `Cobertura` | `TestResults/merged.cobertura.xml` |
| `Xml` | `TestResults/merged.xml` |
| `Coverage` | `TestResults/merged.coverage` |

If `dotnet-coverage` is missing or fails, the tool exits with `1`.

## Run settings

Use a `.runsettings` file for settings you would otherwise repeat:

```bash
test-rerun MyTests.csproj --settings ci.runsettings
```

```xml
<RunSettings>
  <RunConfiguration>
    <MaxCpuCount>1</MaxCpuCount>
    <TestSessionTimeout>3600000</TestSessionTimeout>
  </RunConfiguration>
</RunSettings>
```

Or pass single settings inline, at the end of the command:

```bash
test-rerun MyTests.csproj -- RunConfiguration.MaxCpuCount=1
```

## Environment variables

```bash
test-rerun MyTests.csproj -e ASPNETCORE_ENVIRONMENT=Testing -e API_TIMEOUT=30
```

The variables are set on every run, including reruns.
