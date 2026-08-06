<div align="center">

# 🔄 dotnet-test-rerun

### Automatically rerun failed .NET tests until they pass

[![Build Status][1]][2]
[![Coverage Status][6]][7]
[![NuGet Version][3]][4]
[![NuGet Downloads][5]][4]
[![Docker Pulls][8]][9]

[📖 Documentation](https://joaoopereira.github.io/dotnet-test-rerun/) • [🐛 Issues](https://github.com/joaoopereira/dotnet-test-rerun/issues)

</div>

---

## 🎯 Why dotnet-test-rerun?

Ever had tests fail intermittently due to network issues, timing problems, or external service hiccups? **dotnet-test-rerun** solves this by automatically rerunning failed tests, making your test suites more resilient and your CI/CD pipelines more reliable.

### ✨ Key Features

| Feature | Description |
|---------|-------------|
| 🔄 **Smart Retry** | Automatically reruns only failed tests with configurable attempts |
| 🎯 **Selective Execution** | Targets specific tests with filters instead of rerunning everything |
| 📊 **Multiple Loggers** | Supports trx, junit, console, and custom test loggers |
| 🐳 **Docker Ready** | Pre-built images for .NET 8.0, 9.0, and 10.0 |
| ⚙️ **Highly Configurable** | Extensive options for filtering, delays, and test execution |
| 📈 **Code Coverage** | Collect and merge coverage reports across retry attempts |
| ⚡ **Performance First** | Efficient execution by targeting only failed tests |
| 🔧 **CI/CD Optimized** | Built-in support for GitHub Actions, Azure DevOps, GitLab CI |

---

## 🚀 Quick Start

### Installation

```bash
# Install as a global .NET tool
dotnet tool install --global dotnet-test-rerun
```

### Basic Usage

```bash
# Run tests with automatic retry (default: 3 attempts)
test-rerun path/to/test.dll

# Customize retry attempts
test-rerun path/to/test.dll --rerunMaxAttempts 5

# Add delay between retries (useful for external dependencies)
test-rerun path/to/test.dll --rerunMaxAttempts 5 --delay 10
```

### Docker Usage

```bash
# Use the latest stable version (.NET 10.0)
docker run joaoopereira/dotnet-test-rerun:latest path/to/test.dll --rerunMaxAttempts 3

# Or specify a .NET version
docker run joaoopereira/dotnet-test-rerun:4.0.0-net8 path/to/test.dll
```

---

## 📚 Common Use Cases

<details>
<summary><b>🌐 Integration Tests with External Services</b></summary>

```bash
test-rerun IntegrationTests.dll \
  --filter "Category=Integration" \
  --rerunMaxAttempts 5 \
  --delay 10 \
  --rerunMaxFailedTests 15
```

Perfect for tests that interact with databases, APIs, or microservices that may have transient failures.

</details>

<details>
<summary><b>🚀 CI/CD Pipeline Integration</b></summary>

```yaml
# GitHub Actions example
- name: Run Tests with Retry
  run: |
    test-rerun tests/**/*.dll \
      --configuration Release \
      --rerunMaxAttempts 3 \
      --logger "trx;LogFileName=results.trx" \
      --results-directory ./TestResults
```

Make your CI/CD pipelines more resilient to intermittent failures.

</details>

<details>
<summary><b>📊 Code Coverage Collection</b></summary>

```bash
test-rerun MyTests.dll \
  --collect "XPlat Code Coverage" \
  --mergeCoverageFormat Cobertura \
  --rerunMaxAttempts 3
```

Collect and merge coverage reports even when tests need multiple attempts to pass.

</details>

<details>
<summary><b>🎭 UI/Selenium Tests</b></summary>

```bash
test-rerun UITests.dll \
  --filter "Category=UI" \
  --rerunMaxAttempts 10 \
  --delay 3 \
  --blame
```

Handle flaky UI tests with generous retry attempts and blame mode for debugging.

</details>

---

## 📦 Installation Options

<table>
<tr>
<td width="50%">

### Global Tool (Recommended)

```bash
dotnet tool install --global dotnet-test-rerun
test-rerun --help
```

✅ Available system-wide  
✅ Easy to update  
✅ Simple command

</td>
<td width="50%">

### Docker

```bash
docker pull joaoopereira/dotnet-test-rerun:latest
docker run joaoopereira/dotnet-test-rerun:latest
```

✅ No installation needed  
✅ Multiple .NET versions  
✅ Isolated environment

</td>
</tr>
</table>

**Other options:** [Local Tool](https://joaoopereira.github.io/dotnet-test-rerun/installation#method-2-local-tool) • [NuGet Package](https://joaoopereira.github.io/dotnet-test-rerun/installation#method-4-nuget-package-reference)

---

## 🎛️ Configuration Options

### Core Options

| Option | Default | Description |
|--------|---------|-------------|
| `--rerunMaxAttempts` | `3` | Maximum number of retry attempts |
| `--rerunMaxFailedTests` | `-1` | Maximum failed tests to rerun (no limit) |
| `--rerunFailedThreshold` | `-1` | Max % of failed tests allowed to trigger a rerun (no limit) |
| `--delay, -d` | - | Delay between retries in seconds |
| `--filter` | - | Run tests matching the expression |
| `--deleteReports` | `false` | Clean up report files after execution |

### Test Execution

| Option | Description |
|--------|-------------|
| `--configuration, -c` | Build configuration (Debug/Release) |
| `--framework, -f` | Target framework to test |
| `--no-build` | Skip building before testing |
| `--blame` | Enable blame mode for diagnostics |
| `--collect` | Enable data collectors (e.g., code coverage) |

### Logging & Output

| Option | Default | Description |
|--------|---------|-------------|
| `--logger, -l` | `trx` | Test result logger (trx, junit, console) |
| `--results-directory, -r` | - | Output directory for test results |
| `--loglevel` | `Verbose` | Tool log level |
| `--verbosity, -v` | - | dotnet test verbosity level |
| `--logPassedTests` | `false` | Log each passed test as it completes during execution |

📘 **[View All Options](https://joaoopereira.github.io/dotnet-test-rerun/usage)** • **[Configuration Guide](https://joaoopereira.github.io/dotnet-test-rerun/configuration)** • **[Examples](https://joaoopereira.github.io/dotnet-test-rerun/examples)**

---

## 🐳 Docker Images

| Tag Pattern | .NET Runtime | Use Case |
|------------|--------------|----------|
| `latest`, `{version}` | .NET 10.0 | Latest stable release |
| `{version}-net10` | .NET 10.0 | Explicit .NET 10.0 |
| `{version}-net9` | .NET 9.0 | .NET 9.0 projects |
| `{version}-net8` | .NET 8.0 | .NET 8.0 projects (LTS) |

**Example:**
```bash
docker run joaoopereira/dotnet-test-rerun:4.0.0-net8 tests/MyTests.dll --rerunMaxAttempts 3
```

🐳 **[Docker Hub Repository](https://hub.docker.com/r/joaoopereira/dotnet-test-rerun)** • **[Docker Guide](https://joaoopereira.github.io/dotnet-test-rerun/docker)**

---

## 🤝 Contributing

Contributions are welcome! We appreciate:

- 🐛 Bug reports and fixes
- ✨ New features and enhancements
- 📖 Documentation improvements
- 💡 Ideas and suggestions

**Get Started:**
1. Check the [Contributing Guide](https://joaoopereira.github.io/dotnet-test-rerun/contributing)
2. Browse [Open Issues](https://github.com/joaoopereira/dotnet-test-rerun/issues)

---

## 👥 Community

<div align="center">

### Author

**João Pereira**  
[🌐 Website](https://joaoopereira.com) • [💼 GitHub](https://github.com/joaoopereira)

### Contributors

[![Contributors](https://contrib.rocks/image?repo=joaoopereira/dotnet-test-rerun)](https://github.com/joaoopereira/dotnet-test-rerun/graphs/contributors)

### Show Your Support

⭐ **Star this project** if it helped you!  
🐦 **Share** with your network  
💬 **Discuss** your use cases

</div>

---

## 📄 License

Copyright © 2023-2024 [João Pereira](https://github.com/joaoopereira)

This project is licensed under the **GNU General Public License v3.0** - see the [LICENSE](LICENSE) file for details.

---

## ⚠️ Language Support

**Important:** This tool currently supports **English only**. The output of `dotnet test` may be localized, and if it is not in English, the tool may not function correctly.

---

<div align="center">

**Made with ❤️ for the .NET community**

[Documentation](https://joaoopereira.github.io/dotnet-test-rerun/) • [NuGet](https://www.nuget.org/packages/dotnet-test-rerun) • [Docker Hub](https://hub.docker.com/r/joaoopereira/dotnet-test-rerun)

</div>

[1]: https://github.com/joaoopereira/dotnet-test-rerun/actions/workflows/cd.yml/badge.svg
[2]: https://github.com/joaoopereira/dotnet-test-rerun/actions/workflows/cd.yml
[3]: https://img.shields.io/nuget/v/dotnet-test-rerun.svg?label=dotnet-test-rerun
[4]: https://www.nuget.org/packages/dotnet-test-rerun
[5]: https://img.shields.io/nuget/dt/dotnet-test-rerun.svg?label=nuget-downloads
[6]: https://coveralls.io/repos/github/joaoopereira/dotnet-test-rerun/badge.svg?branch=main
[7]: https://coveralls.io/github/joaoopereira/dotnet-test-rerun?branch=main
[8]: https://img.shields.io/docker/pulls/joaoopereira/dotnet-test-rerun?label=docker-pulls
[9]: https://hub.docker.com/r/joaoopereira/dotnet-test-rerun
