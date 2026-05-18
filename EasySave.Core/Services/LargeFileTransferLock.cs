// EasySave.Core/Services/LargeFileTransferLock.cs
// NEW v3.0 — prevents simultaneous transfer of files exceeding n KB across all jobs

namespace EasySave.Core.Services
{
    /// <summary>
    /// Implements the large-file bandwidth rule from v3.0:
    /// "It is forbidden to transfer two files simultaneously if both exceed n KB."
    ///
    /// Uses a SemaphoreSlim(1,1) so only one large file can be in transit at a time.
    /// Small files (below the threshold) are unaffected and transfer without acquiring the lock.
    ///
    /// Caller pattern:
    ///   largeFileLock.Acquire(fileSize, ct);
    ///   try   { /* copy file */ }
    ///   finally { largeFileLock.Release(fileSize); }
    /// </summary>
    public sealed class LargeFileTransferLock : IDisposable
    {
        private readonly long _thresholdBytes;

        /// <summary>
        /// Semaphore(1,1) — at most one large file can be transferred at a time across all jobs.
        /// </summary>
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        // ─── Constructor ──────────────────────────────────────────────────

        /// <param name="maxFileSizeKb">
        /// Files strictly larger than this value (in KB) are considered "large".
        /// 0 = feature disabled (no restriction applied).
        /// </param>
        public LargeFileTransferLock(long maxFileSizeKb)
        {
            // Convert KB to bytes; maxValue means "no large file is possible" → feature disabled
            _thresholdBytes = maxFileSizeKb > 0 ? maxFileSizeKb * 1024 : long.MaxValue;
        }

        // ─── Public API ──────────────────────────────────────────────────

        /// <summary>Returns true if the file size exceeds the configured threshold.</summary>
        public bool IsLargeFile(long fileSizeBytes)
            => _thresholdBytes != long.MaxValue && fileSizeBytes > _thresholdBytes;

        /// <summary>
        /// Acquires the large-file mutex if the file exceeds the threshold.
        /// Small files pass through without blocking.
        /// Throws OperationCanceledException if ct is cancelled while waiting.
        /// </summary>
        public void Acquire(long fileSizeBytes, CancellationToken ct)
        {
            if (!IsLargeFile(fileSizeBytes)) return;
            _semaphore.Wait(ct);
        }

        /// <summary>
        /// Releases the large-file mutex. Must be called once per successful Acquire().
        /// No-op for small files.
        /// </summary>
        public void Release(long fileSizeBytes)
        {
            if (!IsLargeFile(fileSizeBytes)) return;
            _semaphore.Release();
        }

        /// <inheritdoc/>
        public void Dispose() => _semaphore.Dispose();
    }
}
