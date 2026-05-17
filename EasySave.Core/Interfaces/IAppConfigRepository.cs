// EasySave.Core/Interfaces/IAppConfigRepository.cs
// NEW v2.0 — persistence contract for AppConfiguration

using EasySave.Core.Entities;

namespace EasySave.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for loading and saving application configuration.
    /// Implemented by JsonAppConfigRepository in the Infrastructure layer.
    /// Configuration is stored in appconfig.json.
    /// </summary>
    public interface IAppConfigRepository
    {
        /// <summary>
        /// Loads the application configuration from persistent storage.
        /// Returns a default AppConfiguration if the file does not exist.
        /// </summary>
        AppConfiguration Load();

        /// <summary>
        /// Saves the application configuration to persistent storage.
        /// Overwrites the existing file.
        /// </summary>
        void Save(AppConfiguration config);
    }
}
