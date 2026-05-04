// EasySave.Infrastructure/Repositories/JsonConfigRepository.cs

using EasySave.Core.Entities;
using EasySave.Core.Interfaces;
using EasySave.Infrastructure.Helpers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasySave.Infrastructure.Repositories
{
    /// <summary>
    /// Persists and loads backup job configurations using a JSON file.
    /// The file is stored in the application data directory (never in temp folders).
    /// Runtime-only properties (Status, Progress) are excluded from serialization
    /// using [JsonIgnore] to keep the config file clean and stable.
    /// </summary>
    public class JsonConfigRepository : IConfigRepository
    {
        // ─────────────────────────────────────────────────────────────
        // Configuration
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Full path to the config.json file.
        /// Example: C:\ProgramData\EasySave\config.json
        /// </summary>
        private readonly string _configFilePath;

        /// <summary>
        /// JSON serializer options — indented for Notepad readability.
        /// </summary>
        private readonly JsonSerializerOptions _serializerOptions;

        // ─────────────────────────────────────────────────────────────
        // Constructor
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Initializes the repository with the target config file path.
        /// </summary>
        /// <param name="configFilePath">
        /// Full absolute path to the config.json file.
        /// Provided by AppSettings to avoid hardcoded paths.
        /// </param>
        public JsonConfigRepository(string configFilePath)
        {
            _configFilePath    = configFilePath;
            _serializerOptions = BuildSerializerOptions();
        }

        // ─────────────────────────────────────────────────────────────
        // IConfigRepository implementation
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Loads the list of backup jobs from config.json.
        /// Returns an empty list if the file does not exist yet (first launch).
        /// Returns an empty list if the file is malformed (fail-safe).
        /// </summary>
        /// <returns>List of configured BackupJob instances. Never null.</returns>
        public List<BackupJob> Load()
        {
            if (!File.Exists(_configFilePath))
                return new List<BackupJob>();

            try
            {
                string json = File.ReadAllText(_configFilePath, System.Text.Encoding.UTF8);

                if (string.IsNullOrWhiteSpace(json))
                    return new List<BackupJob>();

                return JsonSerializer.Deserialize<List<BackupJob>>(json, _serializerOptions)
                    ?? new List<BackupJob>();
            }
            catch (Exception)
            {
                // Return empty list rather than crashing on a corrupted config file
                return new List<BackupJob>();
            }
        }

        /// <summary>
        /// Saves the complete list of backup jobs to config.json.
        /// Overwrites the file entirely on every save.
        /// Creates the parent directory if it does not exist.
        /// </summary>
        /// <param name="jobs">Complete list of backup jobs to persist.</param>
        public void Save(List<BackupJob> jobs)
        {
            // Ensure the directory exists before writing
            PathHelper.EnsureParentDirectory(_configFilePath);

            string json = JsonSerializer.Serialize(jobs, _serializerOptions);
            File.WriteAllText(_configFilePath, json, System.Text.Encoding.UTF8);
        }

        // ─────────────────────────────────────────────────────────────
        // Private helpers
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Builds the JSON serializer options used for config.json.
        /// WriteIndented is true for Notepad readability.
        /// Enum values are serialized as strings for human readability.
        /// </summary>
        private static JsonSerializerOptions BuildSerializerOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented        = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters           = { new JsonStringEnumConverter() }
            };
        }
    }
}
