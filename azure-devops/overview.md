# dotnet-test-rerun

Runs your tests, then reruns only the ones that failed, up to the number of attempts you choose. It works on Linux, macOS and Windows agents, and its inputs follow the **Visual Studio Test** task (VSTest@3), so moving a pipeline to it is mostly renaming the task. The step fails only when tests still fail after the last rerun. The TRX files of every run are published to the **Tests** tab as one test run.

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
| `otherConsoleOptions` | — | Any other [option](https://joaoopereira.github.io/dotnet-test-rerun/usage), quoted as in a shell |
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

Source and documentation: [github.com/joaoopereira/dotnet-test-rerun](https://github.com/joaoopereira/dotnet-test-rerun).
