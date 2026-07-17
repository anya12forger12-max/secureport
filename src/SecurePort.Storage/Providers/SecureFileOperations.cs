using System.Security.Cryptography;
using System.Text;
using SecurePort.Core.Interfaces;

namespace SecurePort.Storage.Providers;

/// <summary>
/// Provides secure file operations with validation, atomic writes, and error recovery.
/// </summary>
public sealed class SecureFileOperations : ISecureFileOperations
{
    private static readonly HashSet<string> s_blockedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/etc", "/boot", "/sys", "/proc", "/dev",
        "C:\\Windows", "C:\\System32"
    };

    public async Task SafeWriteAllBytesAsync(string filePath, byte[] data, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (!IsPathSafe(filePath))
            throw new ArgumentException($"Path is not safe: {filePath}", nameof(filePath));

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var tempPath = GetTempFilePath(filePath);
        try
        {
            await File.WriteAllBytesAsync(tempPath, data, ct);
            if (File.Exists(filePath))
                File.Delete(filePath);
            File.Move(tempPath, filePath);
        }
        catch
        {
            TryCleanup(tempPath);
            throw;
        }
    }

    public async Task SafeWriteAllTextAsync(string filePath, string text, CancellationToken ct = default)
    {
        if (!IsPathSafe(filePath))
            throw new ArgumentException($"Path is not safe: {filePath}", nameof(filePath));

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var tempPath = GetTempFilePath(filePath);
        try
        {
            await File.WriteAllTextAsync(tempPath, text, Encoding.UTF8, ct);
            if (File.Exists(filePath))
                File.Delete(filePath);
            File.Move(tempPath, filePath);
        }
        catch
        {
            TryCleanup(tempPath);
            throw;
        }
    }

    public async Task<byte[]?> SafeReadAllBytesAsync(string filePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return null;

        try
        {
            return await File.ReadAllBytesAsync(filePath, ct);
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    public async Task<string?> SafeReadAllTextAsync(string filePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return null;

        try
        {
            return await File.ReadAllTextAsync(filePath, ct);
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    public bool SafeDelete(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return false;

        try
        {
            File.Delete(filePath);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task SecureDeleteAsync(string filePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return;

        try
        {
            var fileInfo = new FileInfo(filePath);
            var length = (int)Math.Min(fileInfo.Length, int.MaxValue);
            if (length > 0)
            {
                var randomBytes = new byte[length];
                RandomNumberGenerator.Fill(randomBytes);
                await File.WriteAllBytesAsync(filePath, randomBytes, ct);
            }
            File.Delete(filePath);
        }
        catch
        {
            TryCleanup(filePath);
        }
    }

    public bool IsPathSafe(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        var fullPath = Path.GetFullPath(filePath);

        if (fullPath.Contains(".."))
            return false;

        foreach (var blocked in s_blockedPaths)
        {
            if (fullPath.StartsWith(blocked, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    public string GetTempFilePath(string originalFilePath)
    {
        var directory = Path.GetDirectoryName(originalFilePath) ?? Path.GetTempPath();
        var fileName = Path.GetFileName(originalFilePath);
        return Path.Combine(directory, $".{fileName}.tmp.{Guid.NewGuid():N}");
    }

    private static void TryCleanup(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
        catch
        {
            // Best effort cleanup
        }
    }
}
