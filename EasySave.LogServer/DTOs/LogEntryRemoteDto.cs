// EasySave.LogServer/DTOs/LogEntryRemoteDto.cs
// Payload received from EasySave clients via POST /api/log

namespace EasySave.LogServer.DTOs
{
    /// <summary>
    /// Data Transfer Object for a log entry sent by an EasySave client.
    /// Extends the standard log entry with machine identification fields
    /// so the centralised log file can identify which client sent each entry.
    /// </summary>
    public sealed class LogEntryRemoteDto
    {
        /// <summary>ISO 8601 timestamp from the client machine.</summary>
        public string Timestamp          { get; set; } = string.Empty;

        /// <summary>Name of the backup job that generated this entry.</summary>
        public string JobName            { get; set; } = string.Empty;

        /// <summary>Full UNC path of the source file.</summary>
        public string SourceFile         { get; set; } = string.Empty;

        /// <summary>Full UNC path of the destination file.</summary>
        public string DestFile           { get; set; } = string.Empty;

        /// <summary>File size in bytes.</summary>
        public long   FileSizeBytes      { get; set; }

        /// <summary>Transfer duration in milliseconds. Negative = error.</summary>
        public long   TransferTimeMs     { get; set; }

        /// <summary>
        /// Encryption duration in milliseconds.
        /// 0 = not encrypted, &gt;0 = success, &lt;0 = CryptoSoft error.
        /// </summary>
        public long   EncryptionTimeMs   { get; set; }

        /// <summary>True if the transfer failed.</summary>
        public bool   IsError            { get; set; }

        /// <summary>True if the file was successfully encrypted.</summary>
        public bool   IsEncrypted        { get; set; }

        /// <summary>Hostname of the client machine. Allows multi-machine identification.</summary>
        public string MachineName        { get; set; } = string.Empty;

        /// <summary>EasySave version running on the client.</summary>
        public string ApplicationVersion { get; set; } = string.Empty;
    }
}
