using EasySave.Core.Enums;
using EasySave.Core.ValueObjects;
using System.Text.Json.Serialization;

namespace EasySave.Core.Entities
{
    /// <summary>
    /// Represents a backup job configured by the user.
    ///
    /// This class contains two kinds of data:
    /// 1. Persistent configuration data:
    ///    - Id
    ///    - Name
    ///    - SourcePath
    ///    - TargetPath
    ///    - Type
    ///    - LastFullBackupDate
    ///
    /// 2. Runtime execution data:
    ///    - Status
    ///    - Progress
    ///
    /// Runtime data must not be stored in config.json.
    /// It is only written to state.json during execution.
    /// </summary>
    public class BackupJob
    {
        // --------------------------------------------------------------------
        // Persistent configuration properties
        // --------------------------------------------------------------------

        /// <summary>
        /// One-based identifier of the backup job.
        /// In version 1.1, the application still supports a maximum of 5 jobs.
        /// This Id is used by the console menu and by command-line arguments.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// User-friendly name of the backup job.
        /// Example: "Documents backup".
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Absolute path of the source directory.
        /// Can be a local path, an external drive path, or a UNC network path.
        /// </summary>
        public string SourcePath { get; set; } = string.Empty;

        /// <summary>
        /// Absolute path of the target directory.
        /// The directory must be accessible before the job can be saved.
        /// </summary>
        public string TargetPath { get; set; } = string.Empty;

        /// <summary>
        /// Backup type selected by the user.
        /// Full: copy every file.
        /// Differential: copy only files modified since the last full backup.
        /// </summary>
        public BackupType Type { get; set; } = BackupType.Full;

        /// <summary>
        /// Date of the last successful full backup.
        ///
        /// This value is required for differential backups.
        /// If a differential backup is launched and this value is null,
        /// EasySave automatically performs a full backup first.
        /// </summary>
        public DateTime? LastFullBackupDate { get; set; } = null;

        // --------------------------------------------------------------------
        // Runtime-only properties
        // --------------------------------------------------------------------
        // These properties are ignored when config.json is written.
        // They are only used while a backup is running and are written to state.json.

        /// <summary>
        /// Current runtime status of the job.
        /// Ignored in config.json because this is execution state, not configuration.
        /// </summary>
        [JsonIgnore]
        public BackupStatus Status { get; set; } = BackupStatus.Inactive;

        /// <summary>
        /// Current runtime progress of the job.
        /// Null when the job is inactive, completed, or failed.
        /// Ignored in config.json because it is written separately to state.json.
        /// </summary>
        [JsonIgnore]
        public BackupProgress? Progress { get; set; } = null;

        // --------------------------------------------------------------------
        // Runtime state methods
        // --------------------------------------------------------------------

        /// <summary>
        /// Resets the job to its default inactive runtime state.
        /// This is called before a new execution starts and also when state.json
        /// is cleared on application startup.
        /// </summary>
        public void ResetState()
        {
            Status = BackupStatus.Inactive;
            Progress = null;
        }

        /// <summary>
        /// Marks the job as active and initializes its progress object.
        /// </summary>
        /// <param name="totalFiles">Number of files that are eligible for backup.</param>
        /// <param name="totalSizeBytes">Total size in bytes of eligible files.</param>
        public void MarkAsActive(int totalFiles, long totalSizeBytes)
        {
            Status = BackupStatus.Active;

            Progress = new BackupProgress
            {
                TotalFiles = totalFiles,
                TotalSizeBytes = totalSizeBytes,
                RemainingFiles = totalFiles,
                RemainingBytes = totalSizeBytes,

                // If there are no files to copy, the job is already logically complete.
                ProgressPercent = totalFiles == 0 ? 100 : 0,

                CurrentSourceFile = string.Empty,
                CurrentDestFile = string.Empty
            };
        }

        /// <summary>
        /// Updates the progress information after a file has been processed.
        /// This method is called after both successful and failed copy attempts,
        /// because the file is no longer pending after the attempt.
        /// </summary>
        /// <param name="fileSizeBytes">Size of the processed file.</param>
        /// <param name="currentSourceFile">UNC source file path.</param>
        /// <param name="currentDestFile">UNC destination file path.</param>
        public void UpdateProgress(
            long fileSizeBytes,
            string currentSourceFile,
            string currentDestFile)
        {
            if (Progress == null)
                return;

            Progress.RemainingFiles = Math.Max(0, Progress.RemainingFiles - 1);
            Progress.RemainingBytes = Math.Max(0, Progress.RemainingBytes - fileSizeBytes);
            Progress.CurrentSourceFile = currentSourceFile;
            Progress.CurrentDestFile = currentDestFile;

            Progress.RecalculatePercent();
        }

        /// <summary>
        /// Marks the job as completed.
        ///
        /// The parameter updateFullBackupDate is used when the effective execution
        /// was a full backup, even if the configured job type is Differential.
        /// This happens when a differential job has never had a full backup before.
        /// </summary>
        /// <param name="updateFullBackupDate">
        /// True when LastFullBackupDate must be updated after completion.
        /// </param>
        public void MarkAsCompleted(bool updateFullBackupDate = false)
        {
            Status = BackupStatus.Completed;
            Progress = null;

            if (Type == BackupType.Full || updateFullBackupDate)
            {
                LastFullBackupDate = DateTime.Now;
            }
        }

        /// <summary>
        /// Marks the job as failed.
        /// The detailed reason for the failure is expected to be available
        /// in the daily log file.
        /// </summary>
        public void MarkAsError()
        {
            Status = BackupStatus.Error;
            Progress = null;
        }

        /// <summary>
        /// Returns a compact textual summary used by the console interface.
        /// </summary>
        public string GetSummary()
        {
            return $"[{Id}] {Name} | {Type} | {SourcePath} → {TargetPath}";
        }
    }
}
