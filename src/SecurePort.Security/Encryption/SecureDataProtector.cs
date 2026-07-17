using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace SecurePort.Security.Encryption
{
    /// <summary>
    /// Higher-level encryption operations for strings and files.
    /// Manages encryption keys and uses platform-specific secure storage.
    /// </summary>
    public class SecureDataProtector
    {
        private readonly IEncryptor _encryptor;

        /// <summary>
        /// The default directory name used for storing encryption keys.
        /// </summary>
        private const string KeyDirectoryName = ".secureport-keys";

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureDataProtector"/> class
        /// using the default <see cref="AesGcmEncryptor"/>.
        /// </summary>
        public SecureDataProtector() : this(new AesGcmEncryptor())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureDataProtector"/> class
        /// with a custom encryption implementation.
        /// </summary>
        /// <param name="encryptor">The encryption provider to use.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="encryptor"/> is null.</exception>
        public SecureDataProtector(IEncryptor encryptor)
        {
            _encryptor = encryptor ?? throw new ArgumentNullException(nameof(encryptor));
        }

        /// <summary>
        /// Encrypts a plaintext string to a base64-encoded ciphertext string.
        /// </summary>
        /// <param name="plaintext">The string to encrypt.</param>
        /// <param name="key">The encryption key.</param>
        /// <returns>A base64-encoded string containing the encrypted data.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="plaintext"/> or <paramref name="key"/> is null.</exception>
        public string EncryptString(string plaintext, byte[] key)
        {
            if (plaintext == null) throw new ArgumentNullException(nameof(plaintext));
            if (key == null) throw new ArgumentNullException(nameof(key));

            byte[] data = Encoding.UTF8.GetBytes(plaintext);
            byte[] encrypted = _encryptor.Encrypt(data, key);
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// Decrypts a base64-encoded ciphertext string back to the original plaintext.
        /// </summary>
        /// <param name="ciphertext">The base64-encoded encrypted string.</param>
        /// <param name="key">The decryption key.</param>
        /// <returns>The decrypted plaintext string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ciphertext"/> or <paramref name="key"/> is null.</exception>
        /// <exception cref="SecurityException">Thrown when authentication fails.</exception>
        public string DecryptString(string ciphertext, byte[] key)
        {
            if (ciphertext == null) throw new ArgumentNullException(nameof(ciphertext));
            if (key == null) throw new ArgumentNullException(nameof(key));

            byte[] encrypted = Convert.FromBase64String(ciphertext);
            byte[] decrypted = _encryptor.Decrypt(encrypted, key);
            return Encoding.UTF8.GetString(decrypted);
        }

        /// <summary>
        /// Encrypts the contents of a file and writes the result to a destination file.
        /// </summary>
        /// <param name="sourcePath">The path to the file to encrypt.</param>
        /// <param name="destinationPath">The path to write the encrypted file.</param>
        /// <param name="key">The encryption key.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the source file does not exist.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the destination directory does not exist.</exception>
        public void EncryptFile(string sourcePath, string destinationPath, byte[] key)
        {
            if (sourcePath == null) throw new ArgumentNullException(nameof(sourcePath));
            if (destinationPath == null) throw new ArgumentNullException(nameof(destinationPath));
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException("Source file not found.", sourcePath);

            string? destDir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                throw new DirectoryNotFoundException($"Destination directory not found: {destDir}");

            byte[] plaintext = File.ReadAllBytes(sourcePath);
            byte[] encrypted = _encryptor.Encrypt(plaintext, key);
            File.WriteAllBytes(destinationPath, encrypted);
        }

        /// <summary>
        /// Decrypts an encrypted file and writes the result to a destination file.
        /// </summary>
        /// <param name="sourcePath">The path to the encrypted file.</param>
        /// <param name="destinationPath">The path to write the decrypted file.</param>
        /// <param name="key">The decryption key.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the source file does not exist.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the destination directory does not exist.</exception>
        /// <exception cref="SecurityException">Thrown when authentication fails.</exception>
        public void DecryptFile(string sourcePath, string destinationPath, byte[] key)
        {
            if (sourcePath == null) throw new ArgumentNullException(nameof(sourcePath));
            if (destinationPath == null) throw new ArgumentNullException(nameof(destinationPath));
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException("Encrypted file not found.", sourcePath);

            string? destDir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                throw new DirectoryNotFoundException($"Destination directory not found: {destDir}");

            byte[] encrypted = File.ReadAllBytes(sourcePath);
            byte[] decrypted = _encryptor.Decrypt(encrypted, key);
            File.WriteAllBytes(destinationPath, decrypted);
        }

        /// <summary>
        /// Generates a new cryptographically secure encryption key.
        /// </summary>
        /// <returns>A 32-byte (256-bit) encryption key.</returns>
        public byte[] GenerateKey()
        {
            return _encryptor.GenerateKey();
        }

        /// <summary>
        /// Persists an encryption key to the platform-specific secure storage location.
        /// </summary>
        /// <param name="key">The encryption key to store.</param>
        /// <param name="keyName">A unique name to identify this key.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> or <paramref name="keyName"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="keyName"/> is empty or whitespace.</exception>
        /// <exception cref="IOException">Thrown when the key file cannot be written.</exception>
        /// <remarks>
        /// On Linux/macOS, the key directory is created with 700 permissions.
        /// On Windows, the key directory inherits default permissions.
        /// The key file is written with restricted permissions where supported.
        /// </remarks>
        public void StoreKey(byte[] key, string keyName)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrWhiteSpace(keyName)) throw new ArgumentException("Key name cannot be empty.", nameof(keyName));

            string keyDirectory = GetSecureKeyDirectory();
            Directory.CreateDirectory(keyDirectory);

            string keyPath = Path.Combine(keyDirectory, SanitizeKeyFileName(keyName) + ".key");
            File.WriteAllBytes(keyPath, key);

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                SetUnixFilePermissions(keyPath);
            }
        }

        /// <summary>
        /// Loads a previously stored encryption key from the platform-specific secure storage.
        /// </summary>
        /// <param name="keyName">The name used when the key was stored.</param>
        /// <returns>The encryption key, or null if the key was not found.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="keyName"/> is empty or whitespace.</exception>
        public byte[]? LoadKey(string keyName)
        {
            if (string.IsNullOrWhiteSpace(keyName)) throw new ArgumentException("Key name cannot be empty.", nameof(keyName));

            string keyDirectory = GetSecureKeyDirectory();
            string keyPath = Path.Combine(keyDirectory, SanitizeKeyFileName(keyName) + ".key");

            if (!File.Exists(keyPath))
                return null;

            return File.ReadAllBytes(keyPath);
        }

        /// <summary>
        /// Returns the platform-specific directory used for secure key storage.
        /// </summary>
        /// <returns>The full path to the key storage directory.</returns>
        private static string GetSecureKeyDirectory()
        {
            string basePath;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                basePath = Path.Combine(home, "Library", "Application Support");
            }
            else
            {
                string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                basePath = string.IsNullOrEmpty(home) ? "/tmp" : home;
            }

            return Path.Combine(basePath, KeyDirectoryName);
        }

        /// <summary>
        /// Restricts file permissions to owner-only (rwx------) on Unix systems.
        /// </summary>
        /// <param name="filePath">The file to restrict permissions on.</param>
        private static void SetUnixFilePermissions(string filePath)
        {
            try
            {
                var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "chmod",
                    Arguments = $"600 \"{filePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                process?.WaitForExit(3000);
                process?.Dispose();
            }
            catch
            {
                // Best effort — file is still usable even if permissions aren't restricted.
            }
        }

        /// <summary>
        /// Sanitizes a key name to be safe for use as a filename.
        /// </summary>
        /// <param name="keyName">The key name to sanitize.</param>
        /// <returns>A sanitized filename-safe string.</returns>
        private static string SanitizeKeyFileName(string keyName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                keyName = keyName.Replace(c, '_');
            }
            return keyName;
        }
    }
}
