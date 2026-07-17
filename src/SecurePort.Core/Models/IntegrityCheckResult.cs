namespace SecurePort.Core.Models;

/// <summary>
/// Contains integrity verification data for a report or data file.
/// </summary>
public sealed record IntegrityCheckResult
{
    /// <summary>The file or data identifier that was verified.</summary>
    public required string ResourceId { get; init; }

    /// <summary>The expected SHA-256 checksum.</summary>
    public required string ExpectedChecksum { get; init; }

    /// <summary>The computed SHA-256 checksum.</summary>
    public required string ComputedChecksum { get; init; }

    /// <summary>Whether the checksums match.</summary>
    public required bool IsValid { get; init; }

    /// <summary>When the verification was performed.</summary>
    public required DateTimeOffset VerifiedAt { get; init; }

    /// <summary>Optional error message if verification failed.</summary>
    public string? ErrorMessage { get; init; }
}
