using SecurePort.Core.Models;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Provides application diagnostic information.
/// </summary>
public interface IDiagnosticsService
{
    /// <summary>Collects a complete diagnostic snapshot.</summary>
    Task<DiagnosticInfo> CollectDiagnosticsAsync(CancellationToken ct = default);

    /// <summary>Returns the current application version information.</summary>
    VersionInfo GetVersionInfo();

    /// <summary>Returns current memory usage statistics.</summary>
    MemoryUsage GetMemoryUsage();

    /// <summary>Returns current thread pool statistics.</summary>
    ThreadPoolInfo GetThreadPoolInfo();

    /// <summary>Returns a health summary.</summary>
    Task<HealthReport> GetHealthSummaryAsync(CancellationToken ct = default);
}

/// <summary>
/// Memory usage statistics.
/// </summary>
public sealed record MemoryUsage
{
    public long WorkingSetBytes { get; init; }
    public long PrivateMemoryBytes { get; init; }
    public long GcTotalMemoryBytes { get; init; }
    public int Gen0Collections { get; init; }
    public int Gen1Collections { get; init; }
    public int Gen2Collections { get; init; }
    public DateTimeOffset CollectedAt { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Thread pool statistics.
/// </summary>
public sealed record ThreadPoolInfo
{
    public int MinWorkerThreads { get; init; }
    public int MaxWorkerThreads { get; init; }
    public int AvailableWorkerThreads { get; init; }
    public int MinCompletionPortThreads { get; init; }
    public int MaxCompletionPortThreads { get; init; }
    public int AvailableCompletionPortThreads { get; init; }
    public int PendingWorkItems { get; init; }
}
