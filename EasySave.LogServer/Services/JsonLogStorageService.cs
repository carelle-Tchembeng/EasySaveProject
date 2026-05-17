// EasySave.LogServer/Services/JsonLogStorageService.cs
// Thread-safe daily JSON log file writer for the centralised Docker log service

using EasySave.LogServer.DTOs;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace EasySave.LogServer.Services
{
    /// <summary>
    /// Writes log entries to a daily JSON file under the configured /logs volume.
    /// Thread-safe: uses one SemaphoreSlim per file to serialise concurrent writes.
    /// Format: one JSON object per line (NDJSON-compatible), easily readable in Notepad.
    /// Single daily file regardless of the number of EasySave clients sending logs.
    /// </summary>
    public sealed class JsonLogStorageService : ILogStorageService
    {
        private readonly string _logDirectory;

        /// <summary>One lock per file path, created lazily on first access.</summary>
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _fileLocks = new();

        private readonly JsonSerializerOptions _options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented        = false  // compact — one entry per line
        };

        public JsonLogStorageService(string logDirectory)
        {
            _logDirectory = logDirectory;
            EnsureLogDirectory();
        }

        // ─── ILogStorageService ──────────────────────────────────────────

        /// <inheritdoc/>
        public async Task AppendLogAsync(LogEntryRemoteDto entry)
        {
            string filePath = GetLogFilePath(DateTime.UtcNow);
            var    fileLock = _fileLocks.GetOrAdd(filePath, _ => new SemaphoreSlim(1, 1));

            await fileLock.WaitAsync();
            try
            {
                string line = JsonSerializer.Serialize(entry, _options) + Environment.NewLine;
                await File.AppendAllTextAsync(filePath, line, Encoding.UTF8);
            }
            finally
            {
                fileLock.Release();
            }
        }

        /// <inheritdoc/>
        public string GetLogFilePath(DateTime date)
            => Path.Combine(_logDirectory, $"{date:yyyy-MM-dd}.json");

        // ─── Private ─────────────────────────────────────────────────────

        private void EnsureLogDirectory()
        {
            if (!Directory.Exists(_logDirectory))
                Directory.CreateDirectory(_logDirectory);
        }
    }
}
