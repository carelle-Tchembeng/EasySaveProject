namespace EasySave.Core.ValueObjects
{
    /// <summary>
    /// Represents a domain-level log entry produced after a file transfer.
    ///
    /// This class belongs to the Core layer and must remain independent
    /// from any concrete logging library.
    ///
    /// The Infrastructure layer converts this object into an EasyLog DTO
    /// through EasyLogAdapter.
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Date and time at which the transfer was processed.
        /// </summary>
        public DateTime Timestamp { get; init; }

        /// <summary>
        /// Name of the backup job that produced this log entry.
        /// </summary>
        public string JobName { get; init; } = string.Empty;

        /// <summary>
        /// Full UNC path of the source file.
        /// </summary>
        public string SourceFile { get; init; } = string.Empty;

        /// <summary>
        /// Full UNC path of the destination file.
        /// </summary>
        public string DestFile { get; init; } = string.Empty;

        /// <summary>
        /// Size of the transferred file in bytes.
        /// </summary>
        public long FileSizeBytes { get; init; }

        /// <summary>
        /// Time required to copy the file, in milliseconds.
        ///
        /// A negative value means the transfer failed.
        /// This follows the project specification.
        /// </summary>
        public long TransferTimeMs { get; init; }

        /// <summary>
        /// Time required to encrypt the file, in milliseconds.
        ///
        /// This field is introduced for forward compatibility with EasySave v2.0.
        ///
        /// 0  = no encryption was performed.
        /// >0 = encryption duration in milliseconds.
        /// <0 = encryption error code.
        ///
        /// In EasySave v1.1, this value will normally remain 0.
        /// </summary>
        public long CryptoTimeMs { get; init; } = 0;

        /// <summary>
        /// Convenience property used to detect failed transfers.
        /// </summary>
        public bool IsError => TransferTimeMs < 0;

        /// <summary>
        /// Creates a log entry for a successful file transfer.
        /// </summary>
        public static LogEntry Success(
            string jobName,
            string sourceFile,
            string destFile,
            long fileSizeBytes,
            long transferTimeMs,
            long cryptoTimeMs = 0)
        {
            return new LogEntry
            {
                Timestamp = DateTime.Now,
                JobName = jobName,
                SourceFile = sourceFile,
                DestFile = destFile,
                FileSizeBytes = fileSizeBytes,
                TransferTimeMs = transferTimeMs,
                CryptoTimeMs = cryptoTimeMs
            };
        }

        /// <summary>
        /// Creates a log entry for a failed file transfer.
        /// TransferTimeMs is set to -1 by convention.
        /// </summary>
        public static LogEntry Failure(
            string jobName,
            string sourceFile,
            string destFile,
            long fileSizeBytes,
            long cryptoTimeMs = 0)
        {
            return new LogEntry
            {
                Timestamp = DateTime.Now,
                JobName = jobName,
                SourceFile = sourceFile,
                DestFile = destFile,
                FileSizeBytes = fileSizeBytes,
                TransferTimeMs = -1,
                CryptoTimeMs = cryptoTimeMs
            };
        }
    }
}
