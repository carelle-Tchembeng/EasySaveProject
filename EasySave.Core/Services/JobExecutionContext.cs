// EasySave.Core/Services/JobExecutionContext.cs
// NEW v3.0 — per-job synchronisation context: pause (ManualResetEventSlim) + stop (CancellationToken)

namespace EasySave.Core.Services
{
    /// <summary>
    /// Holds per-job synchronisation primitives for pause/resume and stop control.
    /// One instance is created per job at the start of ExecuteAllAsync().
    /// Shared between BackupService (controller) and the strategy executing the job.
    /// </summary>
    public sealed class JobExecutionContext : IDisposable
    {
        // ─── Pause / Resume ──────────────────────────────────────────────
        // Initialised to "set" (not paused). Reset() = pause, Set() = resume.
        private readonly ManualResetEventSlim _pauseEvent = new(initialState: true);

        // ─── Stop (cancellation) ─────────────────────────────────────────
        private readonly CancellationTokenSource _stopCts = new();

        // ─── Public API ──────────────────────────────────────────────────

        /// <summary>CancellationToken that is cancelled when Stop() is called.</summary>
        public CancellationToken StopToken => _stopCts.Token;

        /// <summary>Returns true when the job is currently paused.</summary>
        public bool IsPaused => !_pauseEvent.IsSet;

        /// <summary>Returns true when a stop has been requested.</summary>
        public bool IsStopped => _stopCts.IsCancellationRequested;

        /// <summary>
        /// Blocks the calling thread until the job is resumed (or stop is requested).
        /// Called by the strategy at the end of each file transfer.
        /// Spec: "pause effective après le transfert du fichier en cours".
        /// </summary>
        public void WaitIfPaused()
        {
            // Wait() overload accepting CancellationToken — throws OperationCanceledException on stop.
            _pauseEvent.Wait(_stopCts.Token);
        }

        /// <summary>Pauses the job (the strategy thread will block after the current file).</summary>
        public void Pause() => _pauseEvent.Reset();

        /// <summary>Resumes the job (unblocks the strategy thread).</summary>
        public void Resume() => _pauseEvent.Set();

        /// <summary>Requests immediate stop. Throws OperationCanceledException in WaitIfPaused().</summary>
        public void Stop() => _stopCts.Cancel();

        /// <inheritdoc/>
        public void Dispose()
        {
            _pauseEvent.Dispose();
            _stopCts.Dispose();
        }
    }
}
