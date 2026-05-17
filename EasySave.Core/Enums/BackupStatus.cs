// EasySave.Core/Enums/BackupStatus.cs
// UPDATED v2.0 — aligned with corrected diagram

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

        /// <summary>Last execution finished successfully.</summary>
        Completed = 2,

        /// <summary>
        /// Last execution failed or was interrupted by business software.
        /// Check the daily log file for details.
        /// </summary>
        Error = 3
    }
}
