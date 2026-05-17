// EasySave.Core/Services/JobManager.cs
// UPDATED v2.0 — MaxJobs limit removed (unlimited jobs), Id is now Guid

using EasySave.Core.Entities;
using EasySave.Core.Enums;
using EasySave.Core.Interfaces;

namespace EasySave.Core.Services
{
    /// <summary>
    /// Manages CRUD operations on backup jobs.
    /// v2.0: MaxJobs limit removed — jobs are now unlimited.
    /// Id is now Guid — no re-indexing needed on delete.
    /// </summary>
    public class JobManager
    {
        // ─────────────────────────────────────────────────────────────
        // Dependencies and state
        // ─────────────────────────────────────────────────────────────

        private readonly IConfigRepository _configRepository;
        private readonly IFileSystem       _fileSystem;
        private List<BackupJob>            _jobs;

        // ─────────────────────────────────────────────────────────────
        // Constructor
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Initializes JobManager and loads the existing job configuration.
        /// </summary>
        public JobManager(IConfigRepository configRepository, IFileSystem fileSystem)
        {
            _configRepository = configRepository;
            _fileSystem       = fileSystem;
            _jobs             = _configRepository.Load();
        }

        // ─────────────────────────────────────────────────────────────
        // Read operations
        // ─────────────────────────────────────────────────────────────

        /// <summary>Returns a copy of all configured backup jobs.</summary>
        public List<BackupJob> GetAll() => new List<BackupJob>(_jobs);

        /// <summary>
        /// Returns the backup job with the given Guid.
        /// Throws KeyNotFoundException if not found.
        /// </summary>
        public BackupJob GetById(Guid id)
        {
            var job = _jobs.FirstOrDefault(j => j.Id == id);
            if (job == null)
                throw new KeyNotFoundException($"No backup job found with Id: {id}");
            return job;
        }

        /// <summary>Returns the number of currently configured jobs.</summary>
        public int Count => _jobs.Count;

        // ─────────────────────────────────────────────────────────────
        // Write operations
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Adds a new backup job.
        /// v2.0: No limit on number of jobs.
        /// Throws ArgumentException if paths are invalid.
        /// </summary>
        public void Add(string name, string sourcePath, string targetPath, BackupType type)
        {
            if (!ValidatePaths(sourcePath, targetPath))
                throw new ArgumentException("Source or target path does not exist or is not accessible.");

            var job = new BackupJob
            {
                Id         = Guid.NewGuid(),
                Name       = name.Trim(),
                SourcePath = sourcePath.Trim(),
                TargetPath = targetPath.Trim(),
                Type       = type
            };

            _jobs.Add(job);
            Persist();
        }

        /// <summary>
        /// Updates an existing backup job identified by Guid.
        /// Throws KeyNotFoundException if not found.
        /// Throws ArgumentException if paths are invalid.
        /// </summary>
        public void Update(Guid id, string name, string sourcePath, string targetPath, BackupType type)
        {
            var job = GetById(id);

            if (!ValidatePaths(sourcePath, targetPath))
                throw new ArgumentException("Source or target path does not exist or is not accessible.");

            job.Name       = name.Trim();
            job.SourcePath = sourcePath.Trim();
            job.TargetPath = targetPath.Trim();
            job.Type       = type;

            Persist();
        }

        /// <summary>
        /// Deletes the backup job with the given Guid.
        /// v2.0: No re-indexing needed since Id is Guid.
        /// Throws KeyNotFoundException if not found.
        /// </summary>
        public void Delete(Guid id)
        {
            var job = GetById(id);
            _jobs.Remove(job);
            Persist();
        }

        // ─────────────────────────────────────────────────────────────
        // Validation
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns true if both source and target paths exist and are accessible.
        /// </summary>
        public bool ValidatePaths(string sourcePath, string targetPath)
            => _fileSystem.Exists(sourcePath) && _fileSystem.Exists(targetPath);

        // ─────────────────────────────────────────────────────────────
        // Private helpers
        // ─────────────────────────────────────────────────────────────

        private void Persist() => _configRepository.Save(_jobs);
    }
}
