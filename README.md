# dotnet-test-rerun ![build-and-publish](https://github.com/joaoopereira/dotnet-test-rerun/actions/workflows/build-and-publish.yml/badge.svg)[![dotnet-test-rerun][1]][2]
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

[1]: https://img.shields.io/nuget/v/dotnet-test-rerun.svg?label=dotnet-test-rerun
[2]: https://www.nuget.org/packages/dotnet-test-rerun