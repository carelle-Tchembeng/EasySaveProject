// EasySave.Infrastructure/Strategies/DifferentialBackupStrategy.cs
// UPDATED v3.0 — handles priority, large-file lock, pause/stop per-file

using EasySave.Core.Entities;
using EasySave.Core.Interfaces;
using EasySave.Core.Services;
using EasySave.Infrastructure.Helpers;

namespace EasySave.Infrastructure.Strategies
{
    /// <summary>
    /// Copies only files modified since the last full backup.
    /// v3.0: participates in parallel coordination (same per-file order as FullBackupStrategy).
    /// </summary>
    public class DifferentialBackupStrategy : IBackupStrategy
    {
        public void Execute(
            BackupJob              job,
            IFileSystem            fileSystem,
            ILogger                logger,
            IEncryptionService     encryptionService,
            AppConfiguration       config,
            JobExecutionContext     context,
            PriorityManager        priorityManager,
            LargeFileTransferLock  largeFileLock,
            Action<string, string, long, long, long> onFileCopied)
        {
            DateTime referenceDate = job.LastFullBackupDate!.Value;

            foreach (string sourceFile in fileSystem.GetFiles(job.SourcePath))
            {
                if (!IsModifiedSince(sourceFile, referenceDate, fileSystem)) continue;

                // 1. Stop check
                context.StopToken.ThrowIfCancellationRequested();

                long fileSize = fileSystem.GetFileSize(sourceFile);

                // 2. Priority gate
                priorityManager.WaitIfNonPriority(sourceFile, context.StopToken);

                // 3. Register priority + acquire large-file lock
                priorityManager.RegisterFile(sourceFile);
                largeFileLock.Acquire(fileSize, context.StopToken);

                try
                {
                    string destFile = PathHelper.MapToTargetPath(job.SourcePath, job.TargetPath, sourceFile);

                    string? destDir = Path.GetDirectoryName(destFile);
                    if (!string.IsNullOrEmpty(destDir)) fileSystem.CreateDirectory(destDir);

                    string uncSource = fileSystem.ToUncPath(sourceFile);
                    string uncDest   = fileSystem.ToUncPath(destFile);

                    // 4. Copy
                    long transferTimeMs = fileSystem.CopyFile(sourceFile, destFile);

                    // 4b. Encrypt
                    long encryptionTimeMs = 0;
                    if (transferTimeMs >= 0 && ShouldEncrypt(sourceFile, config))
                        encryptionTimeMs = encryptionService.Encrypt(destFile);

                    // 5. Callback
                    onFileCopied(uncSource, uncDest, fileSize, transferTimeMs, encryptionTimeMs);
                }
                finally
                {
                    // 6. Always release
                    largeFileLock.Release(fileSize);
                    priorityManager.ReleaseFile(sourceFile);
                }

                // 7. Pause check AFTER file is done
                context.WaitIfPaused();
            }
        }

        private static bool IsModifiedSince(string filePath, DateTime referenceDate, IFileSystem fs)
            => fs.GetLastWriteTime(filePath) > referenceDate;

        private static bool ShouldEncrypt(string filePath, AppConfiguration config)
            => config.IsExtensionEncrypted(Path.GetExtension(filePath));
    }
}
