namespace SecurePort.Core.Constants;

/// <summary>
/// Security-related constants for cryptographic operations and access control.
/// </summary>
public static class SecurityConstants
{
    /// <summary>
    /// Key size in bits for AES-256 encryption.
    /// </summary>
    public const int EncryptionKeySizeBits = 256;

    /// <summary>
    /// Initialization vector length in bytes for AES-GCM.
    /// </summary>
    public const int IvLengthBytes = 12;

    /// <summary>
    /// Authentication tag length in bytes for AES-GCM.
    /// </summary>
    public const int TagLengthBytes = 16;

    /// <summary>
    /// Salt length in bytes for password-based key derivation.
    /// </summary>
    public const int SaltLengthBytes = 32;

    /// <summary>
    /// Number of PBKDF2 iterations for key derivation.
    /// </summary>
    public const int KeyDerivationIterations = 100_000;

    /// <summary>
    /// The key derivation hash algorithm.
    /// </summary>
    public const string KeyDerivationHashAlgorithm = "SHA-256";

    /// <summary>
    /// Minimum required password length for application access.
    /// </summary>
    public const int MinPasswordLength = 8;

    /// <summary>
    /// Maximum allowed password length.
    /// </summary>
    public const int MaxPasswordLength = 128;

    /// <summary>
    /// Maximum number of failed authentication attempts before lockout.
    /// </summary>
    public const int MaxFailedLoginAttempts = 5;

    /// <summary>
    /// Account lockout duration in minutes.
    /// </summary>
    public const int LockoutDurationMinutes = 15;

    /// <summary>
    /// The name of the protected configuration section.
    /// </summary>
    public const string ProtectedConfigSection = "SecurePort:Security";

    /// <summary>
    /// Prefix used for encrypted storage values.
    /// </summary>
    public const string EncryptedValuePrefix = "ENC:";

    /// <summary>
    /// Maximum age of encryption keys in days before rotation is required.
    /// </summary>
    public const int MaxKeyAgeDays = 90;
}
