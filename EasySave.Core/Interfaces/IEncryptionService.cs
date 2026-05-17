// EasySave.Core/Interfaces/IEncryptionService.cs
// NEW v2.0 — abstraction over CryptoSoft encryption

namespace EasySave.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for file encryption via an external tool (CryptoSoft).
    /// Implemented by CryptoSoftAdapter in the Infrastructure layer.
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Encrypts the specified file using CryptoSoft.
        /// The file is encrypted in-place at the destination path.
        /// </summary>
        /// <param name="filePath">Full path of the file to encrypt.</param>
        /// <returns>
        /// Encryption time in milliseconds if successful.
        /// Negative value if CryptoSoft failed or returned an error code.
        /// </returns>
        long Encrypt(string filePath);

        /// <summary>
        /// Returns true if CryptoSoft is available and accessible.
        /// Checks that the CryptoSoft executable exists at the configured path.
        /// </summary>
        bool IsAvailable();
    }
}
