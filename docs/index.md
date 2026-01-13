---
layout: default
title: Home
nav_order: 1
description: "dotnet-test-rerun is a powerful wrapper for dotnet test that automatically reruns failed tests"
permalink: /
---

# 🔄 dotnet-test-rerun

[![Build Status](https://github.com/joaoopereira/dotnet-test-rerun/actions/workflows/cd.yml/badge.svg)](https://github.com/joaoopereira/dotnet-test-rerun/actions/workflows/cd.yml)
[![Coverage Status](https://coveralls.io/repos/github/joaoopereira/dotnet-test-rerun/badge.svg?branch=main)](https://coveralls.io/github/joaoopereira/dotnet-test-rerun?branch=main)
[![NuGet Version](https://img.shields.io/nuget/v/dotnet-test-rerun.svg?label=dotnet-test-rerun)](https://www.nuget.org/packages/dotnet-test-rerun)
[![NuGet Downloads](https://img.shields.io/nuget/dt/dotnet-test-rerun.svg?label=nuget-downloads)](https://www.nuget.org/packages/dotnet-test-rerun)
[![Docker Pulls](https://img.shields.io/docker/pulls/joaoopereira/dotnet-test-rerun?label=docker-pulls)](https://hub.docker.com/r/joaoopereira/dotnet-test-rerun)

## Overview

**dotnet-test-rerun** is a powerful wrapper for `dotnet test` that automatically reruns failed tests until they pass or reach a maximum number of attempts. This tool is essential for handling intermittent test failures caused by external factors such as:

- Network connectivity issues
- Database availability
- Race conditions
- Timing-dependent tests
- Resource contention

## Key Features

- 🔄 **Automatic Retry**: Automatically reruns failed tests with configurable retry attempts
- 🎯 **Selective Rerun**: Only reruns failed tests, not the entire test suite
- 📊 **Test Reporting**: Supports standard dotnet test loggers (trx, junit, etc.)
- 🐳 **Docker Support**: Available as Docker images for .NET 8.0, 9.0, and 10.0
- 📦 **Global Tool**: Easy installation as a dotnet global tool
- 🔧 **Highly Configurable**: Extensive options for filtering, logging, and test execution
- 📈 **Code Coverage**: Support for code coverage collection and merging
- ⚡ **Performance**: Efficient execution by targeting only failed tests

## Quick Start

### Installation

Install as a global dotnet tool:

```bash
dotnet tool install --global dotnet-test-rerun
```

### Basic Usage

Run tests with automatic retry:

```bash
test-rerun path/to/test.dll
```

Run with custom retry attempts:

```bash
test-rerun path/to/test.dll --rerunMaxAttempts 5
```

## Why Use dotnet-test-rerun?

Unfortunately, there isn't a built-in way with plain `dotnet test` to automatically rerun failed tests. This tool fills that gap by:

1. **Reducing False Failures**: Automatically handles transient failures without manual intervention
2. **Saving Time**: No need to manually rerun failed test suites
3. **CI/CD Optimization**: Makes CI/CD pipelines more resilient to intermittent issues
4. **Better Test Metrics**: Distinguish between truly failed tests and transient failures

## Language Support

⚠️ **Important**: This tool is language-dependent. The output of `dotnet test` may be localized, and if it is not in English, the tool may not function correctly. Currently, only English is supported.

## Navigation

- [Installation Guide](installation) - Detailed installation instructions for various scenarios
- [Usage Guide](usage) - Comprehensive usage documentation and command-line options
- [Configuration](configuration) - Configure test execution, retry behavior, and more
- [Examples](examples) - Real-world examples and use cases
- [Docker Guide](docker) - Using dotnet-test-rerun with Docker
- [Contributing](contributing) - How to contribute to the project

## Support

- 🐛 [Report Issues](https://github.com/joaoopereira/dotnet-test-rerun/issues)
- 📖 [GitHub Repository](https://github.com/joaoopereira/dotnet-test-rerun)

## License

This tool is licensed under GNU General Public License v3.0. See the [LICENSE](https://github.com/joaoopereira/dotnet-test-rerun/blob/main/LICENSE) file for details.

## Author

**João Pereira**
- Website: [joaoopereira.com](https://joaoopereira.com)
- GitHub: [@joaoopereira](https://github.com/joaoopereira)

Give a ⭐ if this project helped you!
