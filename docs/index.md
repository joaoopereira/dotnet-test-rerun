---
layout: default
title: Home
nav_order: 1
description: "dotnet-test-rerun wraps dotnet test and reruns only the failed tests until they pass."
permalink: /
---

# dotnet-test-rerun

[![Build Status](https://github.com/joaoopereira/dotnet-test-rerun/actions/workflows/cd.yml/badge.svg)](https://github.com/joaoopereira/dotnet-test-rerun/actions/workflows/cd.yml)
[![NuGet Version](https://img.shields.io/nuget/v/dotnet-test-rerun.svg?label=dotnet-test-rerun)](https://www.nuget.org/packages/dotnet-test-rerun)
[![NuGet Downloads](https://img.shields.io/nuget/dt/dotnet-test-rerun.svg?label=nuget-downloads)](https://www.nuget.org/packages/dotnet-test-rerun)
[![Docker Pulls](https://img.shields.io/docker/pulls/joaoopereira/dotnet-test-rerun?label=docker-pulls)](https://hub.docker.com/r/joaoopereira/dotnet-test-rerun)

`dotnet test` has no built-in way to retry failed tests. **dotnet-test-rerun** wraps it: it runs your tests, reads the TRX results to find the failed ones, and runs only those again, up to `--rerunMaxAttempts` times.

Use it when tests fail for reasons outside your code: a database that is still starting, a rate-limited API, a slow browser in a UI test, a test host that crashed.

## Quick start

```bash
dotnet tool install --global dotnet-test-rerun

test-rerun path/to/MyTests.csproj
```

The path can be anything `dotnet test` accepts: a project, a solution, a directory or a test `.dll`. Every `dotnet test` option you already use (`--filter`, `--configuration`, `--no-build`, `--logger`, `/p:...`) works the same way.

## What you get

- **Only failed tests are rerun.** A filter is built from the TRX results, so a rerun takes seconds, not the whole suite.
- **xUnit, NUnit and MSTest**, including parameterised tests, and projects that target several frameworks.
- **Safety limits.** Skip reruns when too many tests fail (`--rerunMaxFailedTests`, `--rerunFailedThreshold`).
- **Separate results per run.** Each run writes to its own `attempt-N` folder.
- **Merged code coverage** across all runs (`--mergeCoverageFormat`).
- **A clear exit code**: `0` when every test passed in the end, `1` otherwise.

## Where to go next

| Page | Read it to |
|---|---|
| [Installation](installation) | Install the tool globally, locally, or use Docker |
| [Command reference](usage) | Look up every option and its default |
| [How it works](how-it-works) | Understand the rerun loop, the filters and the results folders |
| [Guides](configuration) | Tune reruns, logging, code coverage and run settings |
| [CI integration](ci) | Copy a GitHub Actions, Azure DevOps or GitLab CI setup |
| [Docker](docker) | Run the tool from a container image |
| [Troubleshooting](troubleshooting) | Fix common problems |
| [Contributing](contributing) | Build, test and change the tool |

## License

Copyright © 2023-2026 [João Pereira](https://joaoopereira.com). Licensed under the [GNU General Public License v3.0](https://github.com/joaoopereira/dotnet-test-rerun/blob/main/LICENSE).
