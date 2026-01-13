---
layout: default
title: Contributing
nav_order: 7
---

# Contributing to dotnet-test-rerun

Thank you for your interest in contributing to dotnet-test-rerun! This guide will help you get started.

## Ways to Contribute

- 🐛 Report bugs and issues
- 💡 Suggest new features or improvements
- 📝 Improve documentation
- 🔧 Submit bug fixes
- ✨ Add new features
- 🧪 Add or improve tests
- 📖 Share examples and use cases

## Getting Started

### Prerequisites

- .NET SDK 8.0, 9.0, or 10.0
- Git
- A code editor (Visual Studio, VS Code, Rider, etc.)

### Fork and Clone

1. Fork the repository on GitHub
2. Clone your fork locally:
   ```bash
   git clone https://github.com/YOUR-USERNAME/dotnet-test-rerun.git
   cd dotnet-test-rerun
   ```

3. Add the upstream repository:
   ```bash
   git remote add upstream https://github.com/joaoopereira/dotnet-test-rerun.git
   ```

### Build the Project

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build --configuration Release

# Run tests
dotnet test --configuration Release
```

Build should complete with no errors or warnings, and all 186 tests should pass.

## Making Changes

### Create a Branch

Create a feature branch from `main`:

```bash
git checkout -b feature/your-feature-name
```

Or for bug fixes:

```bash
git checkout -b fix/issue-description
```

### Code Style

Follow the existing code style in the project:

- Use C# naming conventions
- Keep methods focused and small
- Add XML documentation comments for public APIs
- Write clear, descriptive commit messages

### Commit Messages

Use [Conventional Commits](https://www.conventionalcommits.org/) format:

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `test`: Adding or updating tests
- `refactor`: Code refactoring
- `perf`: Performance improvements
- `chore`: Build process or tooling changes
- `ci`: CI/CD changes

**Examples:**

```
feat(rerun): add delay option between retries

Add a new --delay option that allows users to specify
a delay in seconds between test retry attempts.

Closes #123
```

```
fix: handle null reference in test filter parsing

Fixes NullReferenceException when test filter is empty.

Fixes #456
```

```
docs: update usage examples with new options
```

### Testing

Always add or update tests for your changes:

#### Unit Tests

Add unit tests to `test/dotnet-test-rerun.UnitTests/`:

```csharp
[Fact]
public void NewFeature_Should_BehaveCorrectly()
{
    // Arrange
    var sut = new YourClass();
    
    // Act
    var result = sut.MethodUnderTest();
    
    // Assert
    result.Should().Be(expectedValue);
}
```

#### Integration Tests

Add integration tests to `test/dotnet-test-rerun.IntegrationTests/` if your change affects test execution:

```csharp
[Fact]
public async Task NewFeature_Integration_Should_Work()
{
    // Test with actual test projects
}
```

#### Run Tests Locally

```bash
# Run all tests
dotnet test --configuration Release

# Run specific test project
dotnet test test/dotnet-test-rerun.UnitTests/

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## Pull Request Process

### Before Submitting

1. **Update from upstream**:
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

2. **Build and test**:
   ```bash
   dotnet build --configuration Release
   dotnet test --configuration Release
   ```

3. **Update documentation** if needed:
   - Update README.md
   - Update relevant documentation pages in `docs/`
   - Add examples if applicable

4. **Update CHANGELOG.md** (if applicable):
   Follow the existing format and add your changes under "Unreleased"

### Submit Pull Request

1. Push your changes to your fork:
   ```bash
   git push origin feature/your-feature-name
   ```

2. Go to GitHub and create a Pull Request

3. Fill in the PR template with:
   - Description of changes
   - Related issues (if any)
   - Testing performed
   - Screenshots (for UI changes)

4. Ensure CI checks pass:
   - Build succeeds
   - All tests pass
   - Code coverage is maintained or improved
   - Commit messages follow conventional format

