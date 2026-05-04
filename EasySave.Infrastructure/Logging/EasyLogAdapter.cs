// EasySave.Infrastructure/Logging/EasyLogAdapter.cs

using EasySave.Core.Interfaces;
using EasySave.Core.ValueObjects;
using EasyLog;
using EasyLog.DTOs;

namespace EasySave.Infrastructure.Logging
{
    /// <summary>
    /// Adapter that bridges the Core layer's ILogger interface
    /// and the EasyLog DLL's IEasyLogWriter interface.
    ///
    /// This class is the only place in the Infrastructure layer
    /// that knows about both ILogger (Core) and IEasyLogWriter (EasyLog).
    /// If the EasyLog DLL API changes in a future version,
    /// only this adapter needs to be updated.
    ///
    /// Follows the Adapter design pattern.
    /// </summary>
    public class EasyLogAdapter : ILogger
    {
        // ─────────────────────────────────────────────────────────────
        // Dependencies
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// The EasyLog DLL writer instance.
        /// Injected via constructor to maintain testability.
        /// </summary>
        private readonly IEasyLogWriter _writer;

        // ─────────────────────────────────────────────────────────────
        // Constructor
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Initializes the adapter with an EasyLog writer instance.
        /// </summary>
        /// <param name="writer">
        /// IEasyLogWriter instance obtained from LogWriterFactory.Create().
        /// Must not be null.
        /// </param>
        public EasyLogAdapter(IEasyLogWriter writer)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        // ─────────────────────────────────────────────────────────────
        // ILogger implementation
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Converts a Core LogEntry to an EasyLog LogEntryDto
        /// and delegates writing to the EasyLog DLL.
        /// </summary>
        /// <param name="entry">
        /// Core log entry produced after a file transfer.
        /// Must not be null.
        /// </param>
        public void Log(LogEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            // Convert Core domain type → EasyLog DTO
            LogEntryDto dto = MapToDto(entry);

            // Delegate to EasyLog DLL
            _writer.Write(dto);
        }

        // ─────────────────────────────────────────────────────────────
        // Private mapping
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Maps a Core LogEntry value object to an EasyLog LogEntryDto.
        /// Uses the appropriate factory method based on whether the entry is an error.
        /// </summary>
        /// <param name="entry">Core log entry to convert.</param>
        /// <returns>Equivalent LogEntryDto for the EasyLog DLL.</returns>
        private static LogEntryDto MapToDto(LogEntry entry)
        {
            if (entry.IsError)
            {
                return LogEntryDto.Failure(
                    jobName       : entry.JobName,
                    sourceFile    : entry.SourceFile,
                    destFile      : entry.DestFile,
                    fileSizeBytes : entry.FileSizeBytes);
            }

            return LogEntryDto.Success(
                jobName        : entry.JobName,
                sourceFile     : entry.SourceFile,
                destFile       : entry.DestFile,
                fileSizeBytes  : entry.FileSizeBytes,
                transferTimeMs : entry.TransferTimeMs);
        }
    }
}
