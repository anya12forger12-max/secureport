using SecurePort.Core.Models;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Verifies data integrity using SHA-256 checksums.
/// </summary>
public interface IReportVerificationManager
{
    /// <summary>Computes the SHA-256 checksum of a file.</summary>
    string ComputeChecksum(byte[] data);

    /// <summary>Computes the SHA-256 checksum of a file at the specified path.</summary>
    Task<string> ComputeFileChecksumAsync(string filePath, CancellationToken ct);

    /// <summary>Verifies a file against its expected checksum.</summary>
    Task<IntegrityCheckResult> VerifyFileAsync(string filePath, string expectedChecksum, CancellationToken ct);

    /// <summary>Verifies a report's integrity using its stored metadata.</summary>
    Task<IntegrityCheckResult> VerifyReportAsync(ReportMetadata report, CancellationToken ct);

    /// <summary>Verifies a backup's integrity using its stored metadata.</summary>
    Task<IntegrityCheckResult> VerifyBackupAsync(BackupMetadata backup, CancellationToken ct);
}
