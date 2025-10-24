# dotnet-test-rerun

# Status
[![1]][2] [![6]][7] [![3]][4] [![5]][4] [![8]][9]

# Description
Unfortunately, there isn't a way with plain `dotnet test` to automatically rerun failed tests.\
This tool is wrapper for the `dotnet test` that automatically reruns any tests with the outcome "Failed" until they pass or a maximum number of attempts has been reached. This is useful, for cases where tests may fail intermittently due to external factors such as network connectivity, database availability, or race conditions.\
Please note that this tool is language-dependent. The output of `dotnet test` may be localized, and if it is not in English, the tool may not function correctly. Currently, only English is supported.


# :computer: Usage
## :package: dotnet tool
```sh
dotnet tool install --global dotnet-test-rerun
test-rerun [somepathtodll] [OPTIONS]
```
## :whale: Docker

The Docker images support both .NET 8.0 and .NET 9.0 runtimes. By default, the latest version uses .NET 9.0.

### Available Tags

| Tag | .NET Runtime | Description |
|-----|-------------|-------------|
| `latest` or `{version}` | .NET 9.0 | Default image with .NET 9.0 runtime |
| `{version}-net9` or `{version}-dotnet9` | .NET 9.0 | Explicit .NET 9.0 image |
| `{version}-net8` or `{version}-dotnet8` | .NET 8.0 | .NET 8.0 image |

### Usage

```sh
# Run with default .NET 9.0 image
docker run joaoopereira/dotnet-test-rerun [somepathtodll] [OPTIONS]

# Run with specific .NET version
docker run joaoopereira/dotnet-test-rerun:1.0.0-net8 [somepathtodll] [OPTIONS]

# Run with latest .NET 9.0
docker run joaoopereira/dotnet-test-rerun:latest [somepathtodll] [OPTIONS]
```


## :arrow_forward: Arguments
| argument | description                       |
| -------- | --------------------------------- |
| `path`   | Path to a test project .dll file. |

## :arrow_forward: Options
| option                    | description                                                                                                                                     |
|---------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------|
| `--filter`                | Run tests that match the given expression.                                                                                                      |
| `--settings, -s`          | The run settings file to use when running tests.                                                                                                |
| `--logger, -l`            | Specifies a logger for test results. Multiple values are allowed. *(default: trx)*                                                              |
| `--results-directory, -r` | The directory where the test results are going to be placed. If the specified directory doesn't exist, it's created.                            |
| `--rerunMaxAttempts`      | Maximum # of attempts. *(default: 3)*                                                                                                           |
| `--rerunMaxFailedTests`   | Maximum # of failed tests to rerun. If exceeded, tests will not be rerun. *(default: -1, no limit)*                                             |
| `--loglevel`              | Log Level. *(default: Verbose)*                                                                                                                 |
| `--no-build`              | Do not build the project before testing. Implies --no-restore.                                                                                  |
| `--no-restore`            | Do not restore the project before building.*                                                                                                    |
| `--delay, -d`             | Delay between test runs in seconds.                                                                                                             |
| `--blame`                 | Run the tests in blame mode.                                                                                                                    |
| `--configuration, -c`     | Defines the build configuration. The default for most projects is Debug, but you can override the build configuration settings in your project. |
| `--framework, -f`         | Defines the target framework.                                                                                                                   |
| `--verbosity, -v`         | Sets the verbosity level of the command. Allowed values are quiet, minimal, normal, detailed, and diagnostic.                                   |
| `--deleteReports`         | Delete the generated report files.                                                                                                              |
| `--collect`               | Enables data collector for the test run. Example: --collect "Code Coverage" or --collect "XPlat Code Coverage"                                  |
| `--mergeCoverageFormat`   | Output coverage format. Possible values: Coverage, Cobertura or Xml. It requires dotnet coverage tool to be installed.                          |
| `--environment, -e`       | Sets the value of an environment variable. Can be set multiple times.                                                                           |
| `--inlineRunSettings, --` | Allow the configuration of inline run settings.                                                                                                 |


Notes: 
- Sending `/p:` instructions to set property values is also allowed.

## 👤 Author & Contributors

👤 **João Pereira**

- Website: [joaoopereira.com](https://joaoopereiraa.com)
- Github: [@joaoopereira](https://github.com/joaoopereira)

👥 **Contributors**

[![Contributors](https://contrib.rocks/image?repo=joaoopereira/dotnet-test-rerun)](https://github.com/joaoopereira/dotnet-test-rerun/graphs/contributors)

## :handshake: Contributing

Contributions, issues and feature requests are welcome!\
Feel free to check the [issues page](https://github.com/joaoopereira/dotnet-test-rerun/issues).

## Show your support

Give a :star: if this project helped you!

## :memo: License

Copyright © 2023 [João Pereira](https://github.com/joaoopereira).\
This tool is licensed under GNU General Public License v3.0. See the [LICENSE](/LICENSE) file for details.

[1]: https://github.com/joaoopereira/dotnet-test-rerun/actions/workflows/cd.yml/badge.svg
[2]: https://github.com/joaoopereira/dotnet-test-rerun/actions/workflows/cd.yml
[3]: https://img.shields.io/nuget/v/dotnet-test-rerun.svg?label=dotnet-test-rerun
[4]: https://www.nuget.org/packages/dotnet-test-rerun
[5]: https://img.shields.io/nuget/dt/dotnet-test-rerun.svg?label=nuget-downloads
[6]: https://coveralls.io/repos/github/joaoopereira/dotnet-test-rerun/badge.svg?branch=main
[7]: https://coveralls.io/github/joaoopereira/dotnet-test-rerun?branch=main
[8]: https://img.shields.io/docker/pulls/joaoopereira/dotnet-test-rerun?label=docker-pulls
[9]: https://hub.docker.com/r/joaoopereira/dotnet-test-rerun
