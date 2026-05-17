// EasySave.Core/ValueObjects/LogEntry.cs
// UPDATED v2.0 — added EncryptionTimeMs, IsEncrypted, SuccessWithEncryption()

namespace EasySave.Core.ValueObjects
{
    /// <summary>
    /// Immutable value object representing a single log entry after a file transfer.
    /// v2.0: + EncryptionTimeMs (0=none, >0=ms, &lt;0=error), + IsEncrypted, + SuccessWithEncryption()
    /// </summary>
    public class LogEntry
    {
        public DateTime Timestamp        { get; init; }
        public string   JobName          { get; init; } = string.Empty;
        public string   SourceFile       { get; init; } = string.Empty;
        public string   DestFile         { get; init; } = string.Empty;
        public long     FileSizeBytes    { get; init; }
        public long     TransferTimeMs   { get; init; }
        /// <summary>0=no encryption, >0=duration ms, &lt;0=CryptoSoft error. NEW v2.0.</summary>
        public long     EncryptionTimeMs { get; init; }
        /// <summary>True if file was successfully encrypted. NEW v2.0.</summary>
        public bool     IsEncrypted      { get; init; }
        public bool     IsError          => TransferTimeMs < 0;

        public static LogEntry Success(string jobName, string src, string dest, long size, long transferMs) =>
            new() { Timestamp=DateTime.Now, JobName=jobName, SourceFile=src, DestFile=dest,
                    FileSizeBytes=size, TransferTimeMs=transferMs, EncryptionTimeMs=0, IsEncrypted=false };

        public static LogEntry Failure(string jobName, string src, string dest, long size) =>
            new() { Timestamp=DateTime.Now, JobName=jobName, SourceFile=src, DestFile=dest,
                    FileSizeBytes=size, TransferTimeMs=-1, EncryptionTimeMs=0, IsEncrypted=false };

        /// <summary>NEW v2.0 — transfer + encryption result.</summary>
        public static LogEntry SuccessWithEncryption(string jobName, string src, string dest,
            long size, long transferMs, long encryptionMs) =>
            new() { Timestamp=DateTime.Now, JobName=jobName, SourceFile=src, DestFile=dest,
                    FileSizeBytes=size, TransferTimeMs=transferMs,
                    EncryptionTimeMs=encryptionMs, IsEncrypted=encryptionMs > 0 };
    }
}
