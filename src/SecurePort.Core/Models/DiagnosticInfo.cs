namespace SecurePort.Core.Models;

/// <summary>
/// Application diagnostic information snapshot.
/// </summary>
public sealed record DiagnosticInfo
{
    public required string ApplicationVersion { get; init; }
    public required string OperatingSystem { get; init; }
    public required string RuntimeVersion { get; init; }
    public required string Architecture { get; init; }
    public long WorkingSetBytes { get; init; }
    public int ThreadPoolWorkerThreads { get; init; }
    public int ThreadPoolCompletionPortThreads { get; init; }
    public int ActiveWorkerThreads { get; init; }
    public int ActiveCompletionPortThreads { get; init; }
    public long GcTotalMemoryBytes { get; init; }
    public int Gen0Collections { get; init; }
    public int Gen1Collections { get; init; }
    public int Gen2Collections { get; init; }
    public DateTimeOffset CollectedAt { get; init; } = DateTimeOffset.UtcNow;
    public HealthReport? HealthReport { get; init; }
    public StorageUsageInfo? StorageUsage { get; init; }
    public IReadOnlyList<string>? InstalledPlugins { get; init; }
    public IReadOnlyList<DiagnosticError>? RecentErrors { get; init; }
}

/// <summary>
/// A recorded error for diagnostic display.
/// </summary>
public sealed record DiagnosticError
{
    public required DateTimeOffset Timestamp { get; init; }
    public required string Source { get; init; }
    public required string Message { get; init; }
    public string? StackTrace { get; init; }
    public string? Severity { get; init; }
}
