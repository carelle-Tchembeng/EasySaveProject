// EasySave.Infrastructure/Repositories/JsonStateRepository.cs — unchanged from v1.1
using EasySave.Core.Entities;
using EasySave.Core.Interfaces;
using EasySave.Infrastructure.Helpers;
using System;
using System.IO;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;

namespace EasySave.Infrastructure.Repositories
{
    public class JsonStateRepository : IStateRepository
    {
        private readonly string _stateFilePath;
        private readonly string _tempFilePath;
        private readonly JsonSerializerOptions _options;

        public JsonStateRepository(string stateFilePath)
        {
            // 🛠️ CORRECTION ICI : Si le chemin est vide, on force la valeur par défaut
            if (string.IsNullOrWhiteSpace(stateFilePath))
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                _stateFilePath = Path.Combine(appData, "EasySave", "state.json");
            }
            else
            {
                _stateFilePath = stateFilePath;
            }

            // On génère le fichier temporaire à partir du chemin sécurisé
            _tempFilePath = Path.ChangeExtension(_stateFilePath, ".tmp");
            _options = new JsonSerializerOptions { WriteIndented = true };
        }

        public void Update(List<BackupJob> jobs)
        {
            PathHelper.EnsureParentDirectory(_stateFilePath);
            var entries = jobs.Select(MapToStateEntry).ToList();
            string json = JsonSerializer.Serialize(entries, _options);

            File.WriteAllText(_tempFilePath, json, System.Text.Encoding.UTF8);
            File.Move(_tempFilePath, _stateFilePath, overwrite: true);
        }

        public void Clear(List<BackupJob> jobs)
        {
            foreach (var job in jobs) job.ResetState();
            Update(jobs);
        }

        private static StateEntry MapToStateEntry(BackupJob job)
        {
            var e = new StateEntry
            {
                JobName = job.Name,
                LastActionTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                Status = job.Status.ToString()
            };
            if (job.Progress != null)
            {
                e.TotalFiles = job.Progress.TotalFiles;
                e.TotalSizeBytes = job.Progress.TotalSizeBytes;
                e.RemainingFiles = job.Progress.RemainingFiles;
                e.RemainingBytes = job.Progress.RemainingBytes;
                e.ProgressPercent = job.Progress.ProgressPercent;
                e.CurrentSourceFile = job.Progress.CurrentSourceFile;
                e.CurrentDestFile = job.Progress.CurrentDestFile;
            }
            return e;
        }
    }
}