// EasyLog/DTOs/LogEntryDto.cs

using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasyLog.DTOs
{
    /// <summary>
    /// Data Transfer Object representing a single log entry.
    /// This is the only type that crosses the public boundary of the EasyLog DLL.
    /// All properties use primitive types to ensure easy serialization.
    /// Consumers should use the factory methods to create instances.
    /// </summary>
    public class LogEntryDto
    {
        // ─────────────────────────────────────────────────────────────
        // Properties — all serialized to JSON
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// ISO 8601 timestamp of the file transfer.
        /// Format: yyyy-MM-dd HH:mm:ss.fff
        /// Example: 2024-11-15 14:32:01.847
        /// </summary>
        [JsonPropertyName("timestamp")]
        public string Timestamp { get; init; } = string.Empty;

        /// <summary>
        /// Name of the backup job that produced this log entry.
        /// </summary>
        [JsonPropertyName("jobName")]
        public string JobName { get; init; } = string.Empty;

        /// <summary>
        /// Full UNC path of the source file.
        /// Example: \\srv01\documents\report.pdf
        /// </summary>
        [JsonPropertyName("sourceFile")]
        public string SourceFile { get; init; } = string.Empty;

        /// <summary>
        /// Full UNC path of the destination file after copy.
        /// Example: \\srv02\backup\documents\report.pdf
        /// </summary>
        [JsonPropertyName("destFile")]
        public string DestFile { get; init; } = string.Empty;

        /// <summary>
        /// Size of the transferred file in bytes.
        /// </summary>
        [JsonPropertyName("fileSizeBytes")]
        public long FileSizeBytes { get; init; }

        /// <summary>
        /// Time taken to copy the file in milliseconds.
        /// Negative value indicates a transfer error.
        /// </summary>
        [JsonPropertyName("transferTimeMs")]
        public long TransferTimeMs { get; init; }

        /// <summary>
        /// True if the transfer failed (TransferTimeMs is negative).
        /// Serialized as a boolean in the JSON log for easy filtering.
        /// </summary>
        [JsonPropertyName("isError")]
        public bool IsError => TransferTimeMs < 0;

        // ─────────────────────────────────────────────────────────────
        // Factory methods
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a LogEntryDto for a successful file transfer.
        /// </summary>
        /// <param name="jobName">Name of the backup job.</param>
        /// <param name="sourceFile">UNC source path.</param>
        /// <param name="destFile">UNC destination path.</param>
        /// <param name="fileSizeBytes">File size in bytes.</param>
        /// <param name="transferTimeMs">Transfer duration in milliseconds.</param>
        public static LogEntryDto Success(
            string jobName,
            string sourceFile,
            string destFile,
            long   fileSizeBytes,
            long   transferTimeMs)
        {
            return new LogEntryDto
            {
                Timestamp      = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                JobName        = jobName,
                SourceFile     = sourceFile,
                DestFile       = destFile,
                FileSizeBytes  = fileSizeBytes,
                TransferTimeMs = transferTimeMs
            };
        }

        /// <summary>
        /// Creates a LogEntryDto for a failed file transfer.
        /// TransferTimeMs is set to -1 to signal an error.
        /// </summary>
        /// <param name="jobName">Name of the backup job.</param>
        /// <param name="sourceFile">UNC source path.</param>
        /// <param name="destFile">UNC destination path.</param>
        /// <param name="fileSizeBytes">File size in bytes.</param>
        public static LogEntryDto Failure(
            string jobName,
            string sourceFile,
            string destFile,
            long   fileSizeBytes)
        {
            return new LogEntryDto
            {
                Timestamp      = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                JobName        = jobName,
                SourceFile     = sourceFile,
                DestFile       = destFile,
                FileSizeBytes  = fileSizeBytes,
                TransferTimeMs = -1
            };
        }

        // ─────────────────────────────────────────────────────────────
        // Serialization helper
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Serializes this entry to an indented JSON string.
        /// The indented format ensures readability in Notepad
        /// as required by the EasySave specification.
        /// </summary>
        /// <param name="options">Optional custom serializer options.</param>
        /// <returns>Indented JSON string representation of this entry.</returns>
        public string ToJson(JsonSerializerOptions? options = null)
        {
            var defaultOptions = options ?? new JsonSerializerOptions
            {
                WriteIndented = true
            };

            return JsonSerializer.Serialize(this, defaultOptions);
        }
    }
}
