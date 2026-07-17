using SecurePort.Core.Models;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Generates computed statistics from scan results.
/// </summary>
public interface IStatisticsEngine
{
    /// <summary>
    /// Computes comprehensive statistics from a collection of scan sessions and their results.
    /// </summary>
    ResultsStatistics ComputeStatistics(
        IReadOnlyList<ScanSession> sessions,
        IReadOnlyList<ResultEntry> results);
}
