using SecurePort.Core.Models;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Provides failure recovery capabilities for application subsystems.
/// </summary>
public interface IFailureRecovery
{
    /// <summary>Attempts to recover from configuration corruption.</summary>
    Task<RecoveryResult> RecoverConfigurationAsync(CancellationToken ct = default);

    /// <summary>Attempts to recover from history data corruption.</summary>
    Task<RecoveryResult> RecoverHistoryAsync(CancellationToken ct = default);

    /// <summary>Attempts to recover from interrupted report generation.</summary>
    Task<RecoveryResult> RecoverInterruptedReportAsync(CancellationToken ct = default);

    /// <summary>Performs a full recovery scan across all subsystems.</summary>
    Task<RecoverySummary> RunFullRecoveryAsync(CancellationToken ct = default);

    /// <summary>Creates a backup before recovery operations.</summary>
    Task<string> CreateRecoveryBackupAsync(CancellationToken ct = default);
}
