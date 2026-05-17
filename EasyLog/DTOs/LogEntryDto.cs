// EasyLog/DTOs/LogEntryDto.cs
// UPDATED v2.0 — added EncryptionTimeMs, IsEncrypted, SuccessWithEncryption()

using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasyLog.DTOs
{
    /// <summary>
    /// Public DTO crossing the EasyLog DLL boundary.
    /// v2.0: + EncryptionTimeMs, + IsEncrypted, + SuccessWithEncryption()
    /// </summary>
    public class LogEntryDto
    {
        [JsonPropertyName("timestamp")]
        public string Timestamp { get; init; } = string.Empty;

        [JsonPropertyName("jobName")]
        public string JobName { get; init; } = string.Empty;

        [JsonPropertyName("sourceFile")]
        public string SourceFile { get; init; } = string.Empty;

        [JsonPropertyName("destFile")]
        public string DestFile { get; init; } = string.Empty;

        [JsonPropertyName("fileSizeBytes")]
        public long FileSizeBytes { get; init; }

        [JsonPropertyName("transferTimeMs")]
        public long TransferTimeMs { get; init; }

        /// <summary>0=no encryption, >0=duration ms, &lt;0=CryptoSoft error. NEW v2.0.</summary>
        [JsonPropertyName("encryptionTimeMs")]
        public long EncryptionTimeMs { get; init; }

        [JsonPropertyName("isError")]
        public bool IsError => TransferTimeMs < 0;

        /// <summary>True if file was successfully encrypted. NEW v2.0.</summary>
        [JsonPropertyName("isEncrypted")]
        public bool IsEncrypted { get; init; }

        // ── Factory methods ───────────────────────────────────────────

        public static LogEntryDto Success(string jobName, string src, string dest,
            long size, long transferMs) => new()
        {
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            JobName = jobName, SourceFile = src, DestFile = dest,
            FileSizeBytes = size, TransferTimeMs = transferMs,
            EncryptionTimeMs = 0, IsEncrypted = false
        };

        public static LogEntryDto Failure(string jobName, string src, string dest,
            long size) => new()
        {
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            JobName = jobName, SourceFile = src, DestFile = dest,
            FileSizeBytes = size, TransferTimeMs = -1,
            EncryptionTimeMs = 0, IsEncrypted = false
        };

        /// <summary>NEW v2.0 — includes encryption result.</summary>
        public static LogEntryDto SuccessWithEncryption(string jobName, string src, string dest,
            long size, long transferMs, long encryptionMs) => new()
        {
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            JobName = jobName, SourceFile = src, DestFile = dest,
            FileSizeBytes = size, TransferTimeMs = transferMs,
            EncryptionTimeMs = encryptionMs, IsEncrypted = encryptionMs > 0
        };

        public string ToJson(JsonSerializerOptions? options = null) =>
            JsonSerializer.Serialize(this, options ?? new JsonSerializerOptions { WriteIndented = true });
    }
}
