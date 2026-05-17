// EasySave.Infrastructure/Repositories/JsonConfigRepository.cs — UPDATED v2.0 (Guid ids)
using EasySave.Core.Entities;
using EasySave.Core.Interfaces;
using EasySave.Infrastructure.Helpers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasySave.Infrastructure.Repositories
{
    public class JsonConfigRepository : IConfigRepository
    {
        private readonly string _configFilePath;
        private readonly JsonSerializerOptions _options;

        public JsonConfigRepository(string configFilePath)
        {
            _configFilePath = configFilePath;
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        public List<BackupJob> Load()
        {
            if (!File.Exists(_configFilePath)) return new List<BackupJob>();
            try
            {
                string json = File.ReadAllText(_configFilePath, System.Text.Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(json)) return new List<BackupJob>();
                return JsonSerializer.Deserialize<List<BackupJob>>(json, _options) ?? new List<BackupJob>();
            }
            catch { return new List<BackupJob>(); }
        }

        public void Save(List<BackupJob> jobs)
        {
            PathHelper.EnsureParentDirectory(_configFilePath);
            File.WriteAllText(_configFilePath, JsonSerializer.Serialize(jobs, _options), System.Text.Encoding.UTF8);
        }
    }
}
