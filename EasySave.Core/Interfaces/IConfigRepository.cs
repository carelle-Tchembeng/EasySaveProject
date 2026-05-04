// EasySave.Core/Interfaces/IConfigRepository.cs

using EasySave.Core.Entities;

namespace EasySave.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for persisting and loading backup job configurations.
    /// Implemented by JsonConfigRepository in the Infrastructure layer.
    /// Configuration is stored in config.json under the application data directory.
    /// </summary>
    public interface IConfigRepository
    {
        /// <summary>
        /// Loads the list of backup jobs from persistent storage.
        /// Returns an empty list if no configuration file exists yet.
        /// </summary>
        /// <returns>List of configured backup jobs. Never null.</returns>
        List<BackupJob> Load();

        /// <summary>
        /// Saves the complete list of backup jobs to persistent storage.
        /// Overwrites the existing configuration file entirely.
        /// </summary>
        /// <param name="jobs">List of backup jobs to persist.</param>
        void Save(List<BackupJob> jobs);
    }
}
