# Multi-target Dockerfile for dotnet-test-rerun
# Supports .NET 8.0 and .NET 9.0
#
# Build for .NET 8.0:
#   docker build --build-arg TARGET_DOTNET_VERSION=8.0 -t dotnet-test-rerun:net8 .
#
# Build for .NET 9.0 (default):
#   docker build --build-arg TARGET_DOTNET_VERSION=9.0 -t dotnet-test-rerun:net9 .
#   or simply: docker build -t dotnet-test-rerun:net9 .
#
# Build both targets using bake:
#   docker buildx bake
#   docker buildx bake 8  # build only .NET 8
#   docker buildx bake 9  # build only .NET 9
#   docker buildx bake --set "*.version=1.0.0"  # with custom version

ARG TARGET_DOTNET_VERSION=9.0
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set working directory
WORKDIR /src

# Copy project files
COPY . .

# Restore dependencies for main project only
RUN dotnet restore src/dotnet-test-rerun.csproj /p:DockerBuild=true

# Build the application
RUN dotnet build src/dotnet-test-rerun.csproj --configuration Release --no-restore /p:DockerBuild=true

# Set the target framework based on build arg
ARG TARGET_DOTNET_VERSION
ENV TARGET_FRAMEWORK=net${TARGET_DOTNET_VERSION}

# Publish as framework-dependent application
RUN dotnet publish src/dotnet-test-rerun.csproj \
    --configuration Release \
    --framework $TARGET_FRAMEWORK \
    --output /app \
    --no-build \
    /p:DockerBuild=true

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:${TARGET_DOTNET_VERSION}

# Set working directory
WORKDIR /app

# Copy published application
COPY --from=build /app .

# Make the binary executable
RUN chmod +x test-rerun

# Set entrypoint
ENTRYPOINT ["./test-rerun"]