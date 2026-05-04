// EasySave.Infrastructure/Strategies/DifferentialBackupStrategy.cs

using EasySave.Core.Entities;
using EasySave.Core.Interfaces;
using EasySave.Infrastructure.Helpers;

namespace EasySave.Infrastructure.Strategies
{
    /// <summary>
    /// Backup strategy that copies only files modified since the last full backup.
    /// Compares each file's LastWriteTime against job.LastFullBackupDate.
    /// Files that have not changed are silently skipped.
    ///
    /// Prerequisite: job.LastFullBackupDate must not be null.
    /// If null, BackupService automatically falls back to FullBackupStrategy.
    ///
    /// Implements IBackupStrategy following the Strategy design pattern.
    /// </summary>
    public class DifferentialBackupStrategy : IBackupStrategy
    {
        /// <summary>
        /// Executes a differential backup: copies only files whose LastWriteTime
        /// is more recent than job.LastFullBackupDate.
        /// Recreates only the subdirectories that contain modified files.
        /// Calls onFileCopied for each file that is actually copied.
        /// </summary>
        /// <param name="job">
        /// Backup job containing source/target paths and LastFullBackupDate.
        /// LastFullBackupDate must not be null when this strategy is used.
        /// </param>
        /// <param name="fileSystem">File system abstraction for I/O operations.</param>
        /// <param name="logger">Logger abstraction (unused — logging done via callback).</param>
        /// <param name="onFileCopied">
        /// Callback invoked after each copied file.
        /// Parameters: (sourceFile, destFile, fileSizeBytes, transferTimeMs)
        /// transferTimeMs is negative on copy failure.
        /// Files that are skipped (not modified) do NOT trigger this callback.
        /// </param>
        public void Execute(
            BackupJob job,
            IFileSystem fileSystem,
            ILogger logger,
            Action<string, string, long, long> onFileCopied)
        {
            // LastFullBackupDate must be set — enforced by BackupService.ResolveStrategy()
            DateTime referenceDate = job.LastFullBackupDate!.Value;

            // Retrieve all files recursively from the source directory
            var allFiles = fileSystem.GetFiles(job.SourcePath);

            foreach (string sourceFile in allFiles)
            {
                // Check if this file was modified after the last full backup
                if (!IsModifiedSince(sourceFile, referenceDate, fileSystem))
                    continue; // File unchanged — skip it

                string destFile = PathHelper.MapToTargetPath(
                    job.SourcePath, job.TargetPath, sourceFile);

                // Ensure the destination subdirectory exists
                string? destDirectory = Path.GetDirectoryName(destFile);
                if (!string.IsNullOrEmpty(destDirectory))
                    fileSystem.CreateDirectory(destDirectory);

                // Convert paths to UNC format for log entries
                string uncSource = fileSystem.ToUncPath(sourceFile);
                string uncDest   = fileSystem.ToUncPath(destFile);

                long fileSize     = fileSystem.GetFileSize(sourceFile);
                long transferTime = fileSystem.CopyFile(sourceFile, destFile);

                // Notify BackupService — triggers state.json update and log write
                onFileCopied(uncSource, uncDest, fileSize, transferTime);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Private helpers
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns true if the file was last modified after the specified reference date.
        /// A file is considered "modified" if its LastWriteTime is strictly greater than
        /// the reference date, meaning it was changed after the last full backup.
        /// </summary>
        /// <param name="filePath">Full path of the file to check.</param>
        /// <param name="referenceDate">
        /// The date to compare against — typically job.LastFullBackupDate.
        /// </param>
        /// <param name="fileSystem">File system abstraction for metadata access.</param>
        /// <returns>True if the file was modified after the reference date.</returns>
        private static bool IsModifiedSince(
            string      filePath,
            DateTime    referenceDate,
            IFileSystem fileSystem)
        {
            DateTime lastWriteTime = fileSystem.GetLastWriteTime(filePath);
            return lastWriteTime > referenceDate;
        }
    }
}
