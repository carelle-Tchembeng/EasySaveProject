// EasySave.Core/Services/BackupService.cs
// UPDATED v3.0 — parallel execution, watcher thread, PauseAll/ResumeAll/StopAll, per-job controls

using EasySave.Core.Entities;
using EasySave.Core.Enums;
using EasySave.Core.Interfaces;
using EasySave.Core.ValueObjects;
using System.Collections.Concurrent;

namespace EasySave.Core.Services
{
    /// <summary>
    /// Orchestrates backup job execution.
    /// v3.0 changes:
    /// — Jobs run in PARALLEL via Task.WhenAll().
    /// — PriorityManager enforces priority-file ordering across jobs.
    /// — LargeFileTransferLock serialises transfers of large files.
    /// — Business-software watcher auto-pauses/resumes all jobs (no error, unlike v2.0).
    /// — PauseAll/ResumeAll/StopAll + per-job Pause/Resume/Stop controls.
    /// — ProgressUpdated event for real-time UI refresh.
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
        // Runtime state — active execution contexts (null when idle)
        // ─────────────────────────────────────────────────────────────

        private readonly object _contextLock = new();
        private Dictionary<Guid, JobExecutionContext>? _activeContexts;
        private List<BackupJob>?                       _activeJobs;

        // ─────────────────────────────────────────────────────────────
        // Events
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Fired after each file transfer to allow real-time UI refresh.
        /// The BackupJob argument holds the updated progress.
        /// Subscribers should dispatch to the UI thread as needed.
        /// </summary>
        public event EventHandler<BackupJob>? ProgressUpdated;

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
        // Public execution methods (v3.0 primary path)
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Executes ALL configured jobs in PARALLEL (v3.0 primary mode).
        /// Shared PriorityManager + LargeFileTransferLock coordinate access.
        /// A business-software watcher thread auto-pauses/resumes jobs.
        /// </summary>
        public async Task ExecuteAllAsync()
        {
            var jobs = _jobManager.GetAll();
            if (jobs.Count == 0) return;

            // Shared synchronisation primitives — created once per execution
            using var priorityManager = new PriorityManager(_config.PriorityExtensions);
            using var largeFileLock   = new LargeFileTransferLock(_config.MaxParallelFileSizeKb);

            // One context per job
            var contexts = jobs.ToDictionary(j => j.Id, _ => new JobExecutionContext());
            SetActiveState(jobs, contexts);

            using var watcherCts = new CancellationTokenSource();
            StartBusinessSoftwareWatcher(jobs, contexts, watcherCts.Token);

            try
            {
                var tasks = jobs.Select(job =>
                    ExecuteJobAsync(job, contexts[job.Id], priorityManager, largeFileLock))
                    .ToArray();

                await Task.WhenAll(tasks);
            }
            finally
            {
                watcherCts.Cancel();
                ClearActiveState();
                foreach (var ctx in contexts.Values) ctx.Dispose();
            }
        }

        /// <summary>Executes a single job by Guid (for UI per-job "Run" button).</summary>
        public async Task ExecuteOneAsync(Guid id)
        {
            var job = _jobManager.GetById(id);

            using var priorityManager = new PriorityManager(_config.PriorityExtensions);
            using var largeFileLock   = new LargeFileTransferLock(_config.MaxParallelFileSizeKb);
            using var context         = new JobExecutionContext();

            SetActiveState(new List<BackupJob> { job }, new Dictionary<Guid, JobExecutionContext> { [id] = context });

            try
            {
                await ExecuteJobAsync(job, context, priorityManager, largeFileLock);
            }
            finally
            {
                ClearActiveState();
            }
        }

        // ─── Legacy sync wrappers (CLI compatibility v1.0/1.1/2.0) ───

        /// <summary>Synchronous wrapper — runs all jobs sequentially (CLI mode).</summary>
        public void ExecuteAll() => ExecuteAllAsync().GetAwaiter().GetResult();

        /// <summary>Synchronous single-job execution (CLI mode).</summary>
        public void ExecuteOne(Guid id) => ExecuteOneAsync(id).GetAwaiter().GetResult();

