// EasySave.Infrastructure/Configuration/AppSettings.cs — UPDATED v2.0: + AppConfigFilePath
using System.Text.Json;
namespace EasySave.Infrastructure.Configuration
{
    public class AppSettings
    {
        private static readonly string Base = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EasySave");

        public string ConfigFilePath    { get; set; } = Path.Combine(Base, "config.json");
        public string StateFilePath     { get; set; } = Path.Combine(Base, "state.json");
        public string LogDirectory      { get; set; } = Path.Combine(Base, "logs");
        public string AppConfigFilePath { get; set; } = Path.Combine(Base, "appconfig.json"); // NEW v2.0
        public string DefaultLanguage   { get; set; } = "en";

        public static AppSettings Load(string settingsFilePath)
        {
            if (!File.Exists(settingsFilePath)) return new AppSettings();
            try
            {
                string json = File.ReadAllText(settingsFilePath, System.Text.Encoding.UTF8);
                return JsonSerializer.Deserialize<AppSettings>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new AppSettings();
            }
            catch { return new AppSettings(); }
        }

        public static AppSettings Default() => new AppSettings();
    }
}
