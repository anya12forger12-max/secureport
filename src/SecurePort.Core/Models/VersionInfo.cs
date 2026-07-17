namespace SecurePort.Core.Models;

/// <summary>
/// Semantic version information for the application.
/// </summary>
public sealed record VersionInfo
{
    public required int Major { get; init; }
    public required int Minor { get; init; }
    public required int Patch { get; init; }
    public string? PreReleaseTag { get; init; }
    public string? BuildMetadata { get; init; }
    public string? FullVersionString { get; init; }
    public DateTimeOffset BuildDate { get; init; }
    public string? GitCommitHash { get; init; }
    public string? GitBranch { get; init; }

    /// <summary>Gets the SemVer 2.0 version string without pre-release or build metadata.</summary>
    public string CoreVersion => $"{Major}.{Minor}.{Patch}";

    /// <summary>Gets the full SemVer 2.0 version string.</summary>
    public string SemVer => string.IsNullOrEmpty(PreReleaseTag)
        ? CoreVersion
        : $"{CoreVersion}-{PreReleaseTag}";

    /// <summary>Determines if this version is newer than the specified version.</summary>
    public bool IsNewerThan(VersionInfo other)
    {
        if (Major != other.Major) return Major > other.Major;
        if (Minor != other.Minor) return Minor > other.Minor;
        return Patch > other.Patch;
    }

    /// <summary>Determines the type of update relative to the specified version.</summary>
    public UpdateType GetUpdateType(VersionInfo other)
    {
        if (Major > other.Major) return UpdateType.Major;
        if (Minor > other.Minor) return UpdateType.Minor;
        if (Patch > other.Patch) return UpdateType.Patch;
        return UpdateType.None;
    }
}

public enum UpdateType
{
    None,
    Patch,
    Minor,
    Major
}
