// EasySave.Infrastructure/Configuration/AppSettings.cs

using System.Text.Json;

namespace EasySave.Infrastructure.Configuration
{
    /// <summary>
    /// Holds all application-level configuration values.
    /// Loaded once at startup from appsettings.json.
    /// All paths are computed relative to the application data directory
    /// to ensure compatibility with customer server environments.
    /// Temp paths (e.g. C:\temp) are never used.
    /// </summary>
    public class AppSettings
    {
        // ─────────────────────────────────────────────────────────────
        // Default paths — relative to %ProgramData%\EasySave
        // ─────────────────────────────────────────────────────────────

        private static readonly string BaseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "EasySave");

        // ─────────────────────────────────────────────────────────────
        // Properties
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Full path to the backup job configuration file.
        /// Default: %ProgramData%\EasySave\config.json
        /// </summary>
        public string ConfigFilePath { get; set; } = Path.Combine(BaseDirectory, "config.json");

        /// <summary>
        /// Full path to the real-time state file.
        /// Default: %ProgramData%\EasySave\state.json
        /// </summary>
        public string StateFilePath { get; set; } = Path.Combine(BaseDirectory, "state.json");

        /// <summary>
        /// Full path to the directory containing daily log files.
        /// Default: %ProgramData%\EasySave\logs
        /// </summary>
        public string LogDirectory { get; set; } = Path.Combine(BaseDirectory, "logs");

        /// <summary>
        /// Default language code used if system language detection fails.
        /// Supported values: "fr", "en"
        /// </summary>
        public string DefaultLanguage { get; set; } = "en";

        // ─────────────────────────────────────────────────────────────
        // Factory method
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Loads application settings from the specified JSON file.
        /// If the file does not exist, returns an instance with default values.
        /// If the file is malformed, logs a warning and returns defaults.
        /// </summary>
        /// <param name="settingsFilePath">
        /// Path to the appsettings.json file.
        /// Typically located next to the executable.
        /// </param>
        /// <returns>Populated AppSettings instance. Never null.</returns>
        public static AppSettings Load(string settingsFilePath)
        {
            if (!File.Exists(settingsFilePath))
                return new AppSettings();

            try
            {
                string json = File.ReadAllText(settingsFilePath, System.Text.Encoding.UTF8);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<AppSettings>(json, options)
                    ?? new AppSettings();
            }
            catch (Exception)
            {
                // Return defaults rather than crashing on a malformed settings file
                return new AppSettings();
            }
        }

        /// <summary>
        /// Returns an AppSettings instance with all default values.
        /// Equivalent to calling Load() with a non-existent file path.
        /// </summary>
        public static AppSettings Default() => new AppSettings();
    }
}
