// EasySave.Infrastructure/Logging/EasyLogAdapter.cs
// UPDATED v3.0 — routes logs to local file, remote Docker server, or both

using EasySave.Core.Interfaces;
using EasySave.Core.ValueObjects;
using EasySave.Infrastructure.Remote;
using EasyLog;
using EasyLog.DTOs;

namespace EasySave.Infrastructure.Logging
{
    /// <summary>
    /// Adapter between Core ILogger and EasyLog DLL IEasyLogWriter.
    /// v3.0: adds remote log routing via RemoteLogSender.
    /// Routing is controlled by AppConfiguration.LogStorageMode:
    ///   "Local"  → local file only
    ///   "Remote" → Docker server only
    ///   "Both"   → local file + Docker server
    /// </summary>
    public class EasyLogAdapter : ILogger
    {
        private readonly IEasyLogWriter _writer;
        private RemoteLogSender?        _remoteSender;
        private bool                    _localEnabled  = true;
        private bool                    _remoteEnabled = false;

        public EasyLogAdapter(IEasyLogWriter writer)
            => _writer = writer ?? throw new ArgumentNullException(nameof(writer));

        // ─── Configuration ────────────────────────────────────────────────

        /// <summary>
        /// Configures remote log routing.
        /// Call once at startup after AppConfiguration is loaded.
        /// </summary>
        public void ConfigureRemoteLogging(bool localEnabled, bool remoteEnabled, string? serverUrl)
        {
            _localEnabled  = localEnabled;
            _remoteEnabled = remoteEnabled && !string.IsNullOrWhiteSpace(serverUrl);

            if (_remoteEnabled)
            {
                _remoteSender?.Dispose();
                _remoteSender = new RemoteLogSender(serverUrl!);
            }
        }

        /// <summary>Changes the log format (JSON or XML) at runtime.</summary>
        public void SetFormat(LogFormat format) => _writer.SetFormat(format);

        // ─── ILogger ─────────────────────────────────────────────────────

        /// <summary>
        /// Writes a log entry to the configured destinations.
        /// Thread-safe: EasyLogWriter and RemoteLogSender are both thread-safe.
        /// </summary>
        public void Log(LogEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            // Local file
            if (_localEnabled)
                _writer.Write(MapToDto(entry));

            // Remote Docker server (fire-and-forget)
            if (_remoteEnabled && _remoteSender != null)
                _remoteSender.Send(entry);
        }

        // ─── Mapping ─────────────────────────────────────────────────────

        private static LogEntryDto MapToDto(LogEntry entry)
        {
            if (entry.EncryptionTimeMs != 0)
            {
                return LogEntryDto.SuccessWithEncryption(
                    entry.JobName, entry.SourceFile, entry.DestFile,
                    entry.FileSizeBytes, entry.TransferTimeMs, entry.EncryptionTimeMs);
            }
            return entry.IsError
                ? LogEntryDto.Failure(entry.JobName, entry.SourceFile, entry.DestFile, entry.FileSizeBytes)
                : LogEntryDto.Success(entry.JobName, entry.SourceFile, entry.DestFile, entry.FileSizeBytes, entry.TransferTimeMs);
        }

        /// <summary>Releases remote sender resources.</summary>
        public void Dispose() => _remoteSender?.Dispose();
    }
}
