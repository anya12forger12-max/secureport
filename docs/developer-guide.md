# Developer Guide

## Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- IDE with C# support (Visual Studio 2022, Rider, or VS Code with C# extension)
- Git

## Building from Source

```bash
git clone https://github.com/yourorg/SecurePort.git
cd SecurePort
dotnet restore
dotnet build
```

Run the application:

```bash
dotnet run --project src/SecurePort.UI
```

## Running Tests

```bash
dotnet test
```

To run tests for a specific layer:

```bash
dotnet test src/SecurePort.Core.Tests
dotnet test src/SecurePort.Security.Tests
dotnet test src/SecurePort.Storage.Tests
dotnet test src/SecurePort.Results.Tests
dotnet test src/SecurePort.UI.Tests
```

## Code Conventions

### Language Settings

All projects use these C# language features:

- **File-scoped namespaces** — `namespace SecurePort.Core;` (not block-scoped)
- **Nullable reference types** — enabled globally
- **Implicit usings** — enabled globally
- **Global usings** — defined in `GlobalUsings.cs` per project

### MVVM Pattern

The project uses [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/):

```csharp
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _statusText = string.Empty;

    [RelayCommand]
    private async Task StartScanAsync() { /* ... */ }
}
```

- Use `[ObservableProperty]` on private fields for bindable properties.
- Use `[RelayCommand]` on methods for ICommand generation.
- Never call UI APIs directly from ViewModels.

### Naming

- Files: match the primary type name (`ScanEngine.cs`, `PortScanner.cs`)
- Methods: `VerbNounAsync` for async methods
- Private fields: `_camelCase`
- Parameters and locals: `camelCase`
- Public properties: `PascalCase`

### Nullable Annotations

All reference types are nullable by default. Use `!` only with compiler-confirmed non-null paths:

```csharp
public string? ScanTarget { get; set; }

public void Process(string target)
{
    ArgumentNullException.ThrowIfNull(target);
    // target is now confirmed non-null
}
```

## Adding a New Engine or Manager

1. Create the class in the appropriate layer project.
2. Follow the existing pattern (e.g., `ScanEngine`, `ResultsManager`).
3. Accept dependencies via constructor injection.
4. Use `CancellationToken` for all async methods.
5. Add unit tests in the corresponding `.Tests` project.

Example skeleton:

```csharp
namespace SecurePort.Core;

public class NewEngine
{
    private readonly ILogger _logger;

    public NewEngine(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(Input input, CancellationToken ct)
    {
        // implementation
    }
}
```

## Adding a New Validator

Validators live in the layer that owns the data they validate:

1. Create a class implementing the validation pattern (see `FileValidator`, `SecurityValidator`).
2. Return `ValidationResult` with success/failure and messages.
3. Validators must not have side effects (no file writes, no logging).
4. Test both valid and invalid input paths.

```csharp
public class NewValidator
{
    public ValidationResult Validate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return ValidationResult.Failure("Input is required.");

        return ValidationResult.Success();
    }
}
```

## Testing Patterns

- **Unit tests** — test a single class in isolation. Mock dependencies with interfaces.
- **Arrange / Act / Assert** — every test follows this structure.
- **One assertion per concept** — tests may have multiple `Assert` calls but should test one behavior.
- **Test file naming** — `{ClassName}Tests.cs` (e.g., `ScanEngineTests.cs`).
- **Test method naming** — `MethodName_Scenario_ExpectedResult`.

```csharp
[Fact]
public async Task ScanAsync_ValidTarget_ReturnsResults()
{
    // Arrange
    var engine = new ScanEngine(mockLogger);

    // Act
    var results = await engine.ScanAsync("127.0.0.1", CancellationToken.None);

    // Assert
    Assert.NotNull(results);
    Assert.NotEmpty(results);
}
```

### Running Specific Tests

```bash
dotnet test --filter "FullyQualifiedName~ScanEngineTests"
dotnet test --filter "DisplayName~ScanAsync"
```
