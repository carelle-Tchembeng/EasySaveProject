// EasySave.Core/Entities/BackupJob.cs

using EasySave.Core.Enums;
using EasySave.Core.ValueObjects;

namespace EasySave.Core.Entities
{
    /// <summary>
    /// Represents a backup job configured by the user.
    /// Holds both configuration data (persisted in config.json)
    /// and runtime state (written to state.json during execution).
    /// A maximum of 5 backup jobs can be configured at any time.
    /// </summary>
    public class BackupJob
    {
        // ─────────────────────────────────────────────────────────────
        // Configuration properties — persisted in config.json
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Unique index of the backup job (1 to 5).
        /// Used to identify the job in the CLI and interactive menu.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// User-defined name for the backup job.
        /// Must be non-empty. Example: "Backup_Docs"
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Full path to the source directory to back up.
        /// Supports local drives, external drives, and UNC network paths.
        /// Example: \\srv01\documents
        /// </summary>
        public string SourcePath { get; set; } = string.Empty;

        /// <summary>
        /// Full path to the target directory where files will be copied.
        /// Supports local drives, external drives, and UNC network paths.
        /// Example: \\srv02\backup\documents
        /// </summary>
        public string TargetPath { get; set; } = string.Empty;

        /// <summary>
        /// Type of backup strategy to apply: Full or Differential.
        /// Defaults to Full if not explicitly set.
        /// </summary>
        public BackupType Type { get; set; } = BackupType.Full;

        /// <summary>
        /// Date and time of the last successful full backup execution.
        /// Null if no full backup has ever been performed for this job.
        /// Used by DifferentialBackupStrategy to filter files to copy.
        /// </summary>
        public DateTime? LastFullBackupDate { get; set; } = null;

        // ─────────────────────────────────────────────────────────────
        // Runtime properties — NOT persisted in config.json
        // Written to state.json during execution via IStateRepository
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Current execution status of the backup job.
        /// Updated in real time during execution.
        /// Defaults to Inactive.
        /// </summary>
        public BackupStatus Status { get; set; } = BackupStatus.Inactive;

        /// <summary>
        /// Real-time execution progress of this job.
        /// Null when the job is not active.
        /// Updated after each file is copied.
        /// </summary>
        public BackupProgress? Progress { get; set; } = null;

        // ─────────────────────────────────────────────────────────────
        // State transition methods
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Resets the runtime state to its default inactive values.
        /// Called before starting a new execution to ensure a clean state.
        /// </summary>
        public void ResetState()
        {
            Status   = BackupStatus.Inactive;
            Progress = null;
        }

        /// <summary>
        /// Marks the job as active and initializes progress tracking.
        /// Called by BackupService at the start of an execution.
        /// </summary>
        /// <param name="totalFiles">Total number of files eligible for backup.</param>
        /// <param name="totalSizeBytes">Total size in bytes of all eligible files.</param>
        public void MarkAsActive(int totalFiles, long totalSizeBytes)
        {
            Status = BackupStatus.Active;
            Progress = new BackupProgress
            {
                TotalFiles        = totalFiles,
                TotalSizeBytes    = totalSizeBytes,
                RemainingFiles    = totalFiles,
                RemainingBytes    = totalSizeBytes,
                ProgressPercent   = 0,
                CurrentSourceFile = string.Empty,
                CurrentDestFile   = string.Empty
            };
        }

        /// <summary>
        /// Updates progress after a single file has been copied.
        /// Recalculates the completion percentage.
        /// </summary>
        /// <param name="fileSizeBytes">Size of the file just copied, in bytes.</param>
        /// <param name="currentSourceFile">UNC path of the file just copied.</param>
        /// <param name="currentDestFile">UNC path of the destination file.</param>
        public void UpdateProgress(long fileSizeBytes, string currentSourceFile, string currentDestFile)
        {
            if (Progress == null) return;

            Progress.RemainingFiles   = Math.Max(0, Progress.RemainingFiles - 1);
            Progress.RemainingBytes   = Math.Max(0, Progress.RemainingBytes - fileSizeBytes);
            Progress.CurrentSourceFile = currentSourceFile;
            Progress.CurrentDestFile   = currentDestFile;
            Progress.RecalculatePercent();
        }

        /// <summary>
        /// Marks the job as successfully completed and clears progress.
        /// If the job type is Full, updates LastFullBackupDate to now.
        /// </summary>
        public void MarkAsCompleted()
        {
            Status   = BackupStatus.Completed;
            Progress = null;

            // Only a successful full backup updates the differential reference date
            if (Type == BackupType.Full)
            {
                LastFullBackupDate = DateTime.Now;
            }
        }

        /// <summary>
        /// Marks the job as failed and clears progress.
        /// The daily log file will contain details about the failing file.
        /// </summary>
        public void MarkAsError()
        {
            Status   = BackupStatus.Error;
            Progress = null;
        }

        // ─────────────────────────────────────────────────────────────
        // Display helper
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a short human-readable summary of the backup job.
        /// Used for display in the interactive console menu.
        /// </summary>
        public string GetSummary()
        {
            return $"[{Id}] {Name} | {Type} | {SourcePath} → {TargetPath}";
        }
    }
}
