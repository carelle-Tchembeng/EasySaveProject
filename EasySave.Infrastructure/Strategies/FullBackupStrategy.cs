// EasySave.Infrastructure/Strategies/FullBackupStrategy.cs

using EasySave.Core.Entities;
using EasySave.Core.Interfaces;
using EasySave.Infrastructure.Helpers;

namespace EasySave.Infrastructure.Strategies
{
    /// <summary>
    /// Backup strategy that copies all files from source to target,
    /// regardless of their modification date.
    /// Every execution is a complete mirror of the source directory.
    /// Implements IBackupStrategy following the Strategy design pattern.
    /// </summary>
    public class FullBackupStrategy : IBackupStrategy
    {
        /// <summary>
        /// Executes a full backup: copies every file in the source directory tree
        /// to the corresponding location in the target directory.
        /// Recreates the full directory structure in the target.
        /// Calls onFileCopied after each file transfer (success or failure).
        /// </summary>
        /// <param name="job">Backup job containing source and target paths.</param>
        /// <param name="fileSystem">File system abstraction for I/O operations.</param>
        /// <param name="logger">Logger abstraction (unused here — logging done via callback).</param>
        /// <param name="onFileCopied">
        /// Callback invoked after each file.
        /// Parameters: (sourceFile, destFile, fileSizeBytes, transferTimeMs)
        /// transferTimeMs is negative on copy failure.
        /// </param>
        public void Execute(
            BackupJob job,
            IFileSystem fileSystem,
            ILogger logger,
            Action<string, string, long, long> onFileCopied)
        {
            // Step 1: Recreate the full directory structure in the target
            var directories = fileSystem.GetDirectories(job.SourcePath);
            foreach (string sourceDir in directories)
            {
                string destDir = PathHelper.MapToTargetPath(
                    job.SourcePath, job.TargetPath, sourceDir);

                fileSystem.CreateDirectory(destDir);
            }

            // Step 2: Copy every file in the source tree
            var files = fileSystem.GetFiles(job.SourcePath);
            foreach (string sourceFile in files)
            {
                string destFile = PathHelper.MapToTargetPath(
                    job.SourcePath, job.TargetPath, sourceFile);

                // Convert paths to UNC format for log entries
                string uncSource = fileSystem.ToUncPath(sourceFile);
                string uncDest   = fileSystem.ToUncPath(destFile);

                long fileSize     = fileSystem.GetFileSize(sourceFile);
                long transferTime = fileSystem.CopyFile(sourceFile, destFile);

                // Notify BackupService — triggers state.json update and log write
                onFileCopied(uncSource, uncDest, fileSize, transferTime);
            }
        }
    }
}
