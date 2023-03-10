# dotnet-test-rerun

# Status
[![1]][2] [![3]][4] [![5]][4]

# Description
Unfortunately, the isn't a way with plain `dotnet test` to automatically rerun failed tests.

This tool is wrapper for the `dotnet test` that automatically reruns any tests with the outcome "Failed" until they pass or a maximum number of attempts has been reached. This is useful, for cases where tests may fail intermittently due to external factors such as network connectivity, database availability, or race conditions.

# Installation
```sh
dotnet tool install --global dotnet-test-rerun
```

# Usage
```sh
test-rerun [somepathtodll] [OPTIONS]
```

## Arguments
| argument | description                       |
| -------- | --------------------------------- |
| `path`   | Path to a test project .dll file. |

## Options
| option               | description                                                                                                          |
| -------------------- | -------------------------------------------------------------------------------------------------------------------- |
| `--filter`           | Run tests that match the given expression.                                                                           |
| `--settings, -s`     | The run settings file to use when running tests.                                                                     |
| `--logger, -l`       | Specifies a logger for test results. *(default: trx)*                                                                |
| `--resultsDirectory` | The directory where the test results are going to be placed. If the specified directory doesn't exist, it's created. |
| `--rerunMaxAttempts` | Maximum # of attempts. *(default: 3)*                                                                                |

# License
This tool is licensed under GNU General Public License v3.0. See the [LICENSE](/LICENSE) file for details.

[1]: https://github.com/joaoopereira/dotnet-test-rerun/actions/workflows/cd.yml/badge.svg
[2]: https://github.com/joaoopereira/dotnet-test-rerun/actions/workflows/cd.yml
[3]: https://img.shields.io/nuget/v/dotnet-test-rerun.svg?label=dotnet-test-rerun
[4]: https://www.nuget.org/packages/dotnet-test-rerun
[5]: https://img.shields.io/nuget/dt/dotnet-test-rerun.svg?label=downloads