        /// <summary>Executes jobs at specific 1-based indices (CLI compatibility).</summary>
        public void ExecuteList(List<int> indices)
        {
            var jobs = _jobManager.GetAll();
            foreach (int idx in indices.Where(i => i >= 1 && i <= jobs.Count))
                ExecuteOne(jobs[idx - 1].Id);
        }

        /// <summary>Executes a range of jobs by 1-based index (CLI compatibility).</summary>
        public void ExecuteRange(int from, int to)
        {
            var jobs = _jobManager.GetAll();
            for (int i = from - 1; i < to && i < jobs.Count; i++)
                ExecuteOne(jobs[i].Id);
        }

        // ─────────────────────────────────────────────────────────────
        // Pause / Resume / Stop — global controls
        // ─────────────────────────────────────────────────────────────

        /// <summary>Pauses all currently active jobs. Pause takes effect after each job's current file.</summary>
        public void PauseAll()
        {
            lock (_contextLock)
            {
                if (_activeContexts == null) return;
                foreach (var ctx in _activeContexts.Values) ctx.Pause();
                if (_activeJobs != null)
                    foreach (var job in _activeJobs.Where(j => j.Status == BackupStatus.Active))
                        job.MarkAsPaused();
            }
            UpdateStateForAllJobs();
            _logger.Log(LogEntry.Failure("SYSTEM", "All jobs paused by user", string.Empty, 0));
        }

        /// <summary>Resumes all paused jobs.</summary>
        public void ResumeAll()
        {
            lock (_contextLock)
            {
                if (_activeContexts == null) return;
                foreach (var ctx in _activeContexts.Values) ctx.Resume();
                if (_activeJobs != null)
                    foreach (var job in _activeJobs.Where(j => j.Status == BackupStatus.Paused))
                        job.MarkAsResumed();
            }
            UpdateStateForAllJobs();
            _logger.Log(LogEntry.Success("SYSTEM", "All jobs resumed by user", string.Empty, 0, 0));
        }

        /// <summary>Requests immediate stop for all running jobs.</summary>
        public void StopAll()
        {
            lock (_contextLock)
            {
                if (_activeContexts == null) return;
                foreach (var ctx in _activeContexts.Values) ctx.Stop();
            }
            _logger.Log(LogEntry.Failure("SYSTEM", "All jobs stopped by user", string.Empty, 0));
        }

        // ─────────────────────────────────────────────────────────────
        // Pause / Resume / Stop — per-job controls
        // ─────────────────────────────────────────────────────────────

        /// <summary>Pauses a single job identified by Guid.</summary>
        public void PauseJob(Guid id)
        {
            lock (_contextLock)
            {
                if (_activeContexts != null && _activeContexts.TryGetValue(id, out var ctx))
                    ctx.Pause();
                var job = _activeJobs?.FirstOrDefault(j => j.Id == id);
                if (job?.Status == BackupStatus.Active) job.MarkAsPaused();
            }
            UpdateStateForAllJobs();
        }

        /// <summary>Resumes a single paused job identified by Guid.</summary>
        public void ResumeJob(Guid id)
        {
            lock (_contextLock)
            {
                if (_activeContexts != null && _activeContexts.TryGetValue(id, out var ctx))
                    ctx.Resume();
                var job = _activeJobs?.FirstOrDefault(j => j.Id == id);
                if (job?.Status == BackupStatus.Paused) job.MarkAsResumed();
            }
            UpdateStateForAllJobs();
        }

