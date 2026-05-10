using EasySave.Core.Entities;
using EasySave.Core.Interfaces;
using EasySave.Infrastructure.Helpers;

namespace EasySave.Infrastructure.Strategies
{
    /// <summary>
    /// Differential backup strategy.
    ///
    /// This strategy copies only files modified after LastFullBackupDate.
    ///
    /// If LastFullBackupDate is null, all files are considered eligible.
    /// In normal execution, BackupService handles this case by selecting
    /// FullBackupStrategy instead.
    /// </summary>
    public class DifferentialBackupStrategy : IBackupStrategy
    {
        /// <summary>
        /// Returns files modified after the last full backup.
        /// </summary>
        public List<string> GetEligibleFiles(
            BackupJob job,
            IFileSystem fileSystem)
        {
            if (job.LastFullBackupDate == null)
            {
                return fileSystem.GetFiles(job.SourcePath);
            }

            DateTime referenceDate = job.LastFullBackupDate.Value;

            return fileSystem
                .GetFiles(job.SourcePath)
                .Where(file => IsModifiedSince(file, referenceDate, fileSystem))
                .ToList();
        }

        /// <summary>
        /// Executes the differential backup.
        ///
        /// Only eligible files are copied.
        /// Unmodified files are skipped silently.
        /// </summary>
        public void Execute(
            BackupJob job,
            IFileSystem fileSystem,
            ILogger logger,
            Action<string, string, long, long> onFileCopied)
        {
            var files = GetEligibleFiles(job, fileSystem);

            foreach (string sourceFile in files)
            {
                string destFile = PathHelper.MapToTargetPath(
                    job.SourcePath,
                    job.TargetPath,
                    sourceFile);

                // Ensure the target directory exists before copying the file.
                string? destDirectory = Path.GetDirectoryName(destFile);

                if (!string.IsNullOrWhiteSpace(destDirectory))
                {
                    fileSystem.CreateDirectory(destDirectory);
                }

                string uncSource = fileSystem.ToUncPath(sourceFile);
                string uncDest = fileSystem.ToUncPath(destFile);

                long fileSize = fileSystem.GetFileSize(sourceFile);
                long transferTime = fileSystem.CopyFile(sourceFile, destFile);

                onFileCopied(uncSource, uncDest, fileSize, transferTime);
            }
        }

        /// <summary>
        /// Determines whether a file was modified after the reference date.
        /// </summary>
        private static bool IsModifiedSince(
            string filePath,
            DateTime referenceDate,
            IFileSystem fileSystem)
        {
            DateTime lastWriteTime = fileSystem.GetLastWriteTime(filePath);
            return lastWriteTime > referenceDate;
        }
    }
}
