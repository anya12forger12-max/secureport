# Security Guide

SecurePort is a defensive-only tool. It scans ports and manages results locally. This guide documents the security measures built into the application.

## Threat Model

| Threat | Mitigation |
|--------|-----------|
| Malicious input (target, ports, filenames) | Input validation and sanitization at every entry point |
| Path traversal in file operations | `Path.GetFullPath` validation, sandboxed directories |
| Data exposure at rest | AES-256-GCM encryption for sensitive data |
| Backup tampering | SHA-256 integrity verification on restore |
| Data persistence after deletion | Secure overwrite before file removal |
| Secrets in logs | No secrets, keys, or credentials logged |
| Supply chain (NuGet) | Pinned versions, minimal dependency count |
| Network exfiltration | No outbound connections except requested scans |

## Input Validation

All user-supplied input is validated before use:

- **Scan targets** — validated as IP addresses or resolvable hostnames. No shell metacharacters, path separators, or control characters are permitted.
- **Port numbers** — parsed as integers, clamped to 1–65535. Non-numeric input is rejected.
- **File paths** — all file paths (exports, backups, data directory) are validated against a whitelist of allowed base directories.
- **Tags and notes** — sanitized to remove control characters. Length limits are enforced.

Validation is performed in the Security layer before any downstream processing.

## File Handling Security

### Path Traversal Prevention

Every file path in the application passes through `SecurityValidator.ValidatePath()`:

```csharp
public static bool ValidatePath(string path, string allowedBase)
{
    var fullPath = Path.GetFullPath(path);
    var baseDir = Path.GetFullPath(allowedBase);
    return fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase);
}
```

Paths that escape the allowed base directory are rejected.

### File Extension Whitelist

Only approved file extensions are permitted for read and write operations:

- `.json` — scan data and configuration
- `.csv`, `.txt`, `.html` — exported reports
- `.spbackup` — backup archives

### Temporary Files

Temporary files are created in a secure directory and deleted immediately after use. They are never stored in user-visible locations.

## Encryption

### AES-256-GCM

Sensitive data at rest is encrypted using AES-256-GCM (Galois/Counter Mode):

- **Key size:** 256 bits
- **Nonce:** Unique 96-bit nonce per encryption operation
- **Authentication:** GCM provides authenticated encryption — any tampering is detected on decryption

Key material is derived using PBKDF2 with a user-configured passphrase or a machine-bound key. Raw keys are never stored on disk.

### Key Management

- Encryption keys are derived at runtime, not persisted.
- The passphrase (if used) is never logged or written to configuration files.
- Key rotation is supported through re-encryption of stored data.

## Backup Integrity

Each backup file includes a SHA-256 checksum:

1. **On creation:** the backup payload is hashed before writing.
2. **On restore:** the checksum is recomputed and compared.
3. **On mismatch:** restore is refused with a clear error.

The checksum covers all data in the backup except the checksum field itself.

## Configuration Protection

- Configuration files are stored in the application's data directory, not in world-writable locations.
- Sensitive configuration values (encryption passphrase) are never stored in configuration files.
- File permissions restrict access to the current user on Linux and macOS.
- On Windows, standard user-level ACLs are applied.

## Secure Deletion

When the user deletes data (results, backups, or all data):

1. The file is overwritten with random data.
2. The file is overwritten with zeros.
3. The file is deleted from the filesystem.

This makes recovery by conventional file recovery tools impractical.

## Logging Considerations

The internal logging system (`SecureLogger`) is designed to prevent sensitive data leakage:

- **Never logged:** encryption keys, passphrases, user credentials, full file paths containing sensitive data.
- **Logged:** operation success/failure, timing, error categories (not raw error messages containing paths).
- **Log level control:** users can set log verbosity to minimize data capture.
- **Log rotation:** old logs are rotated and securely deleted.

### Log Output

Logs are written to the application's local data directory. They are never transmitted off-device.

## Defensive-Only Operation

SecurePort is strictly a defensive tool:

- It connects only to the target hosts and ports the user specifies.
- It does not exploit vulnerabilities, send payloads, or modify remote systems.
- It does not scan the local machine unless the user explicitly targets `127.0.0.1` or `localhost`.
- It does not make any outbound connections beyond the requested TCP probes.

No data leaves the user's machine except the TCP SYN packets required for port scanning.
