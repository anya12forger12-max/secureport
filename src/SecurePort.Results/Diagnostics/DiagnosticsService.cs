using System.Diagnostics;
using System.Runtime.InteropServices;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Results.Diagnostics;

/// <summary>
/// Provides application diagnostic information.
/// </summary>
public sealed class DiagnosticsService : IDiagnosticsService
{
    private readonly IHealthMonitor? _healthMonitor;

    public DiagnosticsService(IHealthMonitor? healthMonitor = null)
    {
        _healthMonitor = healthMonitor;
    }

    public async Task<DiagnosticInfo> CollectDiagnosticsAsync(CancellationToken ct = default)
    {
        var memoryUsage = GetMemoryUsage();
        var threadPoolInfo = GetThreadPoolInfo();

        HealthReport? healthReport = null;
        try
        {
            healthReport = _healthMonitor != null
                ? await _healthMonitor.CheckHealthAsync(ct)
                : null;
        }
        catch
        {
            // Health check failure should not prevent diagnostics collection
        }

        return new DiagnosticInfo
        {
            ApplicationVersion = GetVersionInfo().SemVer,
            OperatingSystem = RuntimeInformation.OSDescription,
            RuntimeVersion = RuntimeInformation.FrameworkDescription,
            Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            WorkingSetBytes = memoryUsage.WorkingSetBytes,
            ThreadPoolWorkerThreads = threadPoolInfo.MaxWorkerThreads,
            ThreadPoolCompletionPortThreads = threadPoolInfo.MaxCompletionPortThreads,
            ActiveWorkerThreads = threadPoolInfo.MaxWorkerThreads - threadPoolInfo.AvailableWorkerThreads,
            ActiveCompletionPortThreads = threadPoolInfo.MaxCompletionPortThreads - threadPoolInfo.AvailableCompletionPortThreads,
            GcTotalMemoryBytes = memoryUsage.GcTotalMemoryBytes,
            Gen0Collections = memoryUsage.Gen0Collections,
            Gen1Collections = memoryUsage.Gen1Collections,
            Gen2Collections = memoryUsage.Gen2Collections,
            HealthReport = healthReport,
            InstalledPlugins = GetInstalledPlugins(),
            RecentErrors = Array.Empty<DiagnosticError>()
        };
    }

    public VersionInfo GetVersionInfo()
    {
        var assembly = System.Reflection.Assembly.GetEntryAssembly() ?? System.Reflection.Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version ?? new Version(1, 0, 0);

        return new VersionInfo
        {
            Major = version.Major,
            Minor = version.Minor,
            Patch = version.Build > 0 ? version.Build : 0,
            FullVersionString = assembly.GetName().Version?.ToString() ?? "1.0.0",
            BuildDate = File.GetLastWriteTimeUtc(assembly.Location)
        };
    }

    public MemoryUsage GetMemoryUsage()
    {
        var process = Process.GetCurrentProcess();
        return new MemoryUsage
        {
            WorkingSetBytes = process.WorkingSet64,
            PrivateMemoryBytes = process.PrivateMemorySize64,
            GcTotalMemoryBytes = GC.GetTotalMemory(forceFullCollection: false),
            Gen0Collections = GC.CollectionCount(0),
            Gen1Collections = GC.CollectionCount(1),
            Gen2Collections = GC.CollectionCount(2)
        };
    }

    public ThreadPoolInfo GetThreadPoolInfo()
    {
        ThreadPool.GetAvailableThreads(out var workerAvail, out var portAvail);
        ThreadPool.GetMaxThreads(out var workerMax, out var portMax);
        ThreadPool.GetMinThreads(out var workerMin, out var portMin);

        return new ThreadPoolInfo
        {
            MinWorkerThreads = workerMin,
            MaxWorkerThreads = workerMax,
            AvailableWorkerThreads = workerAvail,
            MinCompletionPortThreads = portMin,
            MaxCompletionPortThreads = portMax,
            AvailableCompletionPortThreads = portAvail
        };
    }

    public async Task<HealthReport> GetHealthSummaryAsync(CancellationToken ct = default)
    {
        if (_healthMonitor != null)
            return await _healthMonitor.CheckHealthAsync(ct);

        return new HealthReport
        {
            OverallStatus = HealthLevel.Unknown,
            Subsystems = Array.Empty<SubsystemHealth>(),
            Summary = "Health monitor not available."
        };
    }

    private static IReadOnlyList<string> GetInstalledPlugins()
    {
        return Array.Empty<string>();
    }
}
