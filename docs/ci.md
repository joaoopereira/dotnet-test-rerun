---
layout: default
title: CI integration
nav_order: 6
redirect_from:
  - /examples
  - /examples.html
---

# CI integration
{: .no_toc }

In every CI system the steps are the same: install the tool, build, run `test-rerun`, and publish the reports from the results directory. The step fails when tests still fail after the reruns, because the tool exits with `1`.

<details open markdown="block">
  <summary>Contents</summary>
  {: .text-delta }
- TOC
{:toc}
</details>

## Pin the version

For reproducible builds, install the tool as a [local tool](installation#local-tool) with a manifest committed to the repository, then run `dotnet tool restore` in CI. The examples below use a global install for brevity.

## GitHub Actions

### With the action

The `joaoopereira/dotnet-test-rerun` action installs the tool and runs it. Set up .NET first.

```yaml
- uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '10.0.x'

- run: dotnet build MySolution.sln --configuration Release

- uses: joaoopereira/dotnet-test-rerun@v5
  with:
    path: MySolution.sln
    arguments: --no-build --configuration Release --rerunMaxAttempts 3
    upload-results: true
```

| Input | Default | Description |
|---|---|---|
| `path` | — | Solution, project or folder to test. |
| `arguments` | — | Any other [option](usage), quoted as in a shell. |
| `version` | the action's tag | Tool version. `@v5.0.0` installs 5.0.0; `@v5` or a branch installs the latest release. |
| `results-directory` | `$RUNNER_TEMP/TestResults` | Folder for the TRX files. Don't pass `--results-directory` in `arguments`. |
| `working-directory` | `.` | Folder to run from. |
| `upload-results` | `false` | Upload the results directory as an artifact, even when tests fail. |
| `artifact-name` | `test-results` | Name of that artifact. |

The `results-directory` output holds the folder, for steps that publish the reports. The step fails when tests still fail after the reruns.

### With a script

```yaml
name: Tests

on:
  push:
    branches: [main]
  pull_request:

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Install dotnet-test-rerun
        run: dotnet tool install --global dotnet-test-rerun

      - name: Build
        run: dotnet build MySolution.sln --configuration Release

      - name: Test
        run: >
          test-rerun MySolution.sln
          --no-build
          --configuration Release
          --rerunMaxAttempts 3
          --rerunMaxFailedTests 20
          --results-directory TestResults

      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: test-results
          path: TestResults/**/*.trx
```

## Azure DevOps

### With the task

Install the [dotnet-test-rerun extension](https://marketplace.visualstudio.com/items?itemName=joaoopereira.dotnet-test-rerun) in your organization. The `TestRerun@1` task runs on any agent OS, and its inputs follow the **Visual Studio Test** task (VSTest@3): replace `VSTest@3` with `TestRerun@1` and most inputs carry over. It installs the tool, runs each test file, reruns the failed tests, and publishes the TRX files of every run to the **Tests** tab as one test run, even when tests fail.

```yaml
steps:
  - task: UseDotNet@2
    inputs:
      packageType: sdk
      version: 10.0.x

  - script: dotnet build MySolution.sln --configuration Release
    displayName: Build

  - task: TestRerun@1
    inputs:
      testAssemblyVer2: |
        **/bin/Release/**/*Tests.dll
        !**/obj/**
      rerunMaxAttempts: 3
      testRunTitle: Unit tests
```

| Input | Default | Maps to |
|---|---|---|
| `testAssemblyVer2` | `**/bin/**/*test.dll`, `**/bin/**/*tests.dll`, `!**/obj/**` | Test assemblies, projects or solutions, one pattern per line. Each match runs separately. |
| `searchFolder` | `$(System.DefaultWorkingDirectory)` | Where to search, and where tests run. |
| `resultsFolder` | `$(Agent.TempDirectory)/TestResults` | TRX files, one subfolder per test file. Also set as the `resultsDirectory` output variable. |
| `testFiltercriteria` | — | `--filter` |
| `runSettingsFile` | — | `--settings` |
| `overrideTestrunParameters` | — | `-name value` pairs, passed as `TestRunParameters` |
| `codeCoverageEnabled` | `false` | `--collect "Code Coverage"` |
| `diagnosticsEnabled` | `false` | `--blame` |
| `otherConsoleOptions` | — | Any other [option](usage), quoted as in a shell |
| `rerunFailedTests` | `true` | Off sets `--rerunMaxAttempts 0` |
| `rerunType` | `basedOnTestFailurePercentage` | Which limit below applies |
| `rerunFailedThreshold` | `30` | `--rerunFailedThreshold` |
| `rerunFailedTestCasesMaxLimit` | `5` | `--rerunMaxFailedTests` |
| `rerunMaxAttempts` | `3` | `--rerunMaxAttempts` |
| `testRunTitle`, `platform`, `configuration` | — | Shown on the published test run |
| `customLoggerConfig` | — | An extra `--logger` |
| `publishRunAttachments` | `true` | Upload the run's attachments |
| `donotPublishTestResults` | `false` | Skip publishing to the **Tests** tab |
| `failOnMinTestsNotRun`, `minimumExpectedTests` | `false`, `1` | Fail when fewer distinct tests ran |
| `version` | latest | Tool version. A pinned version is cached on the agent. |

Not available, because they need `vstest.console`: Test Plan and test run selection, test impact analysis, distributing tests across agents, `runInParallel`, `runTestsInIsolation`, custom adapter paths and the Visual Studio version settings.

### With a script

```yaml
trigger:
  - main

pool:
  vmImage: ubuntu-latest

steps:
  - task: UseDotNet@2
    inputs:
      packageType: sdk
      version: 10.0.x

  - script: dotnet tool install --global dotnet-test-rerun
    displayName: Install dotnet-test-rerun

  - script: dotnet build MySolution.sln --configuration Release
    displayName: Build

  - script: >
      test-rerun MySolution.sln
      --no-build
      --configuration Release
      --rerunMaxAttempts 3
      --results-directory $(Agent.TempDirectory)/TestResults
    displayName: Test

  - task: PublishTestResults@2
    condition: always()
    inputs:
      testResultsFormat: VSTest
      testResultsFiles: '**/*.trx'
      searchFolder: $(Agent.TempDirectory)/TestResults
```

{: .note }
`PublishTestResults@2` publishes every TRX file it finds, so a test that failed and then passed appears once as failed (in the first run's file) and once as passed (in a later file). If you only want the final outcome, use `--logTestResults` to read it in the log.

## GitLab CI

GitLab reads JUnit reports. Add the [`JunitXml.TestLogger`](https://www.nuget.org/packages/JunitXml.TestLogger) package to your test projects, then:

```yaml
test:
  image: mcr.microsoft.com/dotnet/sdk:10.0
  script:
    - dotnet tool install --global dotnet-test-rerun
    - export PATH="$PATH:$HOME/.dotnet/tools"
    - dotnet build MySolution.sln --configuration Release
    - >
      test-rerun MySolution.sln
      --no-build
      --configuration Release
      --rerunMaxAttempts 3
      --logger "junit;LogFileName={assembly}.junit.xml"
      --results-directory TestResults
  artifacts:
    when: always
    reports:
      junit: TestResults/**/*.junit.xml
```

The tool still adds a TRX logger next to the JUnit one, because it reads TRX files to find the failed tests.

## Code coverage in CI

```yaml
# GitHub Actions
- name: Test with coverage
  run: |
    dotnet tool install --global dotnet-coverage
    test-rerun MySolution.sln --no-build --configuration Release \
      --collect "XPlat Code Coverage" \
      --mergeCoverageFormat Cobertura \
      --results-directory TestResults

- uses: codecov/codecov-action@v4
  with:
    files: TestResults/merged.cobertura.xml
```

See [Code coverage](configuration#code-coverage) for the formats and file names.
