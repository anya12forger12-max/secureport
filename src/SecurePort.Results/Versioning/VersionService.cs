using System.Reflection;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Results.Versioning;

/// <summary>
/// Manages application versioning and schema migration.
/// </summary>
public sealed class VersionService : IVersionService
{
    private const int CurrentSchemaVersion = 1;
    private readonly IReadOnlyList<ISchemaMigrator> _migrators;

    public VersionService(IEnumerable<ISchemaMigrator>? migrators = null)
    {
        _migrators = migrators?.ToList() ?? (IReadOnlyList<ISchemaMigrator>)Array.Empty<ISchemaMigrator>();
    }

    public VersionInfo GetCurrentVersion()
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version ?? new Version(1, 0, 0);

        return new VersionInfo
        {
            Major = version.Major,
            Minor = version.Minor,
            Patch = version.Build > 0 ? version.Build : 0,
            FullVersionString = version.ToString(3),
            BuildDate = File.GetLastWriteTimeUtc(assembly.Location),
            GitCommitHash = GetGitCommitHash(),
            GitBranch = GetGitBranch()
        };
    }

    public Task<VersionInfo?> CheckForUpdatesAsync(CancellationToken ct = default)
    {
        // Offline-only application — no remote update checking.
        return Task.FromResult<VersionInfo?>(null);
    }

    public IReadOnlyList<int> GetKnownSchemaVersions()
    {
        var versions = new List<int> { CurrentSchemaVersion };
        versions.AddRange(_migrators.Select(m => m.TargetVersion));
        return versions.Distinct().OrderBy(v => v).ToList();
    }

    public int GetCurrentSchemaVersion() => CurrentSchemaVersion;

    public bool NeedsMigration(int fromSchemaVersion) => fromSchemaVersion < CurrentSchemaVersion;

    private static string? GetGitCommitHash()
    {
        try
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var attrs = assembly.GetCustomAttributes<AssemblyMetadataAttribute>();
            foreach (var attr in attrs)
            {
                if (attr.Key == "GitCommitHash")
                    return attr.Value;
            }
        }
        catch
        {
            // Best effort
        }
        return null;
    }

    private static string? GetGitBranch()
    {
        try
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var attrs = assembly.GetCustomAttributes<AssemblyMetadataAttribute>();
            foreach (var attr in attrs)
            {
                if (attr.Key == "GitBranch")
                    return attr.Value;
            }
        }
        catch
        {
            // Best effort
        }
        return null;
    }
}
