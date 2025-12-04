---
layout: default
title: Examples
---

# Examples

This page provides real-world examples and use cases for dotnet-test-rerun.

## Basic Examples

### Example 1: Simple Retry

Run tests with basic retry logic:

```bash
test-rerun MyTests.dll --rerunMaxAttempts 3
```

**Use Case**: Generic flaky test suite with occasional failures.

### Example 2: Filtered Tests

Target only integration tests:

```bash
test-rerun MyTests.dll --filter "Category=Integration" --rerunMaxAttempts 5
```

**Use Case**: Only retry integration tests that interact with external services.

### Example 3: With Delay

Add a 10-second delay between retries:

```bash
test-rerun MyTests.dll --delay 10 --rerunMaxAttempts 3
```

**Use Case**: Tests depend on external APIs with rate limiting or need time for services to stabilize.

## CI/CD Examples

### Example 4: GitHub Actions

```yaml
name: Test with Retry

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      
      - name: Install dotnet-test-rerun
        run: dotnet tool install --global dotnet-test-rerun
      
      - name: Build
        run: dotnet build --configuration Release
      
      - name: Run Tests with Retry
        run: |
          test-rerun **/bin/Release/**/MyProject.Tests.dll \
            --no-build \
            --configuration Release \
            --rerunMaxAttempts 3 \
            --rerunMaxFailedTests 20 \
            --logger "trx;LogFileName=test-results.trx" \
            --results-directory ./TestResults
      
      - name: Upload Test Results
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: test-results
          path: ./TestResults
```

### Example 5: Azure DevOps Pipeline

```yaml
trigger:
  - main

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: UseDotNet@2
  inputs:
    packageType: 'sdk'
    version: '8.0.x'

- script: dotnet tool install --global dotnet-test-rerun
  displayName: 'Install dotnet-test-rerun'

- script: dotnet build --configuration Release
  displayName: 'Build'

- script: |
    test-rerun $(Build.SourcesDirectory)/**/bin/Release/**/MyProject.Tests.dll \
      --no-build \
      --configuration Release \
      --rerunMaxAttempts 3 \
      --logger "trx;LogFileName=test-results.trx" \
      --results-directory $(Build.ArtifactStagingDirectory)/TestResults
  displayName: 'Run Tests with Retry'

- task: PublishTestResults@2
  condition: always()
  inputs:
    testResultsFormat: 'VSTest'
    testResultsFiles: '**/*.trx'
    searchFolder: '$(Build.ArtifactStagingDirectory)/TestResults'
```

### Example 6: GitLab CI

```yaml
test:
  stage: test
  image: mcr.microsoft.com/dotnet/sdk:8.0
  script:
    - dotnet tool install --global dotnet-test-rerun
    - export PATH="$PATH:$HOME/.dotnet/tools"
    - dotnet build --configuration Release
    - test-rerun tests/**/bin/Release/**/MyProject.Tests.dll 
        --no-build 
        --configuration Release 
        --rerunMaxAttempts 3 
        --logger "junit;LogFileName=junit-results.xml" 
        --results-directory ./TestResults
  artifacts:
    when: always
    reports:
      junit: TestResults/junit-results.xml
```

## Code Coverage Examples

### Example 7: Basic Coverage

Collect code coverage during test execution:

```bash
test-rerun MyTests.dll \
  --collect "XPlat Code Coverage" \
  --rerunMaxAttempts 3
```

### Example 8: Merged Coverage Report

Merge coverage from all test runs into a single report:

```bash
# First, ensure dotnet-coverage is installed
dotnet tool install --global dotnet-coverage

# Run tests with merged coverage
test-rerun MyTests.dll \
  --collect "Code Coverage" \
  --mergeCoverageFormat Cobertura \
  --rerunMaxAttempts 3 \
  --results-directory ./coverage
```

### Example 9: Coverage in CI/CD

```yaml
- name: Test with Coverage
  run: |
    dotnet tool install --global dotnet-coverage
    test-rerun MyTests.dll \
      --collect "XPlat Code Coverage" \
      --mergeCoverageFormat Cobertura \
      --rerunMaxAttempts 3 \
      --results-directory ./coverage

- name: Upload Coverage to Codecov
  uses: codecov/codecov-action@v4
  with:
    files: ./coverage/coverage.cobertura.xml
```

## Advanced Examples

### Example 10: Multiple Test Filters

Run specific categories of tests with different configurations:

```bash
# Run integration tests with retries
test-rerun MyTests.dll \
  --filter "Category=Integration" \
  --rerunMaxAttempts 5 \
  --delay 5

# Run API tests with different settings
test-rerun MyTests.dll \
  --filter "Category=API" \
  --rerunMaxAttempts 3 \
  --delay 2
```

### Example 11: Environment-Specific Configuration

```bash
test-rerun MyTests.dll \
  --rerunMaxAttempts 5 \
  --environment "ASPNETCORE_ENVIRONMENT=Testing" \
  --environment "DATABASE_CONNECTION=Server=testdb;Database=test" \
  --environment "API_TIMEOUT=30" \
  --filter "Category=Integration"
```

### Example 12: Blame Mode for Debugging

Identify problematic tests:

```bash
test-rerun MyTests.dll \
  --blame \
  --verbosity detailed \
  --loglevel Debug \
  --rerunMaxAttempts 3 \
  --results-directory ./blame-results
```

