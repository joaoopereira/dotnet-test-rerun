---
layout: default
title: Usage
nav_order: 3
---

# Usage Guide

This guide covers the basic and advanced usage of dotnet-test-rerun.

## Basic Usage

The simplest way to use dotnet-test-rerun is to provide the path to your test DLL:

```bash
test-rerun path/to/your/test.dll
```

This will:
1. Run all tests in the DLL
2. Identify any failed tests
3. Automatically rerun failed tests up to 3 times (default)
4. Report final results

## Command Syntax

```bash
test-rerun <path> [OPTIONS]
```

### Arguments

| Argument | Description |
|----------|-------------|
| `path` | Path to a test project .dll file |

## Common Options

### Retry Configuration

**`--rerunMaxAttempts <number>`**
- Maximum number of retry attempts
- Default: `3`
- Example: `test-rerun test.dll --rerunMaxAttempts 5`

**`--rerunMaxFailedTests <number>`**
- Maximum number of failed tests to rerun
- If exceeded, no tests will be rerun
- Default: `-1` (no limit)
- Example: `test-rerun test.dll --rerunMaxFailedTests 10`

**`--delay, -d <seconds>`**
- Delay between test runs in seconds
- Useful for allowing external resources to recover
- Example: `test-rerun test.dll --delay 5`

### Test Filtering

**`--filter <expression>`**
- Run tests that match the given expression
- Uses standard dotnet test filter syntax
- Example: `test-rerun test.dll --filter "FullyQualifiedName~Integration"`

### Output Configuration

**`--logger, -l <logger>`**
- Specifies a logger for test results
- Multiple values allowed
- Default: `trx`
- Example: `test-rerun test.dll --logger "trx;LogFileName=results.trx"`

**`--results-directory, -r <path>`**
- Directory where test results are placed
- Directory is created if it doesn't exist
- Example: `test-rerun test.dll --results-directory ./TestResults`

**`--loglevel <level>`**
- Log level for the tool output
- Values: `Quiet`, `Minimal`, `Normal`, `Verbose`, `Debug`
- Default: `Verbose`
- Example: `test-rerun test.dll --loglevel Minimal`

**`--deleteReports`**
- Delete generated report files after execution
- Useful for CI/CD pipelines
- Example: `test-rerun test.dll --deleteReports`

### Build Configuration

**`--configuration, -c <config>`**
- Build configuration to use
- Common values: `Debug`, `Release`
- Example: `test-rerun test.dll --configuration Release`

**`--framework, -f <framework>`**
- Target framework
- Example: `test-rerun test.dll --framework net8.0`

**`--no-build`**
- Skip building the project before testing
- Implies `--no-restore`
- Example: `test-rerun test.dll --no-build`

**`--no-restore`**
- Skip restoring dependencies before building
- Example: `test-rerun test.dll --no-restore`

### Test Execution

**`--verbosity, -v <level>`**
- Verbosity level of the dotnet test command
- Values: `quiet`, `minimal`, `normal`, `detailed`, `diagnostic`
- Example: `test-rerun test.dll --verbosity detailed`

**`--blame`**
- Run tests in blame mode
- Helps identify problematic tests
- Example: `test-rerun test.dll --blame`

**`--environment, -e <variable=value>`**
- Set environment variables
- Can be specified multiple times
- Example: `test-rerun test.dll -e "ENV=Production" -e "DEBUG=false"`

### Code Coverage

**`--collect <collector>`**
- Enable data collector for the test run
- Example: `test-rerun test.dll --collect "Code Coverage"`
- Example: `test-rerun test.dll --collect "XPlat Code Coverage"`

**`--mergeCoverageFormat <format>`**
- Output format for merged coverage
- Values: `Coverage`, `Cobertura`, `Xml`
- Requires `dotnet-coverage` tool to be installed
- Example: `test-rerun test.dll --collect "Code Coverage" --mergeCoverageFormat Cobertura`

### Advanced Options

**`--settings, -s <file>`**
- Run settings file to use
- Example: `test-rerun test.dll --settings test.runsettings`

**`--inlineRunSettings, -- <settings>`**
- Inline run settings configuration
- Must be the last option
- Example: `test-rerun test.dll -- RunConfiguration.MaxCpuCount=1`

## MSBuild Arguments

MSBuild arguments can be passed through using:
- `/p:` or `-p:` or `--property:` for properties
- `-m:` or `/m:` for max CPU count

Examples:
```bash
test-rerun test.dll /p:CollectCoverage=true
test-rerun test.dll -m:3
```

## Usage Examples

### Example 1: Basic Retry with Custom Attempts

```bash
test-rerun MyTests.dll --rerunMaxAttempts 5
```

### Example 2: Filter Specific Tests

```bash
test-rerun MyTests.dll --filter "Category=Integration" --rerunMaxAttempts 3
```

### Example 3: CI/CD Pipeline

```bash
test-rerun MyTests.dll \
  --configuration Release \
  --no-build \
  --logger "trx;LogFileName=results.trx" \
  --results-directory ./TestResults \
  --rerunMaxAttempts 3 \
  --deleteReports
```

### Example 4: With Code Coverage

```bash
test-rerun MyTests.dll \
  --collect "XPlat Code Coverage" \
  --mergeCoverageFormat Cobertura \
  --rerunMaxAttempts 3
```

### Example 5: With Delay Between Retries

```bash
test-rerun MyTests.dll \
  --rerunMaxAttempts 5 \
  --delay 10 \
  --filter "Category=External"
```

### Example 6: Blame Mode for Debugging

```bash
test-rerun MyTests.dll \
  --blame \
  --verbosity detailed \
  --rerunMaxAttempts 3
```

### Example 7: Multiple Test Filters

```bash
test-rerun MyTests.dll \
  --filter "FullyQualifiedName~MyNamespace&Category=Flaky" \
  --rerunMaxAttempts 10 \
  --delay 5
```

## Exit Codes

The tool returns different exit codes based on execution results:

- `0`: All tests passed (or passed after retries)
- `1`: Tests failed after maximum retry attempts
- `2`: Error during execution

## Tips and Best Practices

1. **Use `--delay` for External Dependencies**: Add delays when tests depend on external services that may need time to recover

2. **Limit Reruns with `--rerunMaxFailedTests`**: In large test suites, limit how many failed tests will be rerun to avoid excessive execution time

3. **Use Filters**: Target specific flaky tests with `--filter` to avoid rerunning all tests unnecessarily

4. **CI/CD Integration**: Use `--deleteReports` in CI/CD to keep artifacts clean

5. **Code Coverage**: When collecting coverage, use `--mergeCoverageFormat` to get a single consolidated report

6. **Blame Mode**: Use `--blame` when debugging intermittent failures to identify which tests are problematic

7. **Log Levels**: Use `--loglevel Minimal` in CI/CD for cleaner logs, and `Verbose` or `Debug` when troubleshooting

## Next Steps

- [Configuration](configuration) - Learn about advanced configuration options
- [Examples](examples) - See more real-world scenarios
- [Docker](docker) - Use the tool with Docker
