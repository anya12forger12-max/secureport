using System;
using System.Security;
using System.Security.Cryptography;

namespace SecurePort.Security.Encryption
{
    /// <summary>
    /// Provides an interface for symmetric encryption operations.
    /// </summary>
    public interface IEncryptor
    {
        /// <summary>
        /// Encrypts the specified data using the provided key.
        /// </summary>
        /// <param name="data">The plaintext data to encrypt.</param>
        /// <param name="key">The encryption key.</param>
        /// <returns>The encrypted data including nonce and authentication tag.</returns>
        byte[] Encrypt(byte[] data, byte[] key);

        /// <summary>
        /// Decrypts the specified encrypted data using the provided key.
        /// </summary>
        /// <param name="encrypted">The encrypted data (nonce + ciphertext + tag).</param>
        /// <param name="key">The decryption key.</param>
        /// <returns>The decrypted plaintext data.</returns>
        byte[] Decrypt(byte[] encrypted, byte[] key);

        /// <summary>
        /// Generates a new cryptographically secure random key suitable for AES-256-GCM.
        /// </summary>
        /// <returns>A 32-byte (256-bit) encryption key.</returns>
        byte[] GenerateKey();
    }

    /// <summary>
    /// AES-256-GCM authenticated encryption implementation.
    /// Provides confidentiality and integrity for data using Galois/Counter Mode.
    /// </summary>
    public class AesGcmEncryptor : IEncryptor
    {
        /// <summary>
        /// The size of the nonce (IV) in bytes for GCM mode.
        /// </summary>
        public const int NonceSize = 12;

        /// <summary>
        /// The size of the authentication tag in bytes for GCM mode.
        /// </summary>
        public const int TagSize = 16;

        /// <summary>
        /// The required key size in bytes for AES-256 (256 bits).
        /// </summary>
        public const int KeySize = 32;

        /// <summary>
        /// The overhead size added by nonce and tag combined.
        /// </summary>
        public const int OverheadSize = NonceSize + TagSize;

        /// <summary>
        /// Encrypts the specified data using AES-256-GCM with a randomly generated nonce.
        /// The output format is: nonce (12 bytes) + ciphertext + tag (16 bytes).
        /// </summary>
        /// <param name="data">The plaintext data to encrypt.</param>
        /// <param name="key">The 256-bit encryption key.</param>
        /// <returns>A byte array containing the nonce, encrypted data, and authentication tag.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> or <paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the key size is not 32 bytes.</exception>
        public byte[] Encrypt(byte[] data, byte[] key)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (key.Length != KeySize)
                throw new ArgumentException($"Key must be {KeySize} bytes. Got {key.Length}.", nameof(key));

            byte[] nonce = new byte[NonceSize];
            RandomNumberGenerator.Fill(nonce);

            byte[] ciphertext = new byte[data.Length];
            byte[] tag = new byte[TagSize];

            using (var aes = new AesGcm(key))
            {
                aes.Encrypt(nonce, data, ciphertext, tag);
            }

            byte[] result = new byte[NonceSize + ciphertext.Length + TagSize];
            Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
            Buffer.BlockCopy(ciphertext, 0, result, NonceSize, ciphertext.Length);
            Buffer.BlockCopy(tag, 0, result, NonceSize + ciphertext.Length, TagSize);

            return result;
        }

        /// <summary>
        /// Decrypts the specified data using AES-256-GCM with tag verification.
        /// Expects input format: nonce (12 bytes) + ciphertext + tag (16 bytes).
        /// </summary>
        /// <param name="encrypted">The encrypted data (nonce + ciphertext + tag).</param>
        /// <param name="key">The 256-bit decryption key.</param>
        /// <returns>The decrypted plaintext data.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="encrypted"/> or <paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the key size is not 32 bytes.</exception>
        /// <exception cref="SecurityException">Thrown when authentication tag verification fails, indicating data tampering.</exception>
        /// <exception cref="CryptographicException">Thrown when the encrypted data is malformed or too short.</exception>
        public byte[] Decrypt(byte[] encrypted, byte[] key)
        {
            if (encrypted == null) throw new ArgumentNullException(nameof(encrypted));
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (key.Length != KeySize)
                throw new ArgumentException($"Key must be {KeySize} bytes. Got {key.Length}.", nameof(key));
            if (encrypted.Length < OverheadSize)
                throw new CryptographicException("Encrypted data is too short to contain a valid nonce, ciphertext, and tag.");

            byte[] nonce = new byte[NonceSize];
            Buffer.BlockCopy(encrypted, 0, nonce, 0, NonceSize);

            int ciphertextLength = encrypted.Length - OverheadSize;
            byte[] ciphertext = new byte[ciphertextLength];
            Buffer.BlockCopy(encrypted, NonceSize, ciphertext, 0, ciphertextLength);

            byte[] tag = new byte[TagSize];
            Buffer.BlockCopy(encrypted, NonceSize + ciphertextLength, tag, 0, TagSize);

            byte[] plaintext = new byte[ciphertextLength];

            try
            {
            using (var aes = new AesGcm(key))
            {
                aes.Decrypt(nonce, ciphertext, tag, plaintext);
            }
            }
            catch (CryptographicException ex)
            {
                throw new SecurityException("Authentication failed. The data may have been tampered with or the wrong key was used.", ex);
            }

            return plaintext;
        }

        /// <summary>
        /// Generates a cryptographically secure random key suitable for AES-256-GCM.
        /// </summary>
        /// <returns>A 32-byte (256-bit) encryption key.</returns>
        public byte[] GenerateKey()
        {
            byte[] key = new byte[KeySize];
            RandomNumberGenerator.Fill(key);
            return key;
        }
    }
}
