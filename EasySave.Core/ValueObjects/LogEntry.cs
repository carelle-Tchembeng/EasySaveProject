// EasySave.Core/ValueObjects/LogEntry.cs

namespace EasySave.Core.ValueObjects
{
    /// <summary>
    /// Immutable value object representing a single log entry produced after a file transfer.
    /// Passed to ILogger.Log() after each file copy attempt (successful or failed).
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Date and time when the file transfer occurred.
        /// </summary>
        public DateTime Timestamp { get; init; }

        /// <summary>
        /// Name of the backup job that triggered this transfer.
        /// </summary>
        public string JobName { get; init; } = string.Empty;

        /// <summary>
        /// Full UNC path of the source file that was copied.
        /// Example: \\srv01\documents\report.pdf
        /// </summary>
        public string SourceFile { get; init; } = string.Empty;

        /// <summary>
        /// Full UNC path of the destination file after copy.
        /// Example: \\srv02\backup\documents\report.pdf
        /// </summary>
        public string DestFile { get; init; } = string.Empty;

        /// <summary>
        /// Size of the file in bytes at the time of transfer.
        /// </summary>
        public long FileSizeBytes { get; init; }

        /// <summary>
        /// Time taken to copy the file, in milliseconds.
        /// Negative value indicates a transfer error (e.g. -1 if an exception was thrown).
        /// </summary>
        public long TransferTimeMs { get; init; }

        /// <summary>
        /// Returns true if the transfer failed (TransferTimeMs is negative).
        /// </summary>
        public bool IsError => TransferTimeMs < 0;

        /// <summary>
        /// Creates a LogEntry for a successful file transfer.
        /// </summary>
        public static LogEntry Success(
            string jobName,
            string sourceFile,
            string destFile,
            long fileSizeBytes,
            long transferTimeMs)
        {
            return new LogEntry
            {
                Timestamp      = DateTime.Now,
                JobName        = jobName,
                SourceFile     = sourceFile,
                DestFile       = destFile,
                FileSizeBytes  = fileSizeBytes,
                TransferTimeMs = transferTimeMs
            };
        }

        /// <summary>
        /// Creates a LogEntry for a failed file transfer.
        /// TransferTimeMs is set to -1 to indicate an error.
        /// </summary>
        public static LogEntry Failure(
            string jobName,
            string sourceFile,
            string destFile,
            long fileSizeBytes)
        {
            return new LogEntry
            {
                Timestamp      = DateTime.Now,
                JobName        = jobName,
                SourceFile     = sourceFile,
                DestFile       = destFile,
                FileSizeBytes  = fileSizeBytes,
                TransferTimeMs = -1
            };
        }
    }
}
