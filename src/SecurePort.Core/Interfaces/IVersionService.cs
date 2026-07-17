using SecurePort.Core.Models;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Manages application versioning and migration.
/// </summary>
public interface IVersionService
{
    /// <summary>Gets the current application version.</summary>
    VersionInfo GetCurrentVersion();

    /// <summary>Checks if a newer version is available.</summary>
    Task<VersionInfo?> CheckForUpdatesAsync(CancellationToken ct = default);

    /// <summary>Returns the list of known configuration schema versions.</summary>
    IReadOnlyList<int> GetKnownSchemaVersions();

    /// <summary>Returns the current configuration schema version.</summary>
    int GetCurrentSchemaVersion();

    /// <summary>Determines if a configuration migration is needed.</summary>
    bool NeedsMigration(int fromSchemaVersion);
}

/// <summary>
/// Migrates data between schema versions.
/// </summary>
public interface ISchemaMigrator
{
    /// <summary>Returns the source schema version this migrator handles.</summary>
    int SourceVersion { get; }

    /// <summary>Returns the target schema version this migrator produces.</summary>
    int TargetVersion { get; }

    /// <summary>Migrates the given JSON configuration data.</summary>
    Task<string> MigrateAsync(string jsonData, CancellationToken ct = default);
}
