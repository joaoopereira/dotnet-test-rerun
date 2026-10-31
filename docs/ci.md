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

In every CI system the steps are the same: install the tool, build, run `test-rerun`, and publish the reports from all `attempt-N` folders. The step fails when tests still fail after the reruns, because the tool exits with `1`.

<details open markdown="block">
  <summary>Contents</summary>
  {: .text-delta }
- TOC
{:toc}
</details>

## Pin the version

For reproducible builds, install the tool as a [local tool](installation#local-tool) with a manifest committed to the repository, then run `dotnet tool restore` in CI. The examples below use a global install for brevity.

## GitHub Actions

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
`PublishTestResults@2` publishes every TRX file it finds, so a test that failed and then passed appears once as failed (in `attempt-0`) and once as passed (in a later `attempt-N`). If you only want the final outcome, publish the `attempt-N` folder with the highest `N`, or use `--logTestResults` to read the outcome in the log.

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
