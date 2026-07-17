namespace SecurePort.Core.Models;

/// <summary>
/// Health status levels for application subsystems.
/// </summary>
public enum HealthLevel
{
    Healthy,
    Warning,
    Critical,
    Unknown
}

/// <summary>
/// Represents the health status of a single subsystem.
/// </summary>
public sealed record SubsystemHealth
{
    public required string SubsystemName { get; init; }
    public required HealthLevel Status { get; init; }
    public string? Message { get; init; }
    public string? RecoverySuggestion { get; init; }
    public DateTimeOffset CheckedAt { get; init; } = DateTimeOffset.UtcNow;
    public TimeSpan? ResponseTime { get; init; }
    public IDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}

/// <summary>
/// Comprehensive health report for the entire application.
/// </summary>
public sealed record HealthReport
{
    public required HealthLevel OverallStatus { get; init; }
    public required IReadOnlyList<SubsystemHealth> Subsystems { get; init; }
    public DateTimeOffset GeneratedAt { get; init; } = DateTimeOffset.UtcNow;
    public string? Summary { get; init; }

    public int HealthyCount => Subsystems.Count(s => s.Status == HealthLevel.Healthy);
    public int WarningCount => Subsystems.Count(s => s.Status == HealthLevel.Warning);
    public int CriticalCount => Subsystems.Count(s => s.Status == HealthLevel.Critical);
}
