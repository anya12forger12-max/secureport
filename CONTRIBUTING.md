# Contributing to SecurePort

Thank you for your interest in contributing to SecurePort! This project exists because of contributions from people like you.

## Code of Conduct

This project follows the [Contributor Covenant v2.1](CODE_OF_CONDUCT.md). By participating, you agree to uphold its standards.

## How to Contribute

### Reporting Bugs

1. Search existing [issues](https://github.com/yourorg/SecurePort/issues) to avoid duplicates
2. Use the **Bug Report** template when creating a new issue
3. Include your OS, .NET version, and steps to reproduce

### Suggesting Features

1. Search existing [issues](https://github.com/yourorg/SecurePort/issues) first
2. Use the **Feature Request** template
3. Describe the problem, not just the solution

### Submitting Code

1. **Fork** the repository
2. **Clone** your fork locally:
   ```bash
   git clone https://github.com/YOUR_USERNAME/SecurePort.git
   cd SecurePort
   ```
3. **Create a branch** from `main`:
   ```bash
   git checkout -b feature/your-feature-name
   ```
4. **Make your changes**
5. **Write or update tests** for your changes
6. **Run the test suite** and verify it passes
7. **Commit** with a clear message
8. **Push** and open a Pull Request

## Development Setup

**Prerequisites:**
- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- Git
- A code editor (VS Code, Rider, or Visual Studio recommended)

**Getting started:**
```bash
git clone https://github.com/yourorg/SecurePort.git
cd SecurePort
dotnet restore
dotnet build
dotnet test
```

**Run the application:**
```bash
dotnet run --project src/SecurePort.UI
```

**Run tests:**
```bash
dotnet test
```

## Code Style

Follow the existing code conventions:

- **Target framework:** .NET 6.0
- **UI framework:** Avalonia 11.0.10
- **MVVM:** CommunityToolkit.Mvvm for observable properties and commands
- **File-scoped namespaces** (`namespace Foo;`)
- **Nullable reference types** enabled
- **Implicit usings** enabled
- **No comments in code** unless explicitly requested by reviewer
- **No trailing whitespace** or unnecessary blank lines
- **Use `var`** when the type is obvious from the right side

### Naming Conventions

| Element | Convention | Example |
|---------|-----------|---------|
| Classes | PascalCase | `ScanEngine` |
| Methods | PascalCase | `StartScan` |
| Properties | PascalCase | `IsRunning` |
| Private fields | `_camelCase` | `_scanTimeout` |
| Local variables | camelCase | `portNumber` |
| Parameters | camelCase | `maxConcurrent` |

### Project Structure

Follow the Clean Architecture layer separation:

- `SecurePort.Core` — Domain models, interfaces, scanning logic
- `SecurePort.Security` — Encryption, validation, logging
- `SecurePort.Storage` — File I/O, persistence, backups
- `SecurePort.Results` — Results management, export
- `SecurePort.UI` — Avalonia views, view models, converters

Dependencies flow inward only. Core has no outer-layer dependencies.

## Testing

- Write unit tests for all new functionality
- Place tests in the corresponding `*.Tests` project
- Tests must pass before PR submission: `dotnet test`
- Aim for meaningful coverage, not arbitrary percentages
- Test edge cases: empty inputs, boundary values, error conditions

## Pull Request Process

1. Fill out the PR template completely
2. Reference any related issues (e.g., `Closes #42`)
3. Ensure all CI checks pass
4. Request a review from a maintainer
5. Address review feedback promptly
6. Squash commits if requested before merge

### What We Look For

- Clear problem statement in the PR description
- Focused changes (one logical change per PR)
- Tests that cover the changes
- No unrelated modifications
- Follows existing code style
- No new warnings or errors in the build

## Issue Reporting

Use the appropriate issue template:

- **Bug Report** — For defects with reproduction steps
- **Feature Request** — For new functionality proposals

Include:
- Clear, descriptive title
- OS and .NET version
- Steps to reproduce (for bugs)
- Expected vs actual behavior (for bugs)
- Screenshots if applicable

## Questions?

Open a discussion or check the [docs/](docs/) directory for architecture and developer guides.
