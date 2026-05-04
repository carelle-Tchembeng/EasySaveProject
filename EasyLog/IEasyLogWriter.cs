// EasyLog/IEasyLogWriter.cs

using EasyLog.DTOs;

namespace EasyLog
{
    /// <summary>
    /// Public contract of the EasyLog DLL.
    /// This is the only interface that external consumers should reference.
    /// All future versions of EasyLog must remain backward compatible with this interface:
    /// — existing methods must not be removed
    /// — existing method signatures must not change
    /// — new methods may be added in future versions
    /// </summary>
    public interface IEasyLogWriter
    {
        /// <summary>
        /// Writes a log entry to the daily log file.
        /// The log file is named after the current date (e.g. 2024-11-15.json).
        /// A new file is created automatically at the start of each day.
        /// This method is thread-safe.
        /// </summary>
        /// <param name="entry">The log entry to write. Must not be null.</param>
        void Write(LogEntryDto entry);

        /// <summary>
        /// Returns the full path of the log file for the specified date.
        /// Useful for support teams to quickly locate the correct log file.
        /// The file may not exist yet if no entries have been written for that date.
        /// </summary>
        /// <param name="date">The date for which to retrieve the log file path.</param>
        /// <returns>Full absolute path to the log file for the given date.</returns>
        string GetLogFilePath(DateTime date);
    }
}
