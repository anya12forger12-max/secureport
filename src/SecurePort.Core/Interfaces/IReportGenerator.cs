using SecurePort.Core.Models;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Generates formatted reports from scan session data.
/// </summary>
public interface IReportGenerator
{
    /// <summary>
    /// Generates a report for the specified scan session using the given export options.
    /// </summary>
    /// <param name="session">The completed scan session to report on.</param>
    /// <param name="options">The export format and content options.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The file path of the generated report.</returns>
    Task<string> GenerateReportAsync(ScanSession session, ExportOptions options, CancellationToken ct);

    /// <summary>
    /// Generates a report as a byte array without writing to disk.
    /// </summary>
    /// <param name="session">The completed scan session to report on.</param>
    /// <param name="options">The export format and content options.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The report content as a byte array.</returns>
    Task<byte[]> GenerateReportBytesAsync(ScanSession session, ExportOptions options, CancellationToken ct);

    /// <summary>
    /// Gets the file extension for the specified export format.
    /// </summary>
    /// <param name="format">The export format.</param>
    /// <returns>The file extension including the leading dot.</returns>
    string GetFileExtension(Enums.ExportFormat format);
}
