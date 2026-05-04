// EasySave.Core/Services/BackupService.cs

using EasySave.Core.Entities;
using EasySave.Core.Enums;
using EasySave.Core.Interfaces;
using EasySave.Core.ValueObjects;

namespace EasySave.Core.Services
{
    /// <summary>
    /// Orchestrates the execution of backup jobs.
    /// Resolves the correct strategy (Full or Differential),
    /// tracks progress, updates state.json, and writes log entries.
    /// </summary>
    public class BackupService
    {
        // ─────────────────────────────────────────────────────────────
        // Dependencies
        // ─────────────────────────────────────────────────────────────

        private readonly JobManager         _jobManager;
        private readonly IFileSystem        _fileSystem;
        private readonly IStateRepository   _stateRepository;
        private readonly ILogger            _logger;
        private readonly IBackupStrategy    _fullStrategy;
        private readonly IBackupStrategy    _differentialStrategy;

        // ─────────────────────────────────────────────────────────────
        // Constructor
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Initializes BackupService with all required dependencies.
        /// All parameters are injected — BackupService has no knowledge
        /// of concrete implementations.
        /// </summary>
        public BackupService(
            JobManager       jobManager,
            IFileSystem      fileSystem,
            IStateRepository stateRepository,
            ILogger          logger,
            IBackupStrategy  fullStrategy,
            IBackupStrategy  differentialStrategy)
        {
            _jobManager           = jobManager;
            _fileSystem           = fileSystem;
            _stateRepository      = stateRepository;
            _logger               = logger;
            _fullStrategy         = fullStrategy;
            _differentialStrategy = differentialStrategy;
        }

        // ─────────────────────────────────────────────────────────────
        // Public execution methods
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Executes a single backup job identified by its 1-based index.
        /// </summary>
        /// <param name="index">1-based index of the job to execute.</param>
        public void ExecuteOne(int index)
        {
            var job = _jobManager.GetByIndex(index);
            ExecuteJob(job);
        }

        /// <summary>
        /// Executes all configured backup jobs sequentially, in order.
        /// </summary>
        public void ExecuteAll()
        {
            var jobs = _jobManager.GetAll();
            foreach (var job in jobs)
            {
                ExecuteJob(job);
            }
        }

        /// <summary>
        /// Executes backup jobs for a contiguous range of 1-based indices.
        /// Example: ExecuteRange(1, 3) executes jobs 1, 2, and 3.
        /// </summary>
        /// <param name="from">First index in the range (inclusive).</param>
        /// <param name="to">Last index in the range (inclusive).</param>
        public void ExecuteRange(int from, int to)
        {
            var indices = Enumerable.Range(from, to - from + 1).ToList();
            ExecuteList(indices);
        }

        /// <summary>
        /// Executes backup jobs for an explicit list of 1-based indices.
        /// Example: ExecuteList([1, 3]) executes jobs 1 and 3.
        /// </summary>
        /// <param name="indices">List of 1-based job indices to execute.</param>
        public void ExecuteList(List<int> indices)
        {
            foreach (int index in indices)
            {
                ExecuteOne(index);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Core execution logic
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Executes a single backup job:
        /// 1. Resets and activates the job state
        /// 2. Resolves the appropriate strategy
        /// 3. Runs the strategy with a progress callback
        /// 4. Marks the job as completed or error
        /// 5. Persists final state
        /// </summary>
        private void ExecuteJob(BackupJob job)
        {
            var allJobs = _jobManager.GetAll();

            try
            {
                // Reset any stale state from a previous run
                job.ResetState();

                // Resolve strategy — switch to Full if differential has no reference date
                IBackupStrategy strategy = ResolveStrategy(job);

                // Count eligible files to initialize progress tracking
                var files = _fileSystem.GetFiles(job.SourcePath);
                long totalSize = files.Sum(f => _fileSystem.GetFileSize(f));
                job.MarkAsActive(files.Count, totalSize);

                // Write initial Active state to state.json
                UpdateStateForAllJobs();

                // Execute the strategy — callback is called after each file
                strategy.Execute(job, _fileSystem, _logger, OnFileCopied(job));

                // Mark as completed and update LastFullBackupDate if needed
                job.MarkAsCompleted();
            }
            catch (Exception)
            {
                // Mark as error so state.json reflects the failure
                job.MarkAsError();
            }
            finally
            {
                // Always write the final state (Completed or Error)
                UpdateStateForAllJobs();
            }
        }

        /// <summary>
        /// Builds the callback Action passed to the strategy.
        /// Called after each file is copied (or fails to copy).
        /// Updates job progress and writes to state.json and log.
        /// </summary>
        /// <param name="job">The job currently being executed.</param>
        private Action<string, string, long, long> OnFileCopied(BackupJob job)
        {
            return (sourceFile, destFile, fileSizeBytes, transferTimeMs) =>
            {
                // Update in-memory progress on the job entity
                job.UpdateProgress(fileSizeBytes, sourceFile, destFile);

                // Write updated state to state.json in real time
                UpdateStateForAllJobs();

                // Write log entry via EasyLog (success or failure)
                LogEntry entry = transferTimeMs >= 0
                    ? LogEntry.Success(job.Name, sourceFile, destFile, fileSizeBytes, transferTimeMs)
                    : LogEntry.Failure(job.Name, sourceFile, destFile, fileSizeBytes);

                _logger.Log(entry);
            };
        }

        /// <summary>
        /// Resolves the backup strategy for the given job.
        /// If the job type is Differential but no full backup has been performed yet,
        /// falls back to FullBackupStrategy automatically.
        /// </summary>
        /// <param name="job">The job for which to resolve the strategy.</param>
        /// <returns>The appropriate IBackupStrategy instance.</returns>
        private IBackupStrategy ResolveStrategy(BackupJob job)
        {
            if (job.Type == BackupType.Differential && job.LastFullBackupDate == null)
            {
                // No reference date available — force a full backup first
                // Temporarily change type so MarkAsCompleted() sets LastFullBackupDate
                job.Type = BackupType.Full;
                return _fullStrategy;
            }

            return job.Type == BackupType.Full ? _fullStrategy : _differentialStrategy;
        }

        /// <summary>
        /// Writes the current state of all jobs to state.json.
        /// Fetches the full list from JobManager to include inactive jobs.
        /// </summary>
        private void UpdateStateForAllJobs()
        {
            var allJobs = _jobManager.GetAll();
            _stateRepository.Update(allJobs);
        }
    }
}
