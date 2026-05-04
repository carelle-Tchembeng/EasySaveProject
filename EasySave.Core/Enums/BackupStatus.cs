// EasySave.Core/Enums/BackupStatus.cs

namespace EasySave.Core.Enums
{
    /// <summary>
    /// Represents the current execution state of a backup job.
    /// This value is written in real time to the state file (state.json).
    /// </summary>
    public enum BackupStatus
    {
        /// <summary>
        /// The backup job exists in configuration but is not currently running.
        /// This is the default state after creation or after a completed execution.
        /// </summary>
        Inactive = 0,

        /// <summary>
        /// The backup job is currently being executed.
        /// Files are being copied and progress is being tracked.
        /// </summary>
        Active = 1,

        /// <summary>
        /// The backup job has finished executing successfully.
        /// All eligible files have been copied to the target directory.
        /// </summary>
        Completed = 2,

        /// <summary>
        /// The backup job encountered a critical error during execution.
        /// At least one file could not be copied. Check the daily log file for details.
        /// </summary>
        Error = 3
    }
}
