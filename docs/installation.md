---
layout: default
title: Installation
nav_order: 2
---

# Installation Guide

dotnet-test-rerun can be installed in multiple ways depending on your needs and environment.

## Prerequisites

- .NET SDK 8.0 or 9.0 (.NET 10.0 requires v4 alpha)
- A .NET test project (compatible with `dotnet test`)

## Method 1: Global Tool (Recommended)

Install dotnet-test-rerun as a global .NET tool:

```bash
dotnet tool install --global dotnet-test-rerun
```

After installation, the tool will be available globally as `test-rerun`:

```bash
test-rerun --help
```

### Updating the Global Tool

To update to the latest version:

```bash
dotnet tool update --global dotnet-test-rerun
```

### Uninstalling the Global Tool

To remove the tool:

```bash
dotnet tool uninstall --global dotnet-test-rerun
```

## Method 2: Local Tool

Install as a local tool in your project or solution:

### Step 1: Initialize a tool manifest (if not already present)

```bash
dotnet new tool-manifest
```

### Step 2: Install the local tool

```bash
dotnet tool install dotnet-test-rerun
```

### Step 3: Run the local tool

```bash
dotnet tool run test-rerun -- --help
```

Or use the shorter form:

```bash
dotnet test-rerun --help
```

## Method 3: Docker

Use the pre-built Docker images for containerized environments:

```bash
docker pull joaoopereira/dotnet-test-rerun:latest
```

See the [Docker Guide](docker) for more details on using Docker images.

## Method 4: NuGet Package Reference

While not typically needed for end-users, you can also reference the NuGet package directly in your project if you want to programmatically integrate the tool:

```xml
<ItemGroup>
  <PackageReference Include="dotnet-test-rerun" Version="*" />
</ItemGroup>
```

## Verifying Installation

After installation, verify the tool is working correctly:

```bash
test-rerun --help
```

You should see output showing all available options and usage information.

## Version Check

To check which version is installed:

```bash
dotnet tool list --global | grep dotnet-test-rerun
```

Or for local tools:

```bash
dotnet tool list
```

## Troubleshooting

### "Command not found" Error

If you get a "command not found" error after installing as a global tool:

1. Ensure the .NET tools directory is in your PATH:
   - **Windows**: `%USERPROFILE%\.dotnet\tools`
   - **Linux/macOS**: `$HOME/.dotnet/tools`

2. Restart your terminal/shell

3. Check if the tool is installed:
   ```bash
   dotnet tool list --global
   ```

### Permission Issues

If you encounter permission issues on Linux/macOS:

```bash
sudo dotnet tool install --global dotnet-test-rerun
```

Or install to a user directory without requiring sudo.

### .NET SDK Version Mismatch

If you get errors about .NET SDK versions:

1. Check your installed SDK versions:
   ```bash
   dotnet --list-sdks
   ```

2. Ensure you have .NET 8.0 or 9.0 installed (.NET 10.0 requires v4 alpha)

3. Update your .NET SDK if necessary from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)

## Next Steps

- [Usage Guide](usage) - Learn how to use the tool
- [Configuration](configuration) - Configure advanced options
- [Examples](examples) - See real-world examples
