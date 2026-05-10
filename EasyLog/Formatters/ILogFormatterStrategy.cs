using EasyLog.DTOs;

namespace EasyLog.Formatters
{
    /// <summary>
    /// Defines a formatting strategy for EasyLog.
    ///
    /// Each format, JSON or XML, must be able to:
    /// - provide its file extension
    /// - serialize a list of log entries
    /// - parse existing log entries from a daily log file
    ///
    /// Parsing is required because EasyLog writes valid JSON/XML files
    /// by storing entries as a collection instead of appending invalid fragments.
    /// </summary>
    public interface ILogFormatterStrategy
    {
        /// <summary>
        /// File extension associated with the format.
        /// Example: ".json" or ".xml".
        /// </summary>
        string Extension { get; }

        /// <summary>
        /// Serializes all log entries into a complete valid document.
        /// </summary>
        string FormatEntries(IReadOnlyList<LogEntryDto> entries);

        /// <summary>
        /// Parses existing entries from a previously written log file.
        /// </summary>
        IReadOnlyList<LogEntryDto> ParseEntries(string content);
    }
}
