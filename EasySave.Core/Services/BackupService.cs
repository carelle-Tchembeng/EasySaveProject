using EasySave.Core.Entities;
using EasySave.Core.Enums;
using EasySave.Core.Interfaces;
using EasySave.Core.ValueObjects;

namespace EasySave.Core.Services
{
    /// <summary>
    /// Orchestrates backup execution.
    ///
    /// This service does not directly perform file copy operations.
    /// Instead, it delegates the algorithm to IBackupStrategy implementations.
    ///
    /// Responsibilities:
    /// - Resolve the proper strategy.
    /// - Initialize progress information.
    /// - Update state.json in real time.
    /// - Create log entries.
    /// - Persist LastFullBackupDate after successful full backups.
    /// </summary>
    public class BackupService
    {
        private readonly JobManager _jobManager;
        private readonly IFileSystem _fileSystem;
        private readonly IStateRepository _stateRepository;
        private readonly ILogger _logger;
        private readonly IBackupStrategy _fullStrategy;
        private readonly IBackupStrategy _differentialStrategy;

        public BackupService(
            JobManager jobManager,
            IFileSystem fileSystem,
            IStateRepository stateRepository,
            ILogger logger,
            IBackupStrategy fullStrategy,
            IBackupStrategy differentialStrategy)
        {
            _jobManager = jobManager;
            _fileSystem = fileSystem;
            _stateRepository = stateRepository;
            _logger = logger;
            _fullStrategy = fullStrategy;
            _differentialStrategy = differentialStrategy;
        }

        /// <summary>
        /// Executes a single job by one-based index.
        /// </summary>
        public void ExecuteOne(int index)
        {
            var job = _jobManager.GetByIndex(index);
            ExecuteJob(job);
        }

        /// <summary>
        /// Executes all configured jobs sequentially.
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
        /// Executes jobs in a contiguous range.
        /// Example: 1-3 executes jobs 1, 2 and 3.
        /// </summary>
        public void ExecuteRange(int from, int to)
        {
            var indices = Enumerable.Range(from, to - from + 1).ToList();
            ExecuteList(indices);
        }

        /// <summary>
        /// Executes jobs from an explicit list of one-based indices.
        /// Example: 1;3 executes jobs 1 and 3.
        /// </summary>
        public void ExecuteList(List<int> indices)
        {
            foreach (int index in indices)
            {
                ExecuteOne(index);
            }
        }

        /// <summary>
        /// Executes a single backup job.
        ///
        /// Execution flow:
        /// 1. Reset runtime state.
        /// 2. Resolve the effective strategy.
        /// 3. Compute eligible files for accurate progress.
        /// 4. Mark job as active.
        /// 5. Execute the strategy.
        /// 6. Mark job as completed or error.
        /// 7. Persist updated configuration if needed.
        /// 8. Always update state.json at the end.
        /// </summary>
        private void ExecuteJob(BackupJob job)
        {
            bool fullBackupReferenceMustBeUpdated = false;

            try
            {
                job.ResetState();

                IBackupStrategy strategy = ResolveStrategy(
                    job,
                    out fullBackupReferenceMustBeUpdated);

                // The strategy is responsible for telling us which files are eligible.
                // This avoids wrong progress values for differential backups.
                var eligibleFiles = strategy.GetEligibleFiles(job, _fileSystem);

                long totalSize = eligibleFiles.Sum(file => _fileSystem.GetFileSize(file));

                job.MarkAsActive(eligibleFiles.Count, totalSize);

                UpdateStateForAllJobs();

                strategy.Execute(
                    job,
                    _fileSystem,
                    _logger,
                    OnFileCopied(job));

                job.MarkAsCompleted(fullBackupReferenceMustBeUpdated);

                // Persist LastFullBackupDate if it has been updated.
                _jobManager.Save();
            }
            catch
            {
                // In v1.1 we keep the service simple:
                // detailed errors are visible through log entries or application output.
                job.MarkAsError();
            }
            finally
            {
                // Always write final state, whether the job succeeded or failed.
                UpdateStateForAllJobs();
            }
        }

        /// <summary>
        /// Builds the callback invoked by backup strategies after each file.
        ///
        /// This keeps strategies simple and centralizes:
        /// - progress update
        /// - state file update
        /// - log entry creation
        /// </summary>
        private Action<string, string, long, long> OnFileCopied(BackupJob job)
        {
            return (sourceFile, destFile, fileSizeBytes, transferTimeMs) =>
            {
                job.UpdateProgress(fileSizeBytes, sourceFile, destFile);

                UpdateStateForAllJobs();

                LogEntry entry = transferTimeMs >= 0
                    ? LogEntry.Success(job.Name, sourceFile, destFile, fileSizeBytes, transferTimeMs)
                    : LogEntry.Failure(job.Name, sourceFile, destFile, fileSizeBytes);

                _logger.Log(entry);
            };
        }

        /// <summary>
        /// Resolves the effective backup strategy.
        ///
        /// Important:
        /// We do not modify job.Type here.
        /// If the user configured a Differential job, the configuration remains Differential.
        ///
        /// However, if no previous full backup exists, the effective execution must be Full.
        /// </summary>
        private IBackupStrategy ResolveStrategy(
            BackupJob job,
            out bool fullBackupReferenceMustBeUpdated)
        {
            fullBackupReferenceMustBeUpdated = false;

            if (job.Type == BackupType.Full)
            {
                fullBackupReferenceMustBeUpdated = true;
                return _fullStrategy;
            }

            if (job.Type == BackupType.Differential && job.LastFullBackupDate == null)
            {
                // First differential execution behaves like a full backup.
                fullBackupReferenceMustBeUpdated = true;
                return _fullStrategy;
            }

            return _differentialStrategy;
        }

        /// <summary>
        /// Writes all current job states to state.json.
        /// </summary>
        private void UpdateStateForAllJobs()
        {
            var allJobs = _jobManager.GetAll();
            _stateRepository.Update(allJobs);
        }
    }
}
