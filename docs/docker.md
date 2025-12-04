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
# Latest version (NET 10.0)
docker pull joaoopereira/dotnet-test-rerun:latest

# Specific version and runtime
docker pull joaoopereira/dotnet-test-rerun:4.0.0-net8
```

### Run Tests

```bash
docker run -v $(pwd):/work -w /work \
  joaoopereira/dotnet-test-rerun:latest \
  path/to/test.dll
```

#### Windows (PowerShell)

```powershell
docker run -v ${PWD}:/work -w /work `
  joaoopereira/dotnet-test-rerun:latest `
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
  joaoopereira/dotnet-test-rerun:latest \
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

## CI/CD Integration

### GitHub Actions

```yaml
name: Docker Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Build Test Project
        run: dotnet build --configuration Release
      
      - name: Run Tests with Docker
        run: |
          docker run -v $(pwd):/work -w /work \
            joaoopereira/dotnet-test-rerun:latest \
            tests/**/bin/Release/**/MyTests.dll \
            --rerunMaxAttempts 3 \
            --results-directory /work/TestResults
      
      - name: Upload Results
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: test-results
          path: TestResults
```

### GitLab CI

```yaml
test:
  stage: test
  image: docker:latest
  services:
    - docker:dind
  script:
    - docker pull joaoopereira/dotnet-test-rerun:latest
    - docker run -v $(pwd):/work -w /work 
        joaoopereira/dotnet-test-rerun:latest 
        tests/bin/Release/net8.0/MyTests.dll 
        --rerunMaxAttempts 3
        --results-directory /work/TestResults
  artifacts:
    paths:
      - TestResults/
    when: always
```

### Jenkins

```groovy
pipeline {
    agent any
    
    stages {
        stage('Test') {
            steps {
                script {
                    docker.image('joaoopereira/dotnet-test-rerun:latest').inside {
                        sh '''
                            test-rerun tests/bin/Release/net8.0/MyTests.dll \
                                --rerunMaxAttempts 3 \
                                --results-directory TestResults
                        '''
                    }
                }
            }
        }
    }
    
    post {
        always {
            archiveArtifacts artifacts: 'TestResults/**/*', allowEmptyArchive: true
        }
    }
}
```

## Advanced Docker Usage

### Custom Entrypoint

Override the entrypoint for custom scripts:

```bash
docker run -v $(pwd):/work -w /work \
  --entrypoint /bin/sh \
  joaoopereira/dotnet-test-rerun:latest \
  -c "dotnet --info && test-rerun tests/MyTests.dll"
```

### Environment Variables

Pass environment variables to the container:

```bash
docker run \
  -v $(pwd):/work -w /work \
  -e ASPNETCORE_ENVIRONMENT=Testing \
  -e LOG_LEVEL=Debug \
  joaoopereira/dotnet-test-rerun:latest \
  tests/MyTests.dll \
  --rerunMaxAttempts 3
```

### Network Configuration

Connect to other containers or services:

```bash
# Create a network
docker network create test-network

# Run database container
docker run -d --name testdb --network test-network postgres:15

# Run tests connected to the network
docker run \
  -v $(pwd):/work -w /work \
  --network test-network \
  -e DATABASE_HOST=testdb \
  joaoopereira/dotnet-test-rerun:latest \
  tests/MyTests.dll \
  --filter "Category=Integration"
```

### Multi-Container Setup

Use Docker Compose for complex setups:

```yaml
version: '3.8'

services:
  database:
    image: postgres:15
    environment:
      POSTGRES_PASSWORD: testpassword
      POSTGRES_DB: testdb
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

  test:
    image: joaoopereira/dotnet-test-rerun:latest
    depends_on:
      database:
        condition: service_healthy
    volumes:
      - .:/app
    working_dir: /app
    environment:
      DATABASE_CONNECTION: "Host=database;Database=testdb;Username=postgres;Password=testpassword"
    command: >
      tests/bin/Release/net8.0/IntegrationTests.dll
      --rerunMaxAttempts 5
      --delay 5
      --filter "Category=Database"
```

## Building Custom Images

If you need to customize the image:

### Dockerfile Example

```dockerfile
FROM joaoopereira/dotnet-test-rerun:latest

# Install additional tools
RUN apt-get update && apt-get install -y \
    git \
    && rm -rf /var/lib/apt/lists/*

# Install additional .NET tools
RUN dotnet tool install --global dotnet-coverage

# Set custom environment variables
ENV PATH="${PATH}:/root/.dotnet/tools"

# Copy custom run settings
COPY test.runsettings /app/test.runsettings

WORKDIR /app
```

Build and use:

```bash
docker build -t my-test-runner .

docker run -v $(pwd):/app my-test-runner \
  tests/MyTests.dll \
  --settings /app/test.runsettings
```

## Debugging Docker Issues

### Check Image Contents

```bash
docker run -it --entrypoint /bin/bash \
  joaoopereira/dotnet-test-rerun:latest
```

### View .NET Info

```bash
docker run joaoopereira/dotnet-test-rerun:latest \
  dotnet --info
```

### Verify Tool Version

```bash
docker run joaoopereira/dotnet-test-rerun:latest \
  test-rerun --help
```

## Performance Considerations

### Volume Mount Performance

For better performance on Windows and macOS:

```bash
# Use delegated consistency mode (macOS)
docker run -v $(pwd):/work:delegated -w /work \
  joaoopereira/dotnet-test-rerun:latest \
  tests/MyTests.dll
```

### Resource Limits

Limit container resources:

```bash
docker run \
  --memory="4g" \
  --cpus="2" \
  -v $(pwd):/work -w /work \
  joaoopereira/dotnet-test-rerun:latest \
  tests/MyTests.dll
```

## Best Practices

1. **Pin to Specific Versions**: Use version tags instead of `latest` for reproducibility
   ```bash
   joaoopereira/dotnet-test-rerun:4.0.0-net8
   ```

2. **Match .NET Versions**: Use the Docker image that matches your test project's target framework

3. **Use Volume Mounts**: Mount your project directory to access test files and write results

4. **Set Working Directory**: Always set `-w /work` or use absolute paths

5. **Health Checks**: Use health checks when depending on other services

6. **Clean Up**: Remove containers after use with `--rm` flag
   ```bash
   docker run --rm -v $(pwd):/work -w /work \
     joaoopereira/dotnet-test-rerun:latest tests/MyTests.dll
   ```

7. **Network Isolation**: Use Docker networks for integration test isolation

## Troubleshooting

### Common Issues

**Issue**: "No such file or directory" errors

**Solution**: Ensure paths are absolute or relative to the working directory

```bash
docker run -v $(pwd):/work -w /work \
  joaoopereira/dotnet-test-rerun:latest \
  /work/tests/bin/Release/net8.0/MyTests.dll
```

**Issue**: Permission denied writing results

**Solution**: Set appropriate user permissions

```bash
docker run --user $(id -u):$(id -g) \
  -v $(pwd):/work -w /work \
  joaoopereira/dotnet-test-rerun:latest \
  tests/MyTests.dll
```

**Issue**: Cannot access local services

**Solution**: Use host network mode (Linux only)

```bash
docker run --network host \
  -v $(pwd):/work -w /work \
  joaoopereira/dotnet-test-rerun:latest \
  tests/MyTests.dll
```

## Next Steps

- [Examples](examples) - See complete Docker examples
- [Configuration](configuration) - Configure test execution
- [Usage](usage) - Learn all command options
