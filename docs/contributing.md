---
layout: default
title: Contributing
nav_order: 9
---

# Contributing
{: .no_toc }

Bug reports, ideas, documentation fixes and pull requests are welcome.

<details open markdown="block">
  <summary>Contents</summary>
  {: .text-delta }
- TOC
{:toc}
</details>

## Reporting a bug

[Open an issue](https://github.com/joaoopereira/dotnet-test-rerun/issues/new) with the information listed in [Troubleshooting](troubleshooting#still-stuck). A small project that reproduces the problem helps the most, especially for tests that are not rerun or rerun wrongly: include a test name and the printed `Rerun filter: …`.

## Development setup

You need:

- the .NET SDK version in [`global.json`](https://github.com/joaoopereira/dotnet-test-rerun/blob/main/global.json) (10.0);
- the .NET 8 and 9 runtimes, for the integration test fixtures.

The [dev container](https://github.com/joaoopereira/dotnet-test-rerun/tree/main/.devcontainer) installs .NET 10 and 8.

```bash
git clone https://github.com/joaoopereira/dotnet-test-rerun.git
cd dotnet-test-rerun
dotnet build
```

The first restore installs [Husky.Net](https://alirezanet.github.io/Husky.Net/) git hooks:

- **pre-commit** runs the unit tests;
- **commit-msg** checks the commit message format (see below).

## Running the tests

```bash
# Unit tests: fast, run them on every change
dotnet test test/dotnet-test-rerun.UnitTests/dotnet-test-rerun.UnitTests.csproj

# Integration tests: run the real tool against the fixture projects (several minutes)
dotnet test test/dotnet-test-rerun.IntegrationTests/dotnet-test-rerun.IntegrationTests.csproj
```

Run the tool from source:

```bash
dotnet run --project src/dotnet-test-rerun.csproj --framework net10.0 -- path/to/MyTests.csproj
```

## Project layout

| Folder | Contents |
|---|---|
| `src/Cli` | Command-line options (`RerunCliDefinition`) and the exit code mapping (`RerunRootCommand`) |
| `src/Configuration` | `RerunOptions`, the parsed options |
| `src/Execution` | The rerun loop (`RerunOrchestrator`) and the argument lists for `dotnet test` and `dotnet-coverage` |
| `src/DotNetRunner` | Starts `dotnet test` and `dotnet-coverage`, and classifies how a run ended |
| `src/Analyzers` | Reads TRX files and builds the rerun filters, one strategy per test framework |
| `src/Logging` | Console output with Spectre.Console |
| `test/dotnet-test-rerun.UnitTests` | Unit tests |
| `test/dotnet-test-rerun.IntegrationTests` | Integration tests; the projects they run are in `Fixtures/` |

## Adding a command-line option

1. Add an init-only property to `src/Configuration/RerunOptions.cs`, with the same default as the CLI.
2. Define the option in `src/Cli/RerunCliDefinition.cs`, register it in `AddTo` and map it in `ToOptions`.
3. If `dotnet test` must receive it, add it to `src/Execution/DotNetTestArguments.cs`.
4. Add unit tests.
5. Document it in the [command reference](usage) (`docs/usage.md`). If it needs more than one line, also add a section to `docs/configuration.md` (Guides). Add it to the "Main options" table of `README.md` only if most users need it.

## Commit messages

Commits follow [Conventional Commits](https://www.conventionalcommits.org/): `type(scope): subject`, at most 90 characters, no trailing period. The hook and CI reject other formats.

Types: `feat`, `fix`, `docs`, `test`, `refactor`, `perf`, `build`, `ci`, `chore`, `style`, `revert`.

```text
feat(rerun): add --delay between reruns
fix(analyzer): escape braces in NUnit test names
docs: document the attempt-N results folders
```

The CHANGELOG is generated from these messages at release time, so do not edit it or the version in pull requests.

## Pull requests

1. Create a branch and make your change, with tests.
2. Update the documentation in `docs/` and, if needed, `README.md`.
3. Run the unit tests (the integration tests too, if you changed how tests are run or rerun).
4. Open a pull request and fill in the template. CI checks the commit messages and runs all tests.

Maintainers can add the `preview` label to a pull request to publish a preview package to NuGet, so the change can be tried before it is merged.

## Editing this documentation

The site is built with [Jekyll](https://jekyllrb.com/) and the [Just the Docs](https://just-the-docs.com/) theme from the `docs/` folder. See [`docs/README.md`](https://github.com/joaoopereira/dotnet-test-rerun/blob/main/docs/README.md) to preview it locally.

## License

By contributing, you agree that your contributions are licensed under the [GNU General Public License v3.0](https://github.com/joaoopereira/dotnet-test-rerun/blob/main/LICENSE).
