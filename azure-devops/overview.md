# dotnet-test-rerun

Runs `dotnet test`, then reruns only the tests that failed, up to the number of attempts you choose. The step fails only when tests still fail after the last rerun. The TRX files of every run are published to the **Tests** tab.

```yaml
steps:
  - task: UseDotNet@2
    inputs:
      packageType: sdk
      version: 10.0.x

  - script: dotnet build MySolution.sln --configuration Release

  - task: TestRerun@1
    inputs:
      path: MySolution.sln
      arguments: --no-build --configuration Release --rerunMaxAttempts 3
```

| Input | Default | Description |
|---|---|---|
| `path` | | Solution, project or folder to test. |
| `arguments` | | Any other [option](https://joaoopereira.github.io/dotnet-test-rerun/usage), quoted as in a shell. |
| `version` | latest | Tool version to install. A pinned version is cached on the agent. |
| `resultsDirectory` | `$(Agent.TempDirectory)/TestResults` | Folder for the TRX files. |
| `workingDirectory` | `$(System.DefaultWorkingDirectory)` | Folder to run from. |
| `publishTestResults` | `true` | Publish the TRX files, even when tests fail. |
| `testRunTitle` | | Title of the published test run. |

A test that failed and then passed shows up once as failed (first run) and once as passed (later run), because every run's TRX file is published.

Source and documentation: [github.com/joaoopereira/dotnet-test-rerun](https://github.com/joaoopereira/dotnet-test-rerun).
