// EasySave.Core/Services/PriorityManager.cs
// NEW v3.0 — blocks non-priority files while priority files are in flight across any job

namespace EasySave.Core.Services
{
    /// <summary>
    /// Implements the priority file rule from v3.0:
    /// "No non-priority file can be transferred while at least one priority file is pending."
    ///
    /// Uses an atomic counter for pending priority files and a ManualResetEventSlim gate:
    /// — gate is SET   (open)  when counter == 0 (no pending priority transfers)
    /// — gate is RESET (closed) when counter  > 0 (at least one priority file in flight)
    ///
    /// Non-priority files call WaitIfNonPriority() which blocks on the closed gate.
    /// Priority files call RegisterFile() before transfer and ReleaseFile() after.
    /// </summary>
    public sealed class PriorityManager : IDisposable
    {
        private readonly IReadOnlyList<string> _priorityExtensions;

        /// <summary>Number of priority files currently being transferred across all jobs.</summary>
        private int _pendingPriorityCount;

        /// <summary>Gate: SET = no priority pending, RESET = priority files in flight → non-priority must wait.</summary>
        private readonly ManualResetEventSlim _gate = new(initialState: true);

        private readonly object _lock = new();

        // ─── Constructor ──────────────────────────────────────────────────

        /// <param name="priorityExtensions">
        /// Extensions that have priority (with leading dot, case-insensitive).
        /// Example: [".xlsx", ".docx"]
        /// </param>
        public PriorityManager(IEnumerable<string> priorityExtensions)
        {
            _priorityExtensions = priorityExtensions
                .Select(e => e.StartsWith('.') ? e.ToLowerInvariant() : $".{e.ToLowerInvariant()}")
                .ToList();
        }

        // ─── Public API ──────────────────────────────────────────────────

        /// <summary>Returns true if the file has a priority extension.</summary>
        public bool IsPriorityFile(string filePath)
        {
            if (_priorityExtensions.Count == 0) return false;
            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            return _priorityExtensions.Contains(ext);
        }

        /// <summary>
        /// Called BEFORE a priority file transfer starts.
        /// Increments the counter and closes the gate (blocks non-priority files).
        /// No-op for non-priority files.
        /// </summary>
        public void RegisterFile(string filePath)
        {
            if (!IsPriorityFile(filePath)) return;
            lock (_lock)
            {
                _pendingPriorityCount++;
                if (_pendingPriorityCount == 1)
                    _gate.Reset(); // close gate — block non-priority
            }
        }

        /// <summary>
        /// Called AFTER a priority file transfer completes.
        /// Decrements the counter. When it reaches 0, opens the gate (unblocks non-priority files).
        /// No-op for non-priority files.
        /// </summary>
        public void ReleaseFile(string filePath)
        {
            if (!IsPriorityFile(filePath)) return;
            lock (_lock)
            {
                _pendingPriorityCount = Math.Max(0, _pendingPriorityCount - 1);
                if (_pendingPriorityCount == 0)
                    _gate.Set(); // open gate — unblock non-priority
            }
        }

        /// <summary>
        /// Called by non-priority files BEFORE transfer.
        /// Blocks until no priority files are in flight across any job.
        /// Priority files pass through immediately.
        /// </summary>
        public void WaitIfNonPriority(string filePath, CancellationToken ct)
        {
            if (IsPriorityFile(filePath)) return; // priority files never wait
            _gate.Wait(ct); // blocks if gate is closed (priority files in flight)
        }

        /// <summary>True when at least one priority file is currently being transferred.</summary>
        public bool HasPendingPriority => _pendingPriorityCount > 0;

        /// <inheritdoc/>
        public void Dispose() => _gate.Dispose();
    }
}
