// EasySave.Core/Interfaces/ILogger.cs

using EasySave.Core.ValueObjects;

namespace EasySave.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for writing log entries to the daily log file.
    /// Implemented by EasyLogAdapter in the Infrastructure layer,
    /// which delegates to the EasyLog.dll library.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Writes a log entry to the daily log file.
        /// The log file is named after the current date (e.g. 2024-11-15.json).
        /// A new file is created automatically each day.
        /// </summary>
        /// <param name="entry">
        /// The log entry to write. Contains source/dest paths, file size,
        /// transfer time, and timestamp. Use LogEntry.Success() or LogEntry.Failure()
        /// factory methods to create entries.
        /// </param>
        void Log(LogEntry entry);
    }
}
