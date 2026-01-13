---
layout: default
title: Docker
nav_order: 6
---

# Docker Guide

dotnet-test-rerun is available as Docker images for containerized environments. This guide covers using the tool with Docker.

## Available Images

Docker images are available for multiple .NET runtime versions:

| Tag | .NET Runtime | Description |
|-----|-------------|-------------|
| `latest` | .NET 10.0 | Latest version with .NET 10.0 runtime |
| `{version}` | .NET 10.0 | Specific version with .NET 10.0 runtime |
| `{version}-net10` | .NET 10.0 | Explicit .NET 10.0 image |
| `{version}-dotnet10` | .NET 10.0 | Explicit .NET 10.0 image (alternative tag) |
| `{version}-net9` | .NET 9.0 | .NET 9.0 image |
| `{version}-dotnet9` | .NET 9.0 | .NET 9.0 image (alternative tag) |
| `{version}-net8` | .NET 8.0 | .NET 8.0 image |
| `{version}-dotnet8` | .NET 8.0 | .NET 8.0 image (alternative tag) |

## Docker Hub

Images are hosted on Docker Hub: [joaoopereira/dotnet-test-rerun](https://hub.docker.com/r/joaoopereira/dotnet-test-rerun)

## Basic Usage

### Pull the Image

```bash
# Latest stable version (NET 10.0)
docker pull joaoopereira/dotnet-test-rerun:latest

# Specific version and runtime
docker pull joaoopereira/dotnet-test-rerun:3.4.0-net8
```

### Run Tests

```bash
docker run -v $(pwd):/work -w /work \
  joaoopereira/dotnet-test-rerun:latest \
  path/to/test.dll
```

## Using Specific .NET Versions

### .NET 8.0

```bash
docker run -v $(pwd):/work -w /work \
  joaoopereira/dotnet-test-rerun:4.0.0-net8 \
  tests/bin/Release/net8.0/MyTests.dll \
  --rerunMaxAttempts 3
```

### .NET 9.0

```bash
docker run -v $(pwd):/work -w /work \
  joaoopereira/dotnet-test-rerun:4.0.0-net9 \
  tests/bin/Release/net9.0/MyTests.dll \
  --rerunMaxAttempts 3
```

### .NET 10.0

```bash
docker run -v $(pwd):/work -w /work \
  joaoopereira/dotnet-test-rerun:4.0.0-net10 \
  tests/bin/Release/net10.0/MyTests.dll \
  --rerunMaxAttempts 3
```

## Volume Mounting

Mount your project directory to access test DLLs and write results:

```bash
docker run \
  -v $(pwd):/work \        # Mount current directory
  -w /work \               # Set working directory
  joaoopereira/dotnet-test-rerun:latest \
  tests/MyTests.dll \
  --results-directory /work/TestResults
```

## Docker Compose

Create a `docker-compose.yml` file:

```yaml
version: '3.8'

services:
  test:
    image: joaoopereira/dotnet-test-rerun:latest
    volumes:
      - .:/app
      - test-results:/app/TestResults
    working_dir: /app
    command: >
      tests/bin/Release/net8.0/MyTests.dll
      --rerunMaxAttempts 3
      --results-directory /app/TestResults
      --logger "trx;LogFileName=results.trx"

volumes:
  test-results:
```

Run tests:

```bash
docker-compose run test
```

## Next Steps

- [Examples](examples) - See complete Docker examples
- [Configuration](configuration) - Configure test execution
- [Usage](usage) - Learn all command options
