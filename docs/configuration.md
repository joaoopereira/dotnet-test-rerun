---
layout: default
title: Configuration
nav_order: 4
---

# Configuration Guide

This guide covers advanced configuration options and settings for dotnet-test-rerun.

## Run Settings Files

You can use a run settings file (`.runsettings`) to configure test execution behavior. Specify it with the `--settings` option:

```bash
test-rerun test.dll --settings myconfig.runsettings
```

### Example Run Settings File

```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <RunConfiguration>
    <MaxCpuCount>1</MaxCpuCount>
    <ResultsDirectory>./TestResults</ResultsDirectory>
    <TargetPlatform>x64</TargetPlatform>
    <TargetFrameworkVersion>net8.0</TargetFrameworkVersion>
    <TestSessionTimeout>3600000</TestSessionTimeout>
  </RunConfiguration>

  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="Code Coverage">
        <Configuration>
          <Format>Cobertura</Format>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>

  <LoggerRunSettings>
    <Loggers>
      <Logger friendlyName="trx" />
    </Loggers>
  </LoggerRunSettings>
</RunSettings>
```

## Inline Run Settings

For simple configurations, you can use inline run settings without a file:

```bash
test-rerun test.dll -- RunConfiguration.MaxCpuCount=1 RunConfiguration.TargetPlatform=x64
```

Note: Inline run settings must be specified after `--` and must be the last arguments.

## Environment Variables

Set environment variables for your test execution:

```bash
test-rerun test.dll -e "ASPNETCORE_ENVIRONMENT=Testing" -e "LOG_LEVEL=Debug"
```

Multiple environment variables can be set by repeating the `-e` or `--environment` flag.

## Retry Configuration

### Maximum Attempts

Control how many times failed tests are retried:

```bash
# Default is 3 attempts
test-rerun test.dll --rerunMaxAttempts 5
```

**Choosing the Right Value:**
- For occasional flaky tests: 2-3 attempts
- For highly intermittent tests: 5-10 attempts
- For tests with external dependencies: 3-5 attempts with delays

### Maximum Failed Tests Limit

Prevent excessive reruns when too many tests fail:

```bash
# Don't rerun if more than 10 tests fail
test-rerun test.dll --rerunMaxFailedTests 10
```

This is useful to:
- Fail fast when there's a systemic issue
- Avoid wasting CI/CD time on widespread failures
- Distinguish between isolated flaky tests and real problems

**Recommended Values:**
- Small test suites (< 100 tests): 5-10
- Medium test suites (100-500 tests): 10-20
- Large test suites (> 500 tests): 20-50

### Retry Delay

Add delays between retry attempts to allow external resources to recover:

```bash
# Wait 5 seconds between retry attempts
test-rerun test.dll --delay 5
```

**Use Cases:**
- Database connection pooling issues
- Rate limiting from external APIs
- Service warm-up time
- Cache synchronization

## Test Filtering

Use the `--filter` option to target specific tests:

### Filter by Category

```bash
test-rerun test.dll --filter "Category=Integration"
```

### Filter by Namespace

```bash
test-rerun test.dll --filter "FullyQualifiedName~MyNamespace.IntegrationTests"
```

### Filter by Test Name

```bash
test-rerun test.dll --filter "Name~DatabaseTest"
```

### Complex Filters

Combine multiple criteria:

```bash
# AND operation
test-rerun test.dll --filter "Category=Integration&Priority=High"

# OR operation
test-rerun test.dll --filter "Category=Integration|Category=EndToEnd"

# NOT operation
test-rerun test.dll --filter "Category!=Unit"
```

## Logging Configuration

### Tool Log Level

Control dotnet-test-rerun's own logging:

```bash
test-rerun test.dll --loglevel Minimal
```

Options: `Quiet`, `Minimal`, `Normal`, `Verbose`, `Debug`

### Test Execution Verbosity

Control the verbosity of the underlying `dotnet test` command:

