// EasySave.Infrastructure/Strategies/FullBackupStrategy.cs
// UPDATED v3.0 — handles priority, large-file lock, pause/stop per-file

using EasySave.Core.Entities;
using EasySave.Core.Interfaces;
using EasySave.Core.Services;
using EasySave.Infrastructure.Helpers;

namespace EasySave.Infrastructure.Strategies
{
    /// <summary>
    /// Copies ALL files from source to target.
    /// v3.0: participates in parallel coordination (priority, large-file lock, pause/stop).
    ///
    /// Per-file order of operations (matching sequence diagram):
    ///   1. Check stop token.
    ///   2. Wait for priority window (non-priority files block while priority files are in flight).
    ///   3. Register priority (if applicable) + acquire large-file lock.
    ///   4. Copy + encrypt.
    ///   5. Call onFileCopied callback.
    ///   6. Release large-file lock + deregister priority.
    ///   7. Wait if paused (pause effective AFTER current file — spec requirement).
    /// </summary>
    public class FullBackupStrategy : IBackupStrategy
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
            // Recreate directory structure in target
            foreach (string sourceDir in fileSystem.GetDirectories(job.SourcePath))
            {
                string destDir = PathHelper.MapToTargetPath(job.SourcePath, job.TargetPath, sourceDir);
                fileSystem.CreateDirectory(destDir);
            }

            foreach (string sourceFile in fileSystem.GetFiles(job.SourcePath))
            {
                // 1. Stop check
                context.StopToken.ThrowIfCancellationRequested();

                long fileSize = fileSystem.GetFileSize(sourceFile);

                // 2. Priority gate — non-priority files wait until all priority files are done
                priorityManager.WaitIfNonPriority(sourceFile, context.StopToken);

                // 3. Register priority + acquire large-file lock
                priorityManager.RegisterFile(sourceFile);
                largeFileLock.Acquire(fileSize, context.StopToken);

                try
                {
                    string destFile  = PathHelper.MapToTargetPath(job.SourcePath, job.TargetPath, sourceFile);
                    string uncSource = fileSystem.ToUncPath(sourceFile);
                    string uncDest   = fileSystem.ToUncPath(destFile);

                    // 4. Copy
                    long transferTimeMs = fileSystem.CopyFile(sourceFile, destFile);

                    // 4b. Encrypt (v2.0 feature, maintained in v3.0)
                    long encryptionTimeMs = 0;
                    if (transferTimeMs >= 0 && ShouldEncrypt(sourceFile, config))
                        encryptionTimeMs = encryptionService.Encrypt(destFile);

                    // 5. Callback
                    onFileCopied(uncSource, uncDest, fileSize, transferTimeMs, encryptionTimeMs);
                }
                finally
                {
                    // 6. Always release — even on exception
                    largeFileLock.Release(fileSize);
                    priorityManager.ReleaseFile(sourceFile);
                }

                // 7. Pause check AFTER file is done (spec: "pause effective after current file")
                context.WaitIfPaused();
            }
        }

        private static bool ShouldEncrypt(string filePath, AppConfiguration config)
            => config.IsExtensionEncrypted(Path.GetExtension(filePath));
    }
}
