using EasySave.Core.Entities;
using EasySave.Core.Interfaces;
using EasySave.Infrastructure.Helpers;

namespace EasySave.Infrastructure.Strategies
{
    /// <summary>
    /// Full backup strategy.
    ///
    /// This strategy copies every file from the source directory
    /// to the target directory, preserving the directory structure.
    /// </summary>
    public class FullBackupStrategy : IBackupStrategy
    {
        /// <summary>
        /// For a full backup, every file in the source directory is eligible.
        /// </summary>
        public List<string> GetEligibleFiles(
            BackupJob job,
            IFileSystem fileSystem)
        {
            return fileSystem.GetFiles(job.SourcePath);
        }

        /// <summary>
        /// Executes the full backup.
        ///
        /// Steps:
        /// 1. Recreate the directory structure in the target.
        /// 2. Copy every eligible file.
        /// 3. Notify BackupService through the callback after each file.
        /// </summary>
        public void Execute(
            BackupJob job,
            IFileSystem fileSystem,
            ILogger logger,
            Action<string, string, long, long> onFileCopied)
        {
            // First recreate all subdirectories in the target location.
            var directories = fileSystem.GetDirectories(job.SourcePath);

            foreach (string sourceDir in directories)
            {
                string destDir = PathHelper.MapToTargetPath(
                    job.SourcePath,
                    job.TargetPath,
                    sourceDir);

                fileSystem.CreateDirectory(destDir);
            }

            // Then copy all files.
            var files = GetEligibleFiles(job, fileSystem);

            foreach (string sourceFile in files)
            {
                string destFile = PathHelper.MapToTargetPath(
                    job.SourcePath,
                    job.TargetPath,
                    sourceFile);

                // Logs must contain UNC paths according to the specification.
                string uncSource = fileSystem.ToUncPath(sourceFile);
                string uncDest = fileSystem.ToUncPath(destFile);

                long fileSize = fileSystem.GetFileSize(sourceFile);
                long transferTime = fileSystem.CopyFile(sourceFile, destFile);

                // Delegate progress update, state update and logging to BackupService.
                onFileCopied(uncSource, uncDest, fileSize, transferTime);
            }
        }
    }
}
