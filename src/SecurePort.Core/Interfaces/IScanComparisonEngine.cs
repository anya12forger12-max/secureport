using SecurePort.Core.Models;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Compares two or more completed scan sessions and identifies differences.
/// </summary>
public interface IScanComparisonEngine
{
    /// <summary>
    /// Compares two scan sessions and returns detailed differences.
    /// </summary>
    ScanComparisonResult Compare(ScanSession previous, ScanSession current);

    /// <summary>
    /// Compares multiple scan sessions in chronological order.
    /// </summary>
    IReadOnlyList<ScanComparisonResult> CompareMultiple(IReadOnlyList<ScanSession> sessions);
}
