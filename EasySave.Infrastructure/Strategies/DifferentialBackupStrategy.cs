// EasySave.Infrastructure/Strategies/DifferentialBackupStrategy.cs
// UPDATED v2.0 — adds IEncryptionService + AppConfiguration, callback has 5 params

using EasySave.Core.Entities;
using EasySave.Core.Interfaces;
using EasySave.Infrastructure.Helpers;

namespace EasySave.Infrastructure.Strategies
{
    /// <summary>
    /// Copies only files modified since the last full backup.
    /// v2.0: encrypts eligible files after copy.
    /// </summary>
    public class DifferentialBackupStrategy : IBackupStrategy
    {
        public void Execute(
            BackupJob          job,
            IFileSystem        fileSystem,
            ILogger            logger,
            IEncryptionService encryptionService,
            AppConfiguration   config,
            Action<string, string, long, long, long> onFileCopied)
        {
            DateTime referenceDate = job.LastFullBackupDate!.Value;

            foreach (string sourceFile in fileSystem.GetFiles(job.SourcePath))
            {
                if (!IsModifiedSince(sourceFile, referenceDate, fileSystem)) continue;

                string destFile = PathHelper.MapToTargetPath(job.SourcePath, job.TargetPath, sourceFile);

                string? destDir = Path.GetDirectoryName(destFile);
                if (!string.IsNullOrEmpty(destDir)) fileSystem.CreateDirectory(destDir);

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

        private static bool IsModifiedSince(string filePath, DateTime referenceDate, IFileSystem fs)
            => fs.GetLastWriteTime(filePath) > referenceDate;

        private static bool ShouldEncrypt(string filePath, AppConfiguration config)
            => config.IsExtensionEncrypted(Path.GetExtension(filePath));
    }
}
