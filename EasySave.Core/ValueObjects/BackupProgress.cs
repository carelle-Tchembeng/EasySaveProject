// EasySave.Core/ValueObjects/BackupProgress.cs

namespace EasySave.Core.ValueObjects
{
    /// <summary>
    /// Holds the real-time execution progress of a backup job.
    /// Produced by BackupService after each file is copied.
    /// Written to state.json via IStateRepository.
    /// </summary>
    public class BackupProgress
    {
        /// <summary>
        /// Total number of files eligible for backup at the start of execution.
        /// Does not change during execution.
        /// </summary>
        public int TotalFiles { get; set; }

        /// <summary>
        /// Total size in bytes of all eligible files at the start of execution.
        /// Does not change during execution.
        /// </summary>
        public long TotalSizeBytes { get; set; }

        /// <summary>
        /// Number of files not yet copied.
        /// Decremented after each successful or failed file transfer.
        /// </summary>
        public int RemainingFiles { get; set; }

        /// <summary>
        /// Total size in bytes of files not yet copied.
        /// Decremented after each file transfer.
        /// </summary>
        public long RemainingBytes { get; set; }

        /// <summary>
        /// Completion percentage from 0 to 100.
        /// Calculated as: ((TotalFiles - RemainingFiles) / TotalFiles) * 100
        /// </summary>
        public int ProgressPercent { get; set; }

        /// <summary>
        /// Full UNC path of the source file currently being copied.
        /// Empty string when no transfer is in progress.
        /// </summary>
        public string CurrentSourceFile { get; set; } = string.Empty;

        /// <summary>
        /// Full UNC path of the destination file currently being copied.
        /// Empty string when no transfer is in progress.
        /// </summary>
        public string CurrentDestFile { get; set; } = string.Empty;

        /// <summary>
        /// Recalculates ProgressPercent based on TotalFiles and RemainingFiles.
        /// Call this after updating RemainingFiles.
        /// </summary>
        public void RecalculatePercent()
        {
            if (TotalFiles == 0)
            {
                ProgressPercent = 100;
                return;
            }

            int copiedFiles = TotalFiles - RemainingFiles;
            ProgressPercent = (int)Math.Round((double)copiedFiles / TotalFiles * 100);
        }
    }
}
