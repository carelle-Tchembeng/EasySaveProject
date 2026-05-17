// EasySave.Infrastructure/Strategies/FullBackupStrategy.cs
// UPDATED v2.0 — Execute() adds IEncryptionService + AppConfiguration, callback has 5 params

using EasySave.Core.Entities;
using EasySave.Core.Interfaces;
using EasySave.Infrastructure.Helpers;

namespace EasySave.Infrastructure.Strategies
{
    /// <summary>
    /// Copies ALL files from source to target.
    /// v2.0: encrypts files whose extension is in config.EncryptedExtensions.
    /// Callback: (sourceFile, destFile, sizeBytes, transferMs, encryptionMs)
    /// </summary>
    public class FullBackupStrategy : IBackupStrategy
    {
        public void Execute(
            BackupJob          job,
            IFileSystem        fileSystem,
            ILogger            logger,
            IEncryptionService encryptionService,
            AppConfiguration   config,
            Action<string, string, long, long, long> onFileCopied)
        {
            // Recreate directory structure in target
            foreach (string sourceDir in fileSystem.GetDirectories(job.SourcePath))
            {
                string destDir = PathHelper.MapToTargetPath(job.SourcePath, job.TargetPath, sourceDir);
                fileSystem.CreateDirectory(destDir);
            }

            // Copy every file
            foreach (string sourceFile in fileSystem.GetFiles(job.SourcePath))
            {
                string destFile       = PathHelper.MapToTargetPath(job.SourcePath, job.TargetPath, sourceFile);
                string uncSource      = fileSystem.ToUncPath(sourceFile);
                string uncDest        = fileSystem.ToUncPath(destFile);
                long   fileSize       = fileSystem.GetFileSize(sourceFile);
                long   transferTimeMs = fileSystem.CopyFile(sourceFile, destFile);

                long encryptionTimeMs = 0;
                if (transferTimeMs >= 0 && ShouldEncrypt(sourceFile, config))
                    encryptionTimeMs = encryptionService.Encrypt(destFile);

                onFileCopied(uncSource, uncDest, fileSize, transferTimeMs, encryptionTimeMs);
            }
        }

        private static bool ShouldEncrypt(string filePath, AppConfiguration config)
            => config.IsExtensionEncrypted(Path.GetExtension(filePath));
    }
}
