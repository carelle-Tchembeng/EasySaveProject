// EasySave.Core/Enums/BackupStatus.cs
// UPDATED v3.0 — added Paused state for parallel pause/resume support

namespace EasySave.Core.Enums
{
    /// <summary>
    /// Represents the current execution state of a backup job.
    /// Written in real time to state.json via IStateRepository.
    /// </summary>
    public enum BackupStatus
    {
        /// <summary>Default state — configured but not running.</summary>
        Inactive = 0,

        /// <summary>Job is currently executing.</summary>
        Active = 1,

        /// <summary>
        /// NEW v3.0 — Job is paused (business software detected or user-initiated).
        /// Resumes automatically when business software stops.
        /// </summary>
        Paused = 2,

        /// <summary>Last execution finished successfully.</summary>
        Completed = 3,

        /// <summary>
        /// Last execution failed or was stopped by user.
        /// Check the daily log file for details.
        /// </summary>
        Error = 4
    }
}
