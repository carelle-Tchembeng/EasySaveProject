// EasySave.Infrastructure/Repositories/JsonAppConfigRepository.cs
// NEW v2.0 — persists AppConfiguration to appconfig.json
using EasySave.Core.Entities;
using EasySave.Core.Interfaces;
using EasySave.Infrastructure.Helpers;
using System.Text.Json;
using System.IO;
using System;

namespace EasySave.Infrastructure.Repositories
{
    /// <summary>
    /// Persists and loads AppConfiguration (user settings) to/from appconfig.json.
    /// Stores CryptoSoftPath, BusinessSoftwareName, EncryptedExtensions, LogFormat, DefaultLanguage.
    /// </summary>
    public class JsonAppConfigRepository : IAppConfigRepository
    {
        private readonly string _configFilePath;
        private readonly JsonSerializerOptions _options;

        public JsonAppConfigRepository(string configFilePath)
        {
            // 🛠️ CORRECTION ICI : Sécurité pour éviter le crash si le chemin est vide
            if (string.IsNullOrWhiteSpace(configFilePath))
            {
                // On force le chemin vers C:\ProgramData\EasySave\appconfig.json
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                _configFilePath = Path.Combine(appData, "EasySave", "appconfig.json");
            }
            else
            {
                _configFilePath = configFilePath;
            }

            _options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        }

        public AppConfiguration Load()
        {
            EnsureFileExists();
            try
            {
                string json = File.ReadAllText(_configFilePath, System.Text.Encoding.UTF8);
                return JsonSerializer.Deserialize<AppConfiguration>(json, _options) ?? new AppConfiguration();
            }
            catch { return new AppConfiguration(); }
        }

        public void Save(AppConfiguration config)
        {
            // 🛠️ SÉCURITÉ SUPPLÉMENTAIRE : On s'assure que le dossier parent existe bien
            string? directory = Path.GetDirectoryName(_configFilePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_configFilePath, JsonSerializer.Serialize(config, _options), System.Text.Encoding.UTF8);
        }

        /// <summary>Creates appconfig.json with defaults if it does not exist.</summary>
        private void EnsureFileExists()
        {
            if (!File.Exists(_configFilePath)) Save(new AppConfiguration());
        }
    }
}