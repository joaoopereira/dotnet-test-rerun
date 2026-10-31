---
layout: default
title: Installation
nav_order: 2
---

# Installation
{: .no_toc }

<details open markdown="block">
  <summary>Contents</summary>
  {: .text-delta }
- TOC
{:toc}
</details>

## Requirements

- A .NET SDK, version 8.0 or later. The tool runs on .NET 8, 9 and 10.
- Test projects that run with `dotnet test` (xUnit, NUnit or MSTest on VSTest).

The SDK runs the tool; your test projects can target other frameworks, as long as that SDK can build and run them.

## Global tool

Recommended for local use. The command is available in every folder:

```bash
dotnet tool install --global dotnet-test-rerun
test-rerun --help
```

Update or remove it:

```bash
dotnet tool update --global dotnet-test-rerun
dotnet tool uninstall --global dotnet-test-rerun
```

## Local tool

Recommended for CI and teams. The version is pinned in `.config/dotnet-tools.json`, which you commit:

```bash
dotnet new tool-manifest      # once per repository, if there is no manifest yet
dotnet tool install dotnet-test-rerun
```

Run it with `dotnet` in front:

```bash
dotnet tool restore           # on a fresh clone or in CI
dotnet test-rerun --help
```

## A specific or pre-release version

```bash
dotnet tool install --global dotnet-test-rerun --version 4.2.0

# Pre-releases and preview builds of pull requests
dotnet tool install --global dotnet-test-rerun --version 5.0.0-alpha.0
```

The versions are listed on [NuGet](https://www.nuget.org/packages/dotnet-test-rerun#versions-body-tab).

## Docker

```bash
docker pull joaoopereira/dotnet-test-rerun:latest
```

See the [Docker guide](docker).

## Check the installed version

```bash
dotnet tool list --global dotnet-test-rerun   # global tool
dotnet tool list dotnet-test-rerun            # local tool
```

## `test-rerun: command not found`

Global tools are installed in a folder that must be on your `PATH`:

- Windows: `%USERPROFILE%\.dotnet\tools`
- Linux and macOS: `$HOME/.dotnet/tools`

Open a new terminal after the first install. In containers and CI images, add the folder yourself, for example `export PATH="$PATH:$HOME/.dotnet/tools"`.
