using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SecurePort.Security.Encryption;

namespace SecurePort.Security.Validators
{
    /// <summary>
    /// Provides validation methods for security-sensitive inputs including
    /// encryption keys, file paths, and hostnames.
    /// </summary>
    public static class SecurityValidator
    {
        /// <summary>
        /// Regex pattern matching valid hostname characters per RFC 952 / RFC 1123.
        /// Allows alphanumeric characters, hyphens, and dots. Does not permit leading/trailing hyphens per label.
        /// </summary>
        private static readonly Regex HostnameRegex = new Regex(
            @"^(?!-)[A-Za-z0-9-]{1,63}(?<!-)(\.(?!-)[A-Za-z0-9-]{1,63}(?<!-))*$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// Characters commonly associated with path traversal and injection attacks.
        /// </summary>
        private static readonly char[] DangerousPathChars = new[] { '\0', '\r', '\n' };

        /// <summary>
        /// Validates that an encryption key meets the required length and entropy criteria.
        /// </summary>
        /// <param name="key">The encryption key to validate.</param>
        /// <returns><c>true</c> if the key is valid; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// A valid key must be exactly <see cref="AesGcmEncryptor.KeySize"/> bytes (32 bytes / 256 bits)
        /// and must not be composed entirely of zero bytes.
        /// </remarks>
        public static bool ValidateEncryptionKey(byte[] key)
        {
            if (key == null)
                return false;

            if (key.Length != AesGcmEncryptor.KeySize)
                return false;

            return key.Any(b => b != 0);
        }

        /// <summary>
        /// Validates that a file path is safe and does not contain path traversal sequences.
        /// </summary>
        /// <param name="path">The file path to validate.</param>
        /// <returns><c>true</c> if the path is valid and safe; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Checks for null bytes, newline characters, directory traversal sequences
        /// (..), and attempts to resolve the path to ensure it does not escape
        /// the intended directory boundary.
        /// </remarks>
        public static bool ValidateFilePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            if (path.IndexOfAny(DangerousPathChars) >= 0)
                return false;

            if (path.Contains("\0"))
                return false;

            if (path.Contains(".."))
                return false;

            if (path.Contains("//"))
                return false;

            try
            {
                string fullPath = Path.GetFullPath(path);
                string normalizedPath = fullPath.Replace('\\', '/');

                if (normalizedPath.Contains("/../") || normalizedPath.EndsWith("/.."))
                    return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (PathTooLongException)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates that a hostname is well-formed and free of injection characters.
        /// </summary>
        /// <param name="hostname">The hostname to validate.</param>
        /// <returns><c>true</c> if the hostname is valid; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Validates against RFC 952/1123 hostname format rules:
        /// <list type="bullet">
        ///   <item>Labels must be 1-63 characters</item>
        ///   <item>Labels contain only alphanumeric characters and hyphens</item>
        ///   <item>Labels cannot start or end with a hyphen</item>
        ///   <item>The total hostname must not exceed 253 characters</item>
        ///   <item>No special characters that could enable injection attacks</item>
        /// </list>
        /// </remarks>
        public static bool ValidateHostname(string hostname)
        {
            if (string.IsNullOrWhiteSpace(hostname))
                return false;

            if (hostname.Length > 253)
                return false;

            if (hostname.Contains("\0") || hostname.Contains("\r") || hostname.Contains("\n"))
                return false;

            if (hostname.Contains(" ") || hostname.Contains("\t"))
                return false;

            if (hostname.Contains(".."))
                return false;

            return HostnameRegex.IsMatch(hostname);
        }
    }
}
