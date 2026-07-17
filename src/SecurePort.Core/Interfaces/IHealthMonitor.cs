using SecurePort.Core.Models;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Monitors the health of application subsystems.
/// </summary>
public interface IHealthMonitor
{
    /// <summary>Checks the health of all subsystems and returns a comprehensive report.</summary>
    Task<HealthReport> CheckHealthAsync(CancellationToken ct = default);

    /// <summary>Checks the health of a specific subsystem.</summary>
    Task<SubsystemHealth> CheckSubsystemAsync(string subsystemName, CancellationToken ct = default);

    /// <summary>Returns the names of all registered subsystems.</summary>
    IReadOnlyList<string> GetRegisteredSubsystems();
}

/// <summary>
/// Monitors application health with periodic checks.
/// </summary>
public interface IPeriodicHealthMonitor : IHealthMonitor
{
    /// <summary>Starts periodic health monitoring with the specified interval.</summary>
    void StartMonitoring(TimeSpan interval);

    /// <summary>Stops periodic health monitoring.</summary>
    void StopMonitoring();

    /// <summary>Raised when a subsystem status changes.</summary>
    event EventHandler<SubsystemHealth>? HealthChanged;
}
