# dotnet-test-rerun
wrapper of `dotnet test` command that automatic rerun failed tests

# installation
`dotnet tool install --global dotnet-test-rerun`

# usage
`test-rerun [somepathtodll] [OPTIONS]`

## supported arguments
| argument | description                       |
| -------- | --------------------------------- |
| `path`   | Path to a test project .dll file. |

## supported options
| option               | description                                                                                                          |
| -------------------- | -------------------------------------------------------------------------------------------------------------------- |
| `--filter`           | Run tests that match the given expression.                                                                           |
| `--settings, -s`     | The run settings file to use when running tests.                                                                     |
| `--logger, -l`       | Specifies a logger for test results. *(default: trx)*                         |
| `--resultsDirectory` | The directory where the test results are going to be placed. If the specified directory doesn't exist, it's created. |
| `--rerunMaxAttempts` | Maximum # of attempts. *(default: 3)*                                                                                |
