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
