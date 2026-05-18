// EasySave.Core/Interfaces/IBackupStrategy.cs
// UPDATED v3.0 — Execute() adds JobExecutionContext, PriorityManager, LargeFileTransferLock

using EasySave.Core.Entities;
using EasySave.Core.Services;

namespace EasySave.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for a backup execution strategy.
    /// v3.0: Execute() receives shared synchronisation primitives
    /// so strategies can participate in parallel coordination.
    /// </summary>
    public interface IBackupStrategy
    {
        /// <summary>
        /// Executes the backup strategy for the given job.
        ///
        /// v3.0 behaviour per file:
        ///   1. Check stop token — abort immediately if cancelled.
        ///   2. Wait for priority window via PriorityManager.WaitIfNonPriority().
        ///   3. Register priority file (if applicable).
        ///   4. Acquire large-file lock (if file exceeds threshold).
        ///   5. Copy + encrypt the file.
        ///   6. Call onFileCopied callback.
        ///   7. Release large-file lock + priority registration.
        ///   8. Call context.WaitIfPaused() — pause takes effect AFTER current file.
        /// </summary>
        /// <param name="job">The backup job to execute.</param>
        /// <param name="fileSystem">File system abstraction.</param>
        /// <param name="logger">Log writing abstraction.</param>
        /// <param name="encryptionService">CryptoSoft encryption abstraction.</param>
        /// <param name="config">Application configuration.</param>
        /// <param name="context">
        /// NEW v3.0 — Per-job execution context (pause/resume/stop).
        /// </param>
        /// <param name="priorityManager">
        /// NEW v3.0 — Shared across all jobs. Non-priority files wait when priority files are in flight.
        /// </param>
        /// <param name="largeFileLock">
        /// NEW v3.0 — Shared across all jobs. Only one file above the threshold can transfer at a time.
        /// </param>
        /// <param name="onFileCopied">
        /// Callback invoked after each file is processed.
        /// (sourceFile, destFile, fileSizeBytes, transferTimeMs, encryptionTimeMs)
        /// transferTimeMs &lt; 0 on copy failure.
        /// encryptionTimeMs: 0=not encrypted, &gt;0=encrypted, &lt;0=encryption error.
        /// </param>
        void Execute(
            BackupJob              job,
            IFileSystem            fileSystem,
            ILogger                logger,
            IEncryptionService     encryptionService,
            AppConfiguration       config,
            JobExecutionContext     context,
            PriorityManager        priorityManager,
            LargeFileTransferLock  largeFileLock,
            Action<string, string, long, long, long> onFileCopied);
    }
}
