using EasySave.Infrastructure.Helpers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasySave.Infrastructure.Configuration
{
    /// <summary>
    /// Represents global application settings.
    ///
    /// In EasySave v1.1, this class is mainly used to store:
    /// - configuration file path
    /// - state file path
    /// - log directory
    /// - selected log format: JSON or XML
    /// - selected language
    ///
    /// Settings are stored in ProgramData to avoid hardcoded temporary paths
    /// and to remain suitable for client/server environments.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Path of the settings file itself.
        ///
        /// This property is ignored during serialization because it is metadata,
        /// not an actual configurable setting.
        /// </summary>
        [JsonIgnore]
        public string SettingsFilePath { get; private set; } = PathHelper.GetSettingsPath();

        /// <summary>
        /// Full path to the backup jobs configuration file.
        /// </summary>
        public string ConfigFilePath { get; set; } = PathHelper.GetConfigPath();

        /// <summary>
        /// Full path to the real-time state file.
        /// </summary>
        public string StateFilePath { get; set; } = PathHelper.GetStatePath();

        /// <summary>
        /// Directory where daily log files are created.
        /// </summary>
        public string LogDirectory { get; set; } = PathHelper.GetLogDirectory();

        /// <summary>
        /// Selected log format.
        /// Supported values: "Json" and "Xml".
        /// Default is Json for backward compatibility with EasySave v1.0.
        /// </summary>
        public string LogFormat { get; set; } = "Json";

        /// <summary>
        /// Preferred user interface language.
        /// Supported values: "fr" and "en".
        /// </summary>
        public string Language { get; set; } = "en";

        /// <summary>
        /// Loads settings from the default ProgramData location.
        /// </summary>
        public static AppSettings LoadDefault()
        {
            return Load(PathHelper.GetSettingsPath());
        }

        /// <summary>
        /// Loads settings from the specified file path.
        ///
        /// If the file does not exist, a default one is created automatically.
        /// If the file is empty or corrupted, safe defaults are used.
        /// </summary>
        public static AppSettings Load(string settingsFilePath)
        {
            try
            {
                PathHelper.EnsureParentDirectory(settingsFilePath);

                if (!File.Exists(settingsFilePath))
                {
                    var defaultSettings = new AppSettings
                    {
                        SettingsFilePath = settingsFilePath
                    };

                    defaultSettings.ApplyDefaults();
                    defaultSettings.Save();

                    return defaultSettings;
                }

                string json = File.ReadAllText(settingsFilePath, System.Text.Encoding.UTF8);

                if (string.IsNullOrWhiteSpace(json))
                {
                    var emptySettings = new AppSettings
                    {
                        SettingsFilePath = settingsFilePath
                    };

                    emptySettings.ApplyDefaults();
                    emptySettings.Save();

                    return emptySettings;
                }

                var options = BuildSerializerOptions();

                var settings = JsonSerializer.Deserialize<AppSettings>(json, options)
                    ?? new AppSettings();

                settings.SettingsFilePath = settingsFilePath;
                settings.ApplyDefaults();
                settings.Save();

                return settings;
            }
            catch
            {
                // Fail-safe behavior:
                // EasySave should still start even if the settings file is corrupted.
                var fallbackSettings = new AppSettings
                {
                    SettingsFilePath = settingsFilePath
                };

                fallbackSettings.ApplyDefaults();
                fallbackSettings.Save();

                return fallbackSettings;
            }
        }

        /// <summary>
        /// Saves settings to their current file path.
        /// </summary>
        public void Save()
        {
            SaveAs(SettingsFilePath);
        }

        /// <summary>
        /// Saves settings to a specific file path.
        /// </summary>
        public void SaveAs(string settingsFilePath)
        {
            SettingsFilePath = settingsFilePath;

            PathHelper.EnsureParentDirectory(settingsFilePath);

            var options = BuildSerializerOptions();
            string json = JsonSerializer.Serialize(this, options);

            File.WriteAllText(settingsFilePath, json, System.Text.Encoding.UTF8);
        }

        /// <summary>
        /// Updates the selected log format and persists the change.
        /// Invalid values automatically fall back to Json.
        /// </summary>
        public void SetLogFormat(string format)
        {
            if (string.Equals(format, "Xml", StringComparison.OrdinalIgnoreCase))
            {
                LogFormat = "Xml";
            }
            else
            {
                LogFormat = "Json";
            }

            Save();
        }

        /// <summary>
        /// Ensures all settings have safe default values.
        /// </summary>
        private void ApplyDefaults()
        {
            if (string.IsNullOrWhiteSpace(ConfigFilePath))
                ConfigFilePath = PathHelper.GetConfigPath();

            if (string.IsNullOrWhiteSpace(StateFilePath))
                StateFilePath = PathHelper.GetStatePath();

            if (string.IsNullOrWhiteSpace(LogDirectory))
                LogDirectory = PathHelper.GetLogDirectory();

            if (string.IsNullOrWhiteSpace(LogFormat))
                LogFormat = "Json";

            if (!string.Equals(LogFormat, "Json", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(LogFormat, "Xml", StringComparison.OrdinalIgnoreCase))
            {
                LogFormat = "Json";
            }

            if (string.IsNullOrWhiteSpace(Language))
                Language = "en";
        }

        /// <summary>
        /// Builds JSON serialization options for settings.json.
        /// Indentation is enabled for readability in Notepad.
        /// </summary>
        private static JsonSerializerOptions BuildSerializerOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }
    }
}
