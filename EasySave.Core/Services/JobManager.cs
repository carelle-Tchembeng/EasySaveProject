// EasySave.Core/Services/JobManager.cs

using EasySave.Core.Entities;
using EasySave.Core.Interfaces;

namespace EasySave.Core.Services
{
    /// <summary>
    /// Manages CRUD operations on backup jobs.
    /// Enforces the 5-job limit and path validation rules.
    /// Delegates persistence to IConfigRepository.
    /// </summary>
    public class JobManager
    {
        // ─────────────────────────────────────────────────────────────
        // Constants
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Maximum number of backup jobs allowed. Defined by the specification.
        /// </summary>
        public const int MaxJobs = 5;

        // ─────────────────────────────────────────────────────────────
        // Dependencies and state
        // ─────────────────────────────────────────────────────────────

        private readonly IConfigRepository _configRepository;
        private readonly IFileSystem       _fileSystem;

        /// <summary>
        /// In-memory list of configured backup jobs.
        /// Loaded from config.json on construction and kept in sync on every change.
        /// </summary>
        private List<BackupJob> _jobs;

        // ─────────────────────────────────────────────────────────────
        // Constructor
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Initializes JobManager and loads the existing job configuration.
        /// </summary>
        /// <param name="configRepository">Repository used to persist job configuration.</param>
        /// <param name="fileSystem">File system abstraction used to validate paths.</param>
        public JobManager(IConfigRepository configRepository, IFileSystem fileSystem)
        {
            _configRepository = configRepository;
            _fileSystem       = fileSystem;
            _jobs             = _configRepository.Load();
        }

        // ─────────────────────────────────────────────────────────────
        // Read operations
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a copy of all configured backup jobs.
        /// </summary>
        public List<BackupJob> GetAll() => new List<BackupJob>(_jobs);

        /// <summary>
        /// Returns the backup job at the given 1-based index.
        /// Throws ArgumentOutOfRangeException if index is invalid.
        /// </summary>
        /// <param name="index">1-based index of the job to retrieve.</param>
        public BackupJob GetByIndex(int index)
        {
            ValidateIndex(index);
            return _jobs[index - 1];
        }

        /// <summary>
        /// Returns true if the maximum number of jobs has been reached.
        /// </summary>
        public bool MaxJobsReached() => _jobs.Count >= MaxJobs;

        /// <summary>
        /// Returns the number of currently configured jobs.
        /// </summary>
        public int Count => _jobs.Count;

        // ─────────────────────────────────────────────────────────────
        // Write operations
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Adds a new backup job to the list.
        /// Throws InvalidOperationException if the 5-job limit is reached.
        /// Throws ArgumentException if paths are invalid.
        /// </summary>
        /// <param name="name">User-defined job name. Must be non-empty.</param>
        /// <param name="sourcePath">Full path to the source directory.</param>
        /// <param name="targetPath">Full path to the target directory.</param>
        /// <param name="type">Backup type: Full or Differential.</param>
        public void Add(string name, string sourcePath, string targetPath, Enums.BackupType type)
        {
            if (MaxJobsReached())
                throw new InvalidOperationException($"Cannot add more than {MaxJobs} backup jobs.");

            if (!ValidatePaths(sourcePath, targetPath))
                throw new ArgumentException("Source or target path does not exist or is not accessible.");

            var job = new BackupJob
            {
                Id         = _jobs.Count + 1,
                Name       = name.Trim(),
                SourcePath = sourcePath.Trim(),
                TargetPath = targetPath.Trim(),
                Type       = type
            };

            _jobs.Add(job);
            Persist();
        }

        /// <summary>
        /// Updates an existing backup job at the given 1-based index.
        /// Throws ArgumentOutOfRangeException if index is invalid.
        /// Throws ArgumentException if paths are invalid.
        /// </summary>
        /// <param name="index">1-based index of the job to update.</param>
        /// <param name="name">New job name.</param>
        /// <param name="sourcePath">New source path.</param>
        /// <param name="targetPath">New target path.</param>
        /// <param name="type">New backup type.</param>
        public void Update(int index, string name, string sourcePath, string targetPath, Enums.BackupType type)
        {
            ValidateIndex(index);

            if (!ValidatePaths(sourcePath, targetPath))
                throw new ArgumentException("Source or target path does not exist or is not accessible.");

            var job        = _jobs[index - 1];
            job.Name       = name.Trim();
            job.SourcePath = sourcePath.Trim();
            job.TargetPath = targetPath.Trim();
            job.Type       = type;

            Persist();
        }

        /// <summary>
        /// Deletes the backup job at the given 1-based index.
        /// Re-indexes remaining jobs to keep IDs contiguous (1 to N).
        /// Throws ArgumentOutOfRangeException if index is invalid.
        /// </summary>
        /// <param name="index">1-based index of the job to delete.</param>
        public void Delete(int index)
        {
            ValidateIndex(index);

            _jobs.RemoveAt(index - 1);

            // Re-index remaining jobs so IDs stay contiguous
            for (int i = 0; i < _jobs.Count; i++)
            {
                _jobs[i].Id = i + 1;
            }

            Persist();
        }

        // ─────────────────────────────────────────────────────────────
        // Validation
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns true if both source and target paths exist and are accessible.
        /// Checks UNC paths, local drives, and mapped/external drives.
        /// </summary>
        /// <param name="sourcePath">Source directory path to validate.</param>
        /// <param name="targetPath">Target directory path to validate.</param>
        public bool ValidatePaths(string sourcePath, string targetPath)
        {
            return _fileSystem.Exists(sourcePath) && _fileSystem.Exists(targetPath);
        }

        // ─────────────────────────────────────────────────────────────
        // Private helpers
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Throws ArgumentOutOfRangeException if the given 1-based index is out of range.
        /// </summary>
        private void ValidateIndex(int index)
        {
            if (index < 1 || index > _jobs.Count)
                throw new ArgumentOutOfRangeException(nameof(index),
                    $"Index must be between 1 and {_jobs.Count}. Got: {index}");
        }

        /// <summary>
        /// Persists the current in-memory job list to config.json.
        /// Called after every write operation (Add, Update, Delete).
        /// </summary>
        private void Persist()
        {
            _configRepository.Save(_jobs);
        }
    }
}
