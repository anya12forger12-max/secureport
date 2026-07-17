using System.Security.Cryptography;
using System.Text;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Results.Managers;

/// <summary>
/// Verifies data integrity using SHA-256 checksums for reports and backups.
/// </summary>
public sealed class ReportVerificationManager : IReportVerificationManager
{
    public string ComputeChecksum(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var hash = SHA256.HashData(data);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public async Task<string> ComputeFileChecksumAsync(string filePath, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
        using var sha = SHA256.Create();
        var hash = await sha.ComputeHashAsync(stream, ct);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public async Task<IntegrityCheckResult> VerifyFileAsync(string filePath, string expectedChecksum, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        if (string.IsNullOrWhiteSpace(expectedChecksum))
            throw new ArgumentException("Expected checksum cannot be null or empty.", nameof(expectedChecksum));

        if (!File.Exists(filePath))
        {
            return new IntegrityCheckResult
            {
                ResourceId = filePath,
                ExpectedChecksum = expectedChecksum,
                ComputedChecksum = string.Empty,
                IsValid = false,
                VerifiedAt = DateTimeOffset.UtcNow,
                ErrorMessage = "File not found."
            };
        }

        try
        {
            var computed = await ComputeFileChecksumAsync(filePath, ct);
            var isValid = string.Equals(computed, expectedChecksum, StringComparison.OrdinalIgnoreCase);

            return new IntegrityCheckResult
            {
                ResourceId = filePath,
                ExpectedChecksum = expectedChecksum,
                ComputedChecksum = computed,
                IsValid = isValid,
                VerifiedAt = DateTimeOffset.UtcNow,
                ErrorMessage = isValid ? null : "Checksum mismatch. Data may have been corrupted or tampered with."
            };
        }
        catch (Exception ex)
        {
            return new IntegrityCheckResult
            {
                ResourceId = filePath,
                ExpectedChecksum = expectedChecksum,
                ComputedChecksum = string.Empty,
                IsValid = false,
                VerifiedAt = DateTimeOffset.UtcNow,
                ErrorMessage = $"Verification failed: {ex.Message}"
            };
        }
    }

    public Task<IntegrityCheckResult> VerifyReportAsync(ReportMetadata report, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(report);

        return VerifyFileAsync(report.FilePath, report.Checksum, ct);
    }

    public Task<IntegrityCheckResult> VerifyBackupAsync(BackupMetadata backup, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(backup);

        return VerifyFileAsync(backup.FilePath, backup.Checksum, ct);
    }
}
