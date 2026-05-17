// EasySave.Core/Services/BackupService.cs
// UPDATED v2.0 — adds IEncryptionService, IBusinessSoftwareDetector, AppConfiguration

using EasySave.Core.Entities;
using EasySave.Core.Enums;
using EasySave.Core.Interfaces;
using EasySave.Core.ValueObjects;

namespace EasySave.Core.Services
{
    /// <summary>
    /// Orchestrates backup job execution.
    /// v2.0 changes:
    /// — Checks for business software before and during each execution
    /// — Passes IEncryptionService + AppConfiguration to strategies
    /// — Callback now carries encryptionTimeMs as 5th parameter
    /// — Jobs identified by Guid (not int index)
    /// </summary>
    public class BackupService
    {
        // ─────────────────────────────────────────────────────────────
        // Dependencies
        // ─────────────────────────────────────────────────────────────

        private readonly JobManager                  _jobManager;
        private readonly IFileSystem                 _fileSystem;
        private readonly IStateRepository            _stateRepository;
        private readonly ILogger                     _logger;
        private readonly IBackupStrategy             _fullStrategy;
        private readonly IBackupStrategy             _differentialStrategy;
        private readonly IEncryptionService          _encryptionService;
        private readonly IBusinessSoftwareDetector   _bizDetector;
        private readonly AppConfiguration            _config;

        // ─────────────────────────────────────────────────────────────
        // Constructor
        // ─────────────────────────────────────────────────────────────

        public BackupService(
            JobManager                jobManager,
            IFileSystem               fileSystem,
            IStateRepository          stateRepository,
            ILogger                   logger,
            IBackupStrategy           fullStrategy,
            IBackupStrategy           differentialStrategy,
            IEncryptionService        encryptionService,
            IBusinessSoftwareDetector bizDetector,
            AppConfiguration          config)
        {
            _jobManager           = jobManager;
            _fileSystem           = fileSystem;
            _stateRepository      = stateRepository;
            _logger               = logger;
            _fullStrategy         = fullStrategy;
            _differentialStrategy = differentialStrategy;
            _encryptionService    = encryptionService;
            _bizDetector          = bizDetector;
            _config               = config;
        }

        // ─────────────────────────────────────────────────────────────
        // Public execution methods
        // ─────────────────────────────────────────────────────────────

        /// <summary>Executes a single backup job by Guid.</summary>
        public void ExecuteOne(Guid id)
        {
            var job = _jobManager.GetById(id);
            ExecuteJob(job);
        }

        /// <summary>Executes all configured jobs sequentially.</summary>
        public void ExecuteAll()
        {
            foreach (var job in _jobManager.GetAll())
                ExecuteJob(job);
        }

        /// <summary>Executes a contiguous range of jobs by index (1-based, for CLI compatibility).</summary>
        public void ExecuteRange(int from, int to)
        {
            var jobs = _jobManager.GetAll();
            for (int i = from - 1; i < to && i < jobs.Count; i++)
                ExecuteJob(jobs[i]);
        }

        /// <summary>Executes jobs at specific 1-based indices (for CLI compatibility).</summary>
        public void ExecuteList(List<int> indices)
        {
            var jobs = _jobManager.GetAll();
            foreach (int idx in indices)
            {
                if (idx >= 1 && idx <= jobs.Count)
                    ExecuteJob(jobs[idx - 1]);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Core execution logic
        // ─────────────────────────────────────────────────────────────

        private void ExecuteJob(BackupJob job)
        {
            try
            {
                // ── Step 1: Check business software BEFORE launch ──────
                if (CheckBusinessSoftware(job))
                {
                    LogBusinessSoftwareBlocked(job);
                    job.MarkAsError();
                    UpdateStateForAllJobs();
                    return;
                }

                // ── Step 2: Initialize execution ───────────────────────
                job.ResetState();
                var strategy = ResolveStrategy(job);
                var files    = _fileSystem.GetFiles(job.SourcePath);
                long total   = files.Sum(f => _fileSystem.GetFileSize(f));
                job.MarkAsActive(files.Count, total);
                UpdateStateForAllJobs();

                // ── Step 3: Execute strategy with per-file callback ────
                // CancellationToken for business software interruption
                using var cts = new CancellationTokenSource();

                strategy.Execute(
                    job,
                    _fileSystem,
                    _logger,
                    _encryptionService,
                    _config,
                    OnFileCopied(job, cts));

                // ── Step 4: Complete ───────────────────────────────────
                job.MarkAsCompleted();
            }
            catch (OperationCanceledException)
            {
                // Business software detected mid-execution — already logged in callback
                job.MarkAsError();
            }
            catch (Exception)
            {
                job.MarkAsError();
            }
            finally
            {
                UpdateStateForAllJobs();
            }
        }

        /// <summary>
        /// Builds the per-file callback.
        /// Checks business software after each file.
        /// Logs the transfer (with encryption info).
        /// Updates state.json.
        /// </summary>
        private Action<string, string, long, long, long> OnFileCopied(
            BackupJob job,
            CancellationTokenSource cts)
        {
            return (sourceFile, destFile, fileSizeBytes, transferTimeMs, encryptionTimeMs) =>
            {
                // Update in-memory progress
                job.UpdateProgress(fileSizeBytes, sourceFile, destFile);

                // Write state.json
                UpdateStateForAllJobs();

                // Write log entry
                LogEntry entry = encryptionTimeMs != 0
                    ? LogEntry.SuccessWithEncryption(
                        job.Name, sourceFile, destFile, fileSizeBytes, transferTimeMs, encryptionTimeMs)
                    : (transferTimeMs >= 0
                        ? LogEntry.Success(job.Name, sourceFile, destFile, fileSizeBytes, transferTimeMs)
                        : LogEntry.Failure(job.Name, sourceFile, destFile, fileSizeBytes));

                _logger.Log(entry);

                // Check business software AFTER completing this file
                if (CheckBusinessSoftware(job))
                {
                    LogBusinessSoftwareInterrupted(job, sourceFile);
                    cts.Cancel();
                    cts.Token.ThrowIfCancellationRequested();
                }
            };
        }

        /// <summary>
        /// Returns true if business software is detected and detection is enabled.
        /// </summary>
        private bool CheckBusinessSoftware(BackupJob job)
        {
            if (!_config.IsBusinessSoftwareDetectionEnabled()) return false;
            return _bizDetector.IsRunning(_config.BusinessSoftwareName);
        }

        /// <summary>Resolves the strategy — falls back to Full if Differential has no reference date.</summary>
        private IBackupStrategy ResolveStrategy(BackupJob job)
        {
            if (job.Type == BackupType.Differential && job.LastFullBackupDate == null)
            {
                job.Type = BackupType.Full;
                return _fullStrategy;
            }
            return job.Type == BackupType.Full ? _fullStrategy : _differentialStrategy;
        }

        private void UpdateStateForAllJobs()
            => _stateRepository.Update(_jobManager.GetAll());

        private void LogBusinessSoftwareBlocked(BackupJob job)
        {
            _logger.Log(LogEntry.Failure(
                job.Name,
                $"BLOCKED: {_config.BusinessSoftwareName} is running",
                string.Empty,
                0));
        }

        private void LogBusinessSoftwareInterrupted(BackupJob job, string currentFile)
        {
            _logger.Log(LogEntry.Failure(
                job.Name,
                $"INTERRUPTED: {_config.BusinessSoftwareName} detected during {currentFile}",
                string.Empty,
                0));
        }
    }
}
