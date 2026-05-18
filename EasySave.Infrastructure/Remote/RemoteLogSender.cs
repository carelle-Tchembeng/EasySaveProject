// EasySave.Infrastructure/Remote/RemoteLogSender.cs
// NEW v3.0 — sends log entries to the centralised Docker log server via HTTP POST

using EasySave.Core.ValueObjects;
using System.Net.Http.Json;
using System.Text.Json;

namespace EasySave.Infrastructure.Remote
{
    /// <summary>
    /// Sends log entries to the centralised EasySave.LogServer Docker service via HTTP POST.
    /// Used when AppConfiguration.LogStorageMode is "Remote" or "Both".
    ///
    /// Thread-safe: HttpClient is shared (one instance per application lifetime).
    /// Fire-and-forget: failures are swallowed silently to avoid disrupting backup operations.
    /// </summary>
    public sealed class RemoteLogSender : IDisposable
    {
        private readonly HttpClient  _httpClient;
        private readonly string      _logEndpoint;
        private readonly string      _machineName;
        private readonly string      _appVersion;
        private readonly JsonSerializerOptions _jsonOptions;

        // ─── Constructor ──────────────────────────────────────────────────

        /// <param name="serverBaseUrl">
        /// Base URL of the log server. Example: "http://logserver:5000"
        /// </param>
        /// <param name="appVersion">Application version string (e.g. "3.0.0").</param>
        public RemoteLogSender(string serverBaseUrl, string appVersion = "3.0.0")
        {
            _machineName = Environment.MachineName;
            _appVersion  = appVersion;
            _logEndpoint = $"{serverBaseUrl.TrimEnd('/')}/api/log";

            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented        = false
            };
        }

        // ─── Public API ──────────────────────────────────────────────────

        /// <summary>
        /// Sends a log entry to the remote server asynchronously.
        /// Failures are caught and ignored to protect backup operations.
        /// </summary>
        public async Task SendAsync(LogEntry entry)
        {
            try
            {
                var dto = MapToRemoteDto(entry);
                await _httpClient.PostAsJsonAsync(_logEndpoint, dto, _jsonOptions);
            }
            catch
            {
                // Silent failure — remote logging must never block or crash the backup
            }
        }

        /// <summary>Fire-and-forget version for use in synchronous contexts.</summary>
        public void Send(LogEntry entry) => _ = SendAsync(entry);

        // ─── Mapping ─────────────────────────────────────────────────────

        private LogEntryRemoteDto MapToRemoteDto(LogEntry entry) => new()
        {
            Timestamp          = entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            JobName            = entry.JobName,
            SourceFile         = entry.SourceFile,
            DestFile           = entry.DestFile,
            FileSizeBytes      = entry.FileSizeBytes,
            TransferTimeMs     = entry.TransferTimeMs,
            EncryptionTimeMs   = entry.EncryptionTimeMs,
            IsError            = entry.IsError,
            IsEncrypted        = entry.IsEncrypted,
            MachineName        = _machineName,
            ApplicationVersion = _appVersion
        };

        /// <inheritdoc/>
        public void Dispose() => _httpClient.Dispose();
    }

    /// <summary>DTO matching the LogServer POST /api/log endpoint contract.</summary>
    public sealed class LogEntryRemoteDto
    {
        public string Timestamp          { get; set; } = string.Empty;
        public string JobName            { get; set; } = string.Empty;
        public string SourceFile         { get; set; } = string.Empty;
        public string DestFile           { get; set; } = string.Empty;
        public long   FileSizeBytes      { get; set; }
        public long   TransferTimeMs     { get; set; }
        public long   EncryptionTimeMs   { get; set; }
        public bool   IsError            { get; set; }
        public bool   IsEncrypted        { get; set; }
        public string MachineName        { get; set; } = string.Empty;
        public string ApplicationVersion { get; set; } = string.Empty;
    }
}