### PR Review Process

- Maintainers will review your PR
- Address any feedback or requested changes
- Once approved, your PR will be merged

## Reporting Issues

### Bug Reports

When reporting bugs, include:

1. **Description**: Clear description of the bug
2. **Steps to Reproduce**: Detailed steps to reproduce the issue
3. **Expected Behavior**: What should happen
4. **Actual Behavior**: What actually happens
5. **Environment**:
   - OS (Windows, Linux, macOS)
   - .NET SDK version
   - dotnet-test-rerun version
6. **Test Project Details**:
   - Test framework (xUnit, NUnit, MSTest)
   - Target framework
7. **Logs/Output**: Relevant error messages or logs

### Feature Requests

For feature requests, include:

1. **Problem**: What problem does this solve?
2. **Proposed Solution**: Your suggested approach
3. **Alternatives**: Other approaches you've considered
4. **Use Case**: Real-world scenario where this helps

## Development Guidelines

### Project Structure

```
dotnet-test-rerun/
├── src/                          # Main application code
│   ├── Analyzers/               # Test result analysis
│   ├── Domain/                  # Domain models
│   ├── DotNetRunner/            # Test execution
│   ├── Enums/                   # Enumerations
│   ├── Extensions/              # Utility extensions
│   ├── Logging/                 # Console output
│   ├── RerunCommand/            # Main command logic
│   └── Program.cs               # Entry point
├── test/                        # Test projects
│   ├── dotnet-test-rerun.Common/
│   ├── dotnet-test-rerun.UnitTests/
│   └── dotnet-test-rerun.IntegrationTests/
└── docs/                        # Documentation
```

### Key Components

- **RerunCommand**: Main command logic, retry orchestration
- **DotNetTestRunner**: Executes `dotnet test` and captures results
- **TestResultsAnalyzer**: Parses TRX files to identify failed tests
- **TestFilter**: Builds test filters for rerunning specific tests
- **Logger**: Handles console output with Spectre.Console

### Adding New Options

To add a new command-line option:

1. Add the option to `RerunCommandConfiguration.cs`:
   ```csharp
   public Option<T> NewOption { get; set; }
   ```

2. Configure it in `RerunCommand.cs` constructor:
   ```csharp
   NewOption = new Option<T>(
       name: "--new-option",
       description: "Description of the option");
   AddOption(NewOption);
   ```

3. Use it in the command handler:
   ```csharp
   var newValue = commandResult.GetValueForOption(NewOption);
   ```

4. Add tests for the new option
5. Update documentation

## Coding Standards

### General Guidelines

- Follow SOLID principles
- Write testable code
- Use dependency injection
- Avoid static state
- Handle errors appropriately

### Documentation

- Add XML comments for public APIs
- Update README.md for user-facing changes
- Add examples for new features
- Keep documentation clear and concise

### Dependencies

- Minimize external dependencies
- Keep dependencies up to date
- Document why each dependency is needed

## Release Process

Releases are managed by maintainers:

1. Version is bumped using Versionize
2. CHANGELOG.md is automatically updated
3. Git tag is created
4. NuGet package is published
5. Docker images are built and pushed

Contributors don't need to worry about releases, but should follow semantic versioning guidelines in commit messages.

## Community Guidelines

- Be respectful and inclusive
- Welcome newcomers
- Provide constructive feedback
- Ask questions if unclear
- Help others when possible

## Getting Help

- 🐛 [GitHub Issues](https://github.com/joaoopereira/dotnet-test-rerun/issues) - Report bugs, request features
- 📧 Contact maintainers via GitHub

## Recognition

Contributors will be:
- Listed in the repository's contributors section
- Mentioned in release notes (for significant contributions)
- Credited in the README

## License

By contributing, you agree that your contributions will be licensed under the GNU General Public License v3.0.

Thank you for contributing to dotnet-test-rerun! 🎉
