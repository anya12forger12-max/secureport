namespace SecurePort.Core.Enums;

/// <summary>
/// Represents the current status of a scan session.
/// </summary>
public enum ScanStatus
{
    /// <summary>
    /// The scan has been created but not yet started.
    /// </summary>
    Pending,

    /// <summary>
    /// The scan is currently in progress.
    /// </summary>
    Running,

    /// <summary>
    /// The scan has been temporarily paused.
    /// </summary>
    Paused,

    /// <summary>
    /// The scan has finished successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// The scan terminated due to an error.
    /// </summary>
    Failed,

    /// <summary>
    /// The scan was cancelled by the user.
    /// </summary>
    Cancelled
}
