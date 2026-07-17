namespace SecurePort.Core.Enums;

/// <summary>
/// Categorizes network services by their primary function.
/// </summary>
public enum ServiceCategory
{
    /// <summary>
    /// Web server and HTTP-related services.
    /// </summary>
    Web,

    /// <summary>
    /// Email and mail transfer services.
    /// </summary>
    Mail,

    /// <summary>
    /// Database management system services.
    /// </summary>
    Database,

    /// <summary>
    /// Remote access and administration services.
    /// </summary>
    Remote,

    /// <summary>
    /// File transfer and sharing services.
    /// </summary>
    FileTransfer,

    /// <summary>
    /// Core system and operating system services.
    /// </summary>
    System,

    /// <summary>
    /// Security and authentication services.
    /// </summary>
    Security,

    /// <summary>
    /// Real-time communication and messaging services.
    /// </summary>
    Communication,

    /// <summary>
    /// Online gaming and multiplayer services.
    /// </summary>
    Gaming,

    /// <summary>
    /// User-defined or non-standard services.
    /// </summary>
    Custom,

    /// <summary>
    /// Unknown or unrecognized services.
    /// </summary>
    Unknown
}
