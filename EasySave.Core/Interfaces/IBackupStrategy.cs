// EasySave.Core/Interfaces/IBackupStrategy.cs
// UPDATED v2.0 — Execute() adds IEncryptionService + AppConfiguration parameters

using EasySave.Core.Entities;

namespace EasySave.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for a backup execution strategy.
    /// v2.0: Execute() receives IEncryptionService and AppConfiguration
    /// so strategies can encrypt files based on user configuration.
    /// </summary>
    public interface IBackupStrategy
    {
        /// <summary>
        /// Executes the backup strategy for the given job.
        /// Copies eligible files from job.SourcePath to job.TargetPath.
        /// Encrypts files whose extension is in config.EncryptedExtensions.
        /// Calls onFileCopied after each file transfer (successful or failed).
        /// </summary>
        /// <param name="job">The backup job to execute.</param>
        /// <param name="fileSystem">File system abstraction.</param>
        /// <param name="logger">Log writing abstraction.</param>
        /// <param name="encryptionService">CryptoSoft encryption abstraction. NEW v2.0.</param>
        /// <param name="config">Application configuration (extensions, paths). NEW v2.0.</param>
        /// <param name="onFileCopied">
        /// Callback invoked after each file is processed.
        /// Parameters: (sourceFile, destFile, fileSizeBytes, transferTimeMs, encryptionTimeMs)
        /// transferTimeMs is negative on copy failure.
        /// encryptionTimeMs: 0=not encrypted, >0=encrypted, &lt;0=encryption error.
        /// </param>
        void Execute(
            BackupJob          job,
            IFileSystem        fileSystem,
            ILogger            logger,
            IEncryptionService encryptionService,
            AppConfiguration   config,
            Action<string, string, long, long, long> onFileCopied);
    }
}
