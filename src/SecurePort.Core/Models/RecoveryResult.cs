namespace SecurePort.Core.Models;

/// <summary>
/// Result of a failure recovery operation.
/// </summary>
public sealed record RecoveryResult
{
    public required bool Success { get; init; }
    public required string Component { get; init; }
    public required string RecoveryAction { get; init; }
    public string? Details { get; init; }
    public bool DataLoss { get; init; }
    public DateTimeOffset RecoveredAt { get; init; } = DateTimeOffset.UtcNow;
    public IReadOnlyList<string>? RecoverySteps { get; init; }
}

/// <summary>
/// Summary of all recovery operations performed during a recovery session.
/// </summary>
public sealed record RecoverySummary
{
    public required IReadOnlyList<RecoveryResult> Results { get; init; }
    public bool AllSucceeded => Results.All(r => r.Success);
    public bool AnyDataLoss => Results.Any(r => r.DataLoss);
    public int SucceededCount => Results.Count(r => r.Success);
    public int FailedCount => Results.Count(r => !r.Success);
    public DateTimeOffset ExecutedAt { get; init; } = DateTimeOffset.UtcNow;
}
