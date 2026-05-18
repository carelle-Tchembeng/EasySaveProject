// EasySave.LogServer/Services/ILogStorageService.cs

using EasySave.LogServer.DTOs;

namespace EasySave.LogServer.Services
{
    /// <summary>
    /// Contract for persisting remote log entries to the centralised log store.
    /// </summary>
    public interface ILogStorageService
    {
        /// <summary>
        /// Appends a log entry to the daily log file.
        /// Implementations must be thread-safe — multiple EasySave instances
        /// may call this method concurrently.
        /// </summary>
        Task AppendLogAsync(LogEntryRemoteDto entry);

        /// <summary>Returns the absolute path of the log file for the given date.</summary>
        string GetLogFilePath(DateTime date);
    }
}