### Example 13: Parallel Execution with Limited CPU

```bash
test-rerun MyTests.dll \
  --rerunMaxAttempts 3 \
  -m:2 \
  -- RunConfiguration.MaxCpuCount=2
```

### Example 14: Custom Run Settings

Create a file `test.runsettings`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <RunConfiguration>
    <MaxCpuCount>2</MaxCpuCount>
    <ResultsDirectory>./TestResults</ResultsDirectory>
    <TestSessionTimeout>600000</TestSessionTimeout>
  </RunConfiguration>
  <LoggerRunSettings>
    <Loggers>
      <Logger friendlyName="console" enabled="true">
        <Configuration>
          <Verbosity>minimal</Verbosity>
        </Configuration>
      </Logger>
    </Loggers>
  </LoggerRunSettings>
</RunSettings>
```

Use the run settings file:

```bash
test-rerun MyTests.dll \
  --settings test.runsettings \
  --rerunMaxAttempts 3
```

## Docker Examples

### Example 15: Basic Docker Usage

```bash
docker run -v $(pwd):/work -w /work \
  joaoopereira/dotnet-test-rerun:latest \
  tests/bin/Release/net8.0/MyTests.dll \
  --rerunMaxAttempts 3
```

### Example 16: Docker with Specific .NET Version

```bash
# Use .NET 8.0 image
docker run -v $(pwd):/work -w /work \
  joaoopereira/dotnet-test-rerun:1.0.0-net8 \
  tests/bin/Release/net8.0/MyTests.dll \
  --rerunMaxAttempts 3

# Use .NET 9.0 image
docker run -v $(pwd):/work -w /work \
  joaoopereira/dotnet-test-rerun:1.0.0-net9 \
  tests/bin/Release/net9.0/MyTests.dll \
  --rerunMaxAttempts 3
```

### Example 17: Docker Compose

Create `docker-compose.yml`:

```yaml
version: '3.8'

services:
  test:
    image: joaoopereira/dotnet-test-rerun:latest
    volumes:
      - .:/app
    working_dir: /app
    command: >
      tests/bin/Release/net8.0/MyTests.dll
      --rerunMaxAttempts 3
      --filter "Category=Integration"
      --results-directory /app/TestResults
```

Run with:

```bash
docker-compose run test
```

## Real-World Scenarios

### Scenario 1: Microservices Integration Tests

Testing microservices with potential network hiccups:

```bash
test-rerun IntegrationTests.dll \
  --filter "Category=Microservices" \
  --rerunMaxAttempts 5 \
  --delay 10 \
  --rerunMaxFailedTests 15 \
  --environment "SERVICE_TIMEOUT=60" \
  --logger "trx;LogFileName=integration-results.trx"
```

### Scenario 2: Database Tests with Connection Pool Issues

```bash
test-rerun DatabaseTests.dll \
  --filter "FullyQualifiedName~Database" \
  --rerunMaxAttempts 3 \
  --delay 5 \
  --environment "CONNECTION_POOL_SIZE=50" \
  --environment "CONNECTION_TIMEOUT=30"
```

### Scenario 3: UI/Selenium Tests with Timing Issues

```bash
test-rerun UITests.dll \
  --filter "Category=UI|Category=Selenium" \
  --rerunMaxAttempts 10 \
  --delay 3 \
  --rerunMaxFailedTests 10 \
  --blame \
  --verbosity detailed
```

### Scenario 4: Performance Tests with Warm-up

```bash
test-rerun PerformanceTests.dll \
  --filter "Category=Performance" \
  --rerunMaxAttempts 3 \
  --delay 30 \
  --rerunMaxFailedTests 5 \
  -- RunConfiguration.MaxCpuCount=1
```

### Scenario 5: End-to-End Tests in CI

```bash
test-rerun E2ETests.dll \
  --configuration Release \
  --no-build \
  --filter "Category=E2E&Priority=High" \
  --rerunMaxAttempts 3 \
  --rerunMaxFailedTests 10 \
  --delay 5 \
  --logger "trx;LogFileName=e2e-results.trx" \
  --logger "console;verbosity=minimal" \
  --results-directory ./E2EResults \
  --deleteReports \
  --collect "XPlat Code Coverage" \
  --mergeCoverageFormat Cobertura
```

## Tips for Different Test Types

### Unit Tests
Usually don't need retries, but if they're flaky:
```bash
test-rerun UnitTests.dll --rerunMaxAttempts 2 --rerunMaxFailedTests 5
```

### Integration Tests
Often need retries due to external dependencies:
```bash
test-rerun IntegrationTests.dll --rerunMaxAttempts 5 --delay 5 --rerunMaxFailedTests 20
```

### End-to-End Tests
Typically the most flaky, need generous retry configuration:
```bash
test-rerun E2ETests.dll --rerunMaxAttempts 10 --delay 10 --rerunMaxFailedTests 15
```

### Performance Tests
Should have limited retries to avoid skewing results:
```bash
test-rerun PerfTests.dll --rerunMaxAttempts 2 --delay 30
```

## Next Steps

- [Configuration](configuration) - Deep dive into all configuration options
- [Docker](docker) - More Docker-specific examples
- [Contributing](contributing) - Contribute your own examples
