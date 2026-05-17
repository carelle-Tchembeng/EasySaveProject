// EasySave.Core/Entities/BackupJob.cs
// UPDATED v2.0 — Id is now Guid per corrected diagram

using EasySave.Core.Enums;
using EasySave.Core.ValueObjects;

namespace EasySave.Core.Entities
{
    /// <summary>
    /// Represents a backup job configured by the user.
    /// v2.0: Id changed from int to Guid — jobs are now unlimited.
    /// </summary>
    public class BackupJob
    {
        // ─── Configuration (persisted in config.json) ────────────────

        /// <summary>Unique identifier. Guid allows unlimited jobs without index conflicts.</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>User-defined name. Example: "Backup_Docs"</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Full path to the source directory (local, external, or UNC).</summary>
        public string SourcePath { get; set; } = string.Empty;

        /// <summary>Full path to the target directory (local, external, or UNC).</summary>
        public string TargetPath { get; set; } = string.Empty;

        /// <summary>Backup strategy: Full or Differential.</summary>
        public BackupType Type { get; set; } = BackupType.Full;

        /// <summary>
        /// Date of the last successful full backup.
        /// Null if no full backup has ever been performed for this job.
        /// Used by DifferentialBackupStrategy to filter files.
        /// </summary>
        public DateTime? LastFullBackupDate { get; set; } = null;

        // ─── Runtime state (written to state.json, NOT to config.json) ─

        /// <summary>Current execution status. Defaults to Inactive.</summary>
        public BackupStatus Status { get; set; } = BackupStatus.Inactive;

        /// <summary>Real-time progress. Null when not active.</summary>
        public BackupProgress? Progress { get; set; } = null;

        // ─── State transitions ─────────────────────────────────────────

        /// <summary>Resets runtime state to default inactive values.</summary>
        public void ResetState()
        {
            Status   = BackupStatus.Inactive;
            Progress = null;
        }

        /// <summary>
        /// Marks the job as active and initializes progress tracking.
        /// </summary>
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
        /// Marks the job as completed.
        /// Updates LastFullBackupDate if the type is Full.
        /// </summary>
        public void MarkAsCompleted()
        {
            Status   = BackupStatus.Completed;
            Progress = null;
            if (Type == BackupType.Full)
                LastFullBackupDate = DateTime.Now;
        }

        /// <summary>Marks the job as failed (copy error or business software interruption).</summary>
        public void MarkAsError()
        {
            Status   = BackupStatus.Error;
            Progress = null;
        }

        /// <summary>
        /// Updates progress after a single file has been copied.
        /// </summary>
        public void UpdateProgress(long fileSizeBytes, string currentSourceFile, string currentDestFile)
        {
            if (Progress == null) return;
            Progress.RemainingFiles    = Math.Max(0, Progress.RemainingFiles - 1);
            Progress.RemainingBytes    = Math.Max(0, Progress.RemainingBytes - fileSizeBytes);
            Progress.CurrentSourceFile = currentSourceFile;
            Progress.CurrentDestFile   = currentDestFile;
            Progress.RecalculatePercent();
        }

        /// <summary>Returns a short human-readable summary for UI display.</summary>
        public string GetSummary()
            => $"{Name} | {Type} | {SourcePath} → {TargetPath}";
    }
}
