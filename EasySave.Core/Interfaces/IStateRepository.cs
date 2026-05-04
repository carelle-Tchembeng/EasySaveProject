// EasySave.Core/Interfaces/IStateRepository.cs

using EasySave.Core.Entities;

namespace EasySave.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for writing real-time backup job state.
    /// Implemented by JsonStateRepository in the Infrastructure layer.
    /// State is written to a single state.json file after each file transfer.
    /// </summary>
    public interface IStateRepository
    {
        /// <summary>
        /// Overwrites state.json with the current state of all backup jobs.
        /// Called after each file is copied to reflect real-time progress.
        /// Must be written atomically to avoid corrupt state files.
        /// </summary>
        /// <param name="jobs">Complete list of all backup jobs and their current state.</param>
        void Update(List<BackupJob> jobs);

        /// <summary>
        /// Resets all job states to Inactive in state.json.
        /// Called on application startup to clear any stale active states
        /// left by a previous crash or forced termination.
        /// </summary>
        /// <param name="jobs">Complete list of all backup jobs.</param>
        void Clear(List<BackupJob> jobs);
    }
}
