<div align="center">

# dotnet-test-rerun

**Rerun only the failed .NET tests, automatically, until they pass.**

[![Build Status][1]][2]
[![Coverage Status][6]][7]
[![NuGet Version][3]][4]
[![NuGet Downloads][5]][4]
[![Docker Pulls][8]][9]

[Documentation](https://joaoopereira.github.io/dotnet-test-rerun/) •
[Command reference](https://joaoopereira.github.io/dotnet-test-rerun/usage) •
[Issues](https://github.com/joaoopereira/dotnet-test-rerun/issues)

</div>

`dotnet test` has no built-in way to retry failed tests. `dotnet-test-rerun` wraps it: it runs your tests, reads the TRX results to find the failed ones, and runs **only those** again, up to `--rerunMaxAttempts` times. It works with xUnit, NUnit and MSTest, and accepts the same options as `dotnet test`.

Use it to keep CI green when tests fail for reasons outside your code: a database that is still starting, a rate-limited API, a slow browser in a UI test.

## Quick start

```bash
dotnet tool install --global dotnet-test-rerun

# Run the tests, then rerun the failed ones up to 3 times (the default)
test-rerun path/to/MyTests.csproj

# Rerun up to 5 times, waiting 10 seconds before each rerun
test-rerun path/to/MyTests.csproj --rerunMaxAttempts 5 --delay 10
```

The path can be anything `dotnet test` accepts: a project, a solution, a directory or a test `.dll`.

## How it works

```text
attempt-0   dotnet test ...                                   → 120 passed, 3 failed
attempt-1   dotnet test ... --filter "<the 3 failed tests>"   → 2 passed, 1 failed
attempt-2   dotnet test ... --filter "<the 1 failed test>"    → 1 passed
            exit code 0
```

- Each run writes its results to its own `attempt-N` folder under `--results-directory`.
- If you pass `--filter`, it is kept on every rerun: `(your filter)&(failed tests)`.
- Use `--rerunMaxFailedTests` or `--rerunFailedThreshold` to skip reruns when too many tests fail, because that usually means a real problem.
- The exit code is `0` if every test passed in the end, and `1` otherwise.

More detail: [How it works](https://joaoopereira.github.io/dotnet-test-rerun/how-it-works).

## Main options

| Option | Default | Description |
|---|---|---|
| `--rerunMaxAttempts` | `3` | Maximum number of reruns after the first run |
| `--rerunMaxFailedTests` | `-1` (no limit) | Do not rerun if more tests than this failed |
| `--rerunFailedThreshold` | `-1` (no limit) | Do not rerun if more than this percentage of tests failed |
| `--delay`, `-d` | `0` | Seconds to wait before each rerun |
| `--results-directory`, `-r` | `.` | Where results go; each run writes to an `attempt-N` subfolder |
| `--mergeCoverageFormat` | — | Merge the coverage of all runs (`Coverage`, `Cobertura`, `Xml`); needs `dotnet-coverage` |
| `--logTestResults` | `false` | Print the result of each test after every run |
| `--deleteReports` | `false` | Delete the TRX files when finished |

`--filter`, `--logger`, `--settings`, `--configuration`, `--framework`, `--no-build`, `--blame`, `--collect`, `-e`, `/p:` and inline run settings are passed to `dotnet test`. See the [command reference](https://joaoopereira.github.io/dotnet-test-rerun/usage) for all options.

## Use in CI

```yaml
# GitHub Actions
- run: dotnet tool install --global dotnet-test-rerun
- run: dotnet build --configuration Release
- run: test-rerun MySolution.sln --no-build --configuration Release --results-directory TestResults
- uses: actions/upload-artifact@v4
  if: always()
  with:
    name: test-results
    path: TestResults/**/*.trx
```

Azure DevOps and GitLab CI examples: [CI integration](https://joaoopereira.github.io/dotnet-test-rerun/ci).

## Docker

```bash
docker run --rm -v "$PWD:/src" joaoopereira/dotnet-test-rerun:latest /src/MyTests.csproj --results-directory /src/TestResults
```

Tags: `latest` and `<version>` (.NET 10), `<version>-net10`, `<version>-net9`, `<version>-net8`. See the [Docker guide](https://joaoopereira.github.io/dotnet-test-rerun/docker).

## Contributing

Bug reports, ideas and pull requests are welcome. Read the [contributing guide](https://joaoopereira.github.io/dotnet-test-rerun/contributing) and browse the [open issues](https://github.com/joaoopereira/dotnet-test-rerun/issues).

[![Contributors](https://contrib.rocks/image?repo=joaoopereira/dotnet-test-rerun)](https://github.com/joaoopereira/dotnet-test-rerun/graphs/contributors)

## License

Copyright © 2023-2026 [João Pereira](https://joaoopereira.com). Licensed under the [GNU General Public License v3.0](LICENSE).

[1]: https://github.com/joaoopereira/dotnet-test-rerun/actions/workflows/cd.yml/badge.svg
[2]: https://github.com/joaoopereira/dotnet-test-rerun/actions/workflows/cd.yml
[3]: https://img.shields.io/nuget/v/dotnet-test-rerun.svg?label=dotnet-test-rerun
[4]: https://www.nuget.org/packages/dotnet-test-rerun
[5]: https://img.shields.io/nuget/dt/dotnet-test-rerun.svg?label=nuget-downloads
[6]: https://coveralls.io/repos/github/joaoopereira/dotnet-test-rerun/badge.svg?branch=main
[7]: https://coveralls.io/github/joaoopereira/dotnet-test-rerun?branch=main
[8]: https://img.shields.io/docker/pulls/joaoopereira/dotnet-test-rerun?label=docker-pulls
[9]: https://hub.docker.com/r/joaoopereira/dotnet-test-rerun
