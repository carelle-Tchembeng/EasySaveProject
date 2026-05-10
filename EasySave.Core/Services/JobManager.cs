using EasySave.Core.Entities;
using EasySave.Core.Interfaces;

namespace EasySave.Core.Services
{
    /// <summary>
    /// Manages backup job configuration.
    ///
    /// Responsibilities:
    /// - Load jobs from persistent storage.
    /// - Add, update, and delete jobs.
    /// - Enforce the v1.1 limit of 5 backup jobs.
    /// - Validate source and target paths.
    /// - Persist changes through IConfigRepository.
    ///
    /// This class contains business rules related to job management.
    /// </summary>
    public class JobManager
    {
        /// <summary>
        /// Maximum number of jobs allowed in EasySave v1.1.
        /// This limit will be removed in EasySave v2.0.
        /// </summary>
        public const int MaxJobs = 5;

        private readonly IConfigRepository _configRepository;
        private readonly IFileSystem _fileSystem;

        /// <summary>
        /// In-memory job list.
        /// The list is loaded once at startup and saved after each modification.
        /// </summary>
        private List<BackupJob> _jobs;

        public JobManager(IConfigRepository configRepository, IFileSystem fileSystem)
        {
            _configRepository = configRepository;
            _fileSystem = fileSystem;

            // Load existing jobs from config.json.
            _jobs = _configRepository.Load();

            // Ensure IDs are always consistent and contiguous.
            ReindexJobs();

            // Persist the normalized state.
            Persist();
        }

        /// <summary>
        /// Returns a copy of the current job list.
        ///
        /// Returning a copy prevents external classes from replacing
        /// or directly modifying the internal list reference.
        /// </summary>
        public List<BackupJob> GetAll()
        {
            return new List<BackupJob>(_jobs);
        }

        /// <summary>
        /// Returns a job using its one-based index.
        /// </summary>
        public BackupJob GetByIndex(int index)
        {
            ValidateIndex(index);
            return _jobs[index - 1];
        }

        /// <summary>
        /// Indicates whether the v1.1 job limit has been reached.
        /// </summary>
        public bool MaxJobsReached()
        {
            return _jobs.Count >= MaxJobs;
        }

        /// <summary>
        /// Number of configured backup jobs.
        /// </summary>
        public int Count => _jobs.Count;

        /// <summary>
        /// Adds a new backup job.
        ///
        /// Throws:
        /// - InvalidOperationException if the maximum number of jobs is reached.
        /// - ArgumentException if paths are invalid.
        /// </summary>
        public void Add(
            string name,
            string sourcePath,
            string targetPath,
            Enums.BackupType type)
        {
            if (MaxJobsReached())
                throw new InvalidOperationException($"Cannot add more than {MaxJobs} backup jobs.");

            if (!ValidatePaths(sourcePath, targetPath))
                throw new ArgumentException("Source or target path does not exist or is not accessible.");

            var job = new BackupJob
            {
                Id = _jobs.Count + 1,
                Name = name.Trim(),
                SourcePath = sourcePath.Trim(),
                TargetPath = targetPath.Trim(),
                Type = type
            };

            _jobs.Add(job);
            Persist();
        }

        /// <summary>
        /// Updates an existing backup job.
        /// </summary>
        public void Update(
            int index,
            string name,
            string sourcePath,
            string targetPath,
            Enums.BackupType type)
        {
            ValidateIndex(index);

            if (!ValidatePaths(sourcePath, targetPath))
                throw new ArgumentException("Source or target path does not exist or is not accessible.");

            var job = _jobs[index - 1];

            job.Name = name.Trim();
            job.SourcePath = sourcePath.Trim();
            job.TargetPath = targetPath.Trim();
            job.Type = type;

            Persist();
        }

        /// <summary>
        /// Deletes a backup job and re-indexes the remaining jobs.
        /// </summary>
        public void Delete(int index)
        {
            ValidateIndex(index);

            _jobs.RemoveAt(index - 1);

            // Keep IDs contiguous after deletion.
            ReindexJobs();

            Persist();
        }

        /// <summary>
        /// Validates that both source and target paths exist.
        /// The actual path access logic is delegated to IFileSystem.
        /// </summary>
        public bool ValidatePaths(string sourcePath, string targetPath)
        {
            return _fileSystem.Exists(sourcePath) && _fileSystem.Exists(targetPath);
        }

        /// <summary>
        /// Persists the current job list.
        ///
        /// This public method is mainly used after backup execution,
        /// because LastFullBackupDate may have changed.
        /// </summary>
        public void Save()
        {
            Persist();
        }

        /// <summary>
        /// Ensures that a one-based job index is valid.
        /// </summary>
        private void ValidateIndex(int index)
        {
            if (index < 1 || index > _jobs.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index),
                    $"Index must be between 1 and {_jobs.Count}. Got: {index}");
            }
        }

        /// <summary>
        /// Reassigns IDs from 1 to N.
        /// This avoids gaps after deletion.
        /// </summary>
        private void ReindexJobs()
        {
            for (int i = 0; i < _jobs.Count; i++)
            {
                _jobs[i].Id = i + 1;
            }
        }

        /// <summary>
        /// Saves the complete job list to config.json.
        /// </summary>
        private void Persist()
        {
            _configRepository.Save(_jobs);
        }
    }
}
