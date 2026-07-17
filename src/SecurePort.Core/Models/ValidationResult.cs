namespace SecurePort.Core.Models;

/// <summary>
/// Represents the result of a validation operation.
/// </summary>
public sealed record ValidationResult
{
    /// <summary>Whether the validation passed.</summary>
    public required bool IsValid { get; init; }

    /// <summary>Machine-readable error code, or null when valid.</summary>
    public string? ErrorCode { get; init; }

    /// <summary>Human-readable error description, or null when valid.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Suggested fix for the validation failure, or null.</summary>
    public string? SuggestedFix { get; init; }

    /// <summary>The category of validation that was performed.</summary>
    public required string ValidationCategory { get; init; }

    /// <summary>When the validation was performed.</summary>
    public DateTimeOffset ValidatedAt { get; init; } = DateTimeOffset.UtcNow;

    public static ValidationResult Valid(string category) => new()
    {
        IsValid = true,
        ValidationCategory = category
    };

    public static ValidationResult Invalid(string category, string errorCode, string errorMessage, string? suggestedFix = null) => new()
    {
        IsValid = false,
        ErrorCode = errorCode,
        ErrorMessage = errorMessage,
        SuggestedFix = suggestedFix,
        ValidationCategory = category
    };
}

/// <summary>
/// Aggregated validation result containing multiple individual results.
/// </summary>
public sealed record ValidationReport
{
    public required IReadOnlyList<ValidationResult> Results { get; init; }
    public required bool AllValid { get; init; }
    public int ErrorCount => Results.Count(r => !r.IsValid);
    public int WarningCount => Results.Count(r => r.IsValid && r.ErrorCode != null);

    public static ValidationReport FromResults(IReadOnlyList<ValidationResult> results) => new()
    {
        Results = results,
        AllValid = results.All(r => r.IsValid)
    };
}
