// EasySave.Core/Enums/BackupType.cs

namespace EasySave.Core.Enums
{
    /// <summary>
    /// Defines the type of backup strategy to apply when executing a backup job.
    /// </summary>
    public enum BackupType
    {
        /// <summary>
        /// All files from the source directory are copied to the target,
        /// regardless of their modification date.
        /// </summary>
        Full = 0,

        /// <summary>
        /// Only files modified since the last full backup are copied to the target.
        /// If no full backup has ever been performed, a full backup is executed instead.
        /// </summary>
        Differential = 1
    }
}
