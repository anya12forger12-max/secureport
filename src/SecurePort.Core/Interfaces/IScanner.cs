using SecurePort.Core.Models;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Defines the contract for executing network port scans.
/// </summary>
public interface IScanner
{
    /// <summary>
    /// Gets an observable stream of progress updates for the current scan.
    /// </summary>
    IObservable<ScanProgress> ProgressChanged { get; }

    /// <summary>
    /// Gets a value indicating whether a scan is currently in progress.
    /// </summary>
    bool IsScanning { get; }

    /// <summary>
    /// Starts an asynchronous port scan against the specified target.
    /// </summary>
    /// <param name="target">The scan target configuration.</param>
    /// <param name="ct">A cancellation token to stop the scan.</param>
    /// <returns>The completed scan session containing all results.</returns>
    Task<ScanSession> StartScanAsync(ScanTarget target, CancellationToken ct);

    /// <summary>
    /// Cancels the currently running scan.
    /// </summary>
    /// <returns>A task that completes when the scan has been cancelled.</returns>
    Task CancelScanAsync();
}