        /// <summary>Requests immediate stop for a single job.</summary>
        public void StopJob(Guid id)
        {
            lock (_contextLock)
            {
                if (_activeContexts != null && _activeContexts.TryGetValue(id, out var ctx))
                    ctx.Stop();
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Core execution logic
        // ─────────────────────────────────────────────────────────────

        private async Task ExecuteJobAsync(
            BackupJob             job,
            JobExecutionContext   context,
            PriorityManager      priorityManager,
            LargeFileTransferLock largeFileLock)
        {
            try
            {
                job.ResetState();
                var strategy = ResolveStrategy(job);
                var files    = _fileSystem.GetFiles(job.SourcePath);
                long total   = files.Sum(f => _fileSystem.GetFileSize(f));
                job.MarkAsActive(files.Count, total);
                UpdateStateForAllJobs();

                await Task.Run(() => strategy.Execute(
                    job,
                    _fileSystem,
                    _logger,
                    _encryptionService,
                    _config,
                    context,
                    priorityManager,
                    largeFileLock,
                    BuildFileCopiedCallback(job, context)));

                if (!context.IsStopped)
                    job.MarkAsCompleted();
                else
                    job.MarkAsError();
            }
            catch (OperationCanceledException)
            {
                job.MarkAsError();
            }
            catch
            {
                job.MarkAsError();
            }
            finally
            {
                UpdateStateForAllJobs();
            }
        }

        /// <summary>
        /// Builds the per-file callback:
        ///   — updates in-memory progress
        ///   — writes state.json
        ///   — writes log (local and/or remote)
        ///   — fires ProgressUpdated event for real-time UI
        /// </summary>
        private Action<string, string, long, long, long> BuildFileCopiedCallback(
            BackupJob job, JobExecutionContext context)
        {
            return (sourceFile, destFile, fileSizeBytes, transferTimeMs, encryptionTimeMs) =>
            {
                job.UpdateProgress(fileSizeBytes, sourceFile, destFile);
                UpdateStateForAllJobs();

                LogEntry entry = encryptionTimeMs != 0
                    ? LogEntry.SuccessWithEncryption(
                        job.Name, sourceFile, destFile, fileSizeBytes, transferTimeMs, encryptionTimeMs)
                    : (transferTimeMs >= 0
                        ? LogEntry.Success(job.Name, sourceFile, destFile, fileSizeBytes, transferTimeMs)
                        : LogEntry.Failure(job.Name, sourceFile, destFile, fileSizeBytes));

                _logger.Log(entry);

                // Real-time UI notification
                ProgressUpdated?.Invoke(this, job);
            };
        }

        // ─────────────────────────────────────────────────────────────
        // Business-software watcher (v3.0: auto pause/resume, no error)
        // ─────────────────────────────────────────────────────────────

        private void StartBusinessSoftwareWatcher(
            List<BackupJob>                       jobs,
            Dictionary<Guid, JobExecutionContext> contexts,
            CancellationToken                     ct)
        {
            if (!_config.IsBusinessSoftwareDetectionEnabled()) return;

            Task.Run(() =>
            {
                bool wasRunning = false;

                while (!ct.IsCancellationRequested)
                {
                    bool isRunning = _bizDetector.IsRunning(_config.BusinessSoftwareName);

                    if (isRunning && !wasRunning)
                    {
                        // Pause all active jobs
                        foreach (var ctx in contexts.Values) ctx.Pause();
                        foreach (var job in jobs.Where(j => j.Status == BackupStatus.Active))
                            job.MarkAsPaused();
                        UpdateStateForAllJobs();
                        _logger.Log(LogEntry.Failure(
                            "SYSTEM",
                            $"Business software '{_config.BusinessSoftwareName}' detected — all jobs paused",
                            string.Empty, 0));
                        wasRunning = true;
                    }
                    else if (!isRunning && wasRunning)
                    {
                        // Resume all paused jobs
                        foreach (var ctx in contexts.Values) ctx.Resume();
                        foreach (var job in jobs.Where(j => j.Status == BackupStatus.Paused))
                            job.MarkAsResumed();
                        UpdateStateForAllJobs();
                        _logger.Log(LogEntry.Success(
                            "SYSTEM",
                            $"Business software '{_config.BusinessSoftwareName}' stopped — all jobs resumed",
                            string.Empty, 0, 0));
                        wasRunning = false;
                    }

                    Thread.Sleep(500);
                }
            }, ct);
        }

        // ─────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────

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
        {
            var jobs = _activeJobs ?? _jobManager.GetAll();
            _stateRepository.Update(jobs);
        }

        private void SetActiveState(List<BackupJob> jobs, Dictionary<Guid, JobExecutionContext> contexts)
        {
            lock (_contextLock) { _activeJobs = jobs; _activeContexts = contexts; }
        }

        private void ClearActiveState()
        {
            lock (_contextLock) { _activeJobs = null; _activeContexts = null; }
        }
    }
}
