namespace SecurePort.Core.Interfaces;

/// <summary>
/// Provides secure file operations with validation and error recovery.
/// </summary>
public interface ISecureFileOperations
{
    /// <summary>Safely writes data to a file with atomic replacement.</summary>
    Task SafeWriteAllBytesAsync(string filePath, byte[] data, CancellationToken ct = default);

    /// <summary>Safely writes text to a file with atomic replacement.</summary>
    Task SafeWriteAllTextAsync(string filePath, string text, CancellationToken ct = default);

    /// <summary>Safely reads all bytes from a file with error recovery.</summary>
    Task<byte[]?> SafeReadAllBytesAsync(string filePath, CancellationToken ct = default);

    /// <summary>Safely reads all text from a file with error recovery.</summary>
    Task<string?> SafeReadAllTextAsync(string filePath, CancellationToken ct = default);

    /// <summary>Safely deletes a file, returning success status.</summary>
    bool SafeDelete(string filePath);

    /// <summary>Securely overwrites a file before deletion.</summary>
    Task SecureDeleteAsync(string filePath, CancellationToken ct = default);

    /// <summary>Validates that a file path is safe (no path traversal, etc.).</summary>
    bool IsPathSafe(string filePath);

    /// <summary>Creates a temporary file for atomic write operations.</summary>
    string GetTempFilePath(string originalFilePath);
}
