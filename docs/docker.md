---
layout: default
title: Docker
nav_order: 7
---

# Docker

Images are published on Docker Hub as [`joaoopereira/dotnet-test-rerun`](https://hub.docker.com/r/joaoopereira/dotnet-test-rerun). The entry point is `test-rerun`, so the container arguments are the tool arguments.

## Tags

| Tag | .NET | Notes |
|---|---|---|
| `latest` | 10.0 | Latest stable release. Pre-releases do not move it. |
| `<version>`, `<version>-net10`, `<version>-dotnet10` | 10.0 | |
| `<version>-net9`, `<version>-dotnet9` | 9.0 | |
| `<version>-net8`, `<version>-dotnet8` | 8.0 | |

`<version>` is a NuGet version such as `4.2.0` or `5.0.0-alpha.0`. Pin a version in CI instead of using `latest`.

## Running tests

Mount your repository and make it the working directory. Paths are then relative to the repository:

```bash
docker run --rm \
  -v "$PWD:/src" -w /src \
  joaoopereira/dotnet-test-rerun:5.0.0-net8 \
  tests/MyTests/MyTests.csproj \
  --rerunMaxAttempts 3 \
  --results-directory TestResults
```

The images are based on the .NET SDK image of the same version, because the tool runs `dotnet test`. Choose the image whose .NET version matches the target framework of your test projects.

{: .warning }
Images up to 5.0.0-alpha.0 are based on the .NET runtime image, which cannot run `dotnet test`. Use a later version.

## Docker Compose

```yaml
services:
  tests:
    image: joaoopereira/dotnet-test-rerun:5.0.0
    volumes:
      - .:/src
    working_dir: /src
    command: >
      MySolution.sln
      --rerunMaxAttempts 3
      --filter "Category=Integration"
      --results-directory TestResults
```

```bash
docker compose run --rm tests
```

## Building the images

The `Dockerfile` and `docker-bake.hcl` are at the root of the repository:

```bash
docker buildx bake                          # .NET 8, 9 and 10
docker buildx bake 8                        # only .NET 8
VERSION=1.2.3 docker buildx bake            # set the version used in the tags
```