```bash
test-rerun test.dll --verbosity detailed
```

Options: `quiet`, `minimal`, `normal`, `detailed`, `diagnostic`

### Test Loggers

Specify which loggers to use for test results:

```bash
# Single logger
test-rerun test.dll --logger "trx;LogFileName=results.trx"

# Multiple loggers
test-rerun test.dll --logger trx --logger "console;verbosity=detailed"

# JUnit format (useful for Jenkins, GitLab CI)
test-rerun test.dll --logger "junit;LogFileName=junit-results.xml"
```

Common logger formats:
- `trx` - Visual Studio Test Results File
- `junit` - JUnit XML format
- `html` - HTML report (requires additional package)
- `console` - Console output

## Code Coverage Configuration

### Basic Coverage Collection

```bash
test-rerun test.dll --collect "XPlat Code Coverage"
```

### Merge Coverage Reports

When using retries, coverage reports can be merged:

```bash
test-rerun test.dll \
  --collect "XPlat Code Coverage" \
  --mergeCoverageFormat Cobertura
```

**Prerequisites:** Install `dotnet-coverage` tool:
```bash
dotnet tool install --global dotnet-coverage
```

### Coverage Formats

- `Coverage` - Binary Microsoft code coverage format
- `Cobertura` - XML format (widely supported by CI/CD systems)
- `Xml` - Alternative XML format

## Build Configuration

### Skip Build

When tests are already built:

```bash
test-rerun test.dll --no-build
```

### Skip Restore

When dependencies are already restored:

```bash
test-rerun test.dll --no-restore
```

### Configuration and Framework

```bash
test-rerun test.dll --configuration Release --framework net8.0
```

### MSBuild Properties

Pass properties to MSBuild:

```bash
test-rerun test.dll /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

## Results Directory

Specify where test results are stored:

```bash
test-rerun test.dll --results-directory ./TestResults
```

Clean up results after execution:

```bash
test-rerun test.dll --deleteReports
```

## Blame Mode

Enable blame mode to identify problematic tests:

```bash
test-rerun test.dll --blame
```

Blame mode will:
- Create a sequence file showing test execution order
- Capture a crash dump if the test host crashes
- Help identify tests that cause hangs or crashes

## Complete Configuration Example

Here's a comprehensive example combining multiple options:

```bash
test-rerun MyProject.Tests.dll \
  --configuration Release \
  --framework net8.0 \
  --no-restore \
  --filter "Category=Integration|Category=EndToEnd" \
  --rerunMaxAttempts 5 \
  --rerunMaxFailedTests 15 \
  --delay 3 \
  --logger "trx;LogFileName=test-results.trx" \
  --logger "console;verbosity=minimal" \
  --results-directory ./TestResults \
  --collect "XPlat Code Coverage" \
  --mergeCoverageFormat Cobertura \
  --loglevel Normal \
  --verbosity minimal \
  --environment "TEST_ENV=CI" \
  --environment "DATABASE_TIMEOUT=30" \
  --blame \
  /p:CollectCoverage=true
```

## Configuration Best Practices

1. **Start Conservative**: Begin with low retry attempts (2-3) and increase only if needed

2. **Use Filters Wisely**: Only rerun tests that are known to be flaky; use filters to target them

3. **Set Failure Limits**: Always set `--rerunMaxFailedTests` to prevent excessive reruns

4. **Add Delays for External Dependencies**: Use `--delay` when tests depend on external services

5. **Clean Up Artifacts**: Use `--deleteReports` in CI/CD pipelines to avoid accumulating files

6. **Appropriate Verbosity**: Use lower verbosity in CI/CD, higher when debugging locally

7. **Collect Coverage Carefully**: Coverage collection adds overhead; only enable when needed

8. **Use Run Settings Files**: For complex configurations, use `.runsettings` files for maintainability

## Next Steps

- [Examples](examples) - See complete real-world examples
- [Usage](usage) - Review all available options
- [Docker](docker) - Configure Docker deployments
