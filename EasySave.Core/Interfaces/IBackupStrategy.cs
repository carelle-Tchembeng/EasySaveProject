// EasySave.Core/Interfaces/IBackupStrategy.cs

using EasySave.Core.Entities;

namespace EasySave.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for a backup execution strategy.
    /// Implementations: FullBackupStrategy, DifferentialBackupStrategy.
    /// Follows the Strategy design pattern to allow interchangeable algorithms.
    /// </summary>
    public interface IBackupStrategy
    {
        /// <summary>
        /// Executes the backup strategy for the given job.
        /// Copies eligible files from job.SourcePath to job.TargetPath.
        /// Calls onFileCopied after each file transfer (successful or failed).
        /// </summary>
        /// <param name="job">The backup job to execute.</param>
        /// <param name="fileSystem">Abstraction over file system operations.</param>
        /// <param name="logger">Abstraction over log writing (EasyLog).</param>
        /// <param name="onFileCopied">
        /// Callback invoked after each file is processed.
        /// Parameters: (sourceFile, destFile, fileSizeBytes, transferTimeMs)
        /// transferTimeMs is negative if the copy failed.
        /// </param>
        void Execute(
            BackupJob job,
            IFileSystem fileSystem,
            ILogger logger,
            Action<string, string, long, long> onFileCopied);
    }
}
