using EasyLog.DTOs;
using System.Text.Json;

namespace EasyLog.Formatters
{
    /// <summary>
    /// Formats log entries as a valid JSON array.
    ///
    /// Example:
    /// [
    ///   { "timestamp": "...", "jobName": "..." },
    ///   { "timestamp": "...", "jobName": "..." }
    /// ]
    /// </summary>
    public class JsonLogFormatterStrategy : ILogFormatterStrategy
    {
        private readonly JsonSerializerOptions _options;

        public string Extension => ".json";

        public JsonLogFormatterStrategy(bool indent)
        {
            _options = new JsonSerializerOptions
            {
                WriteIndented = indent
            };
        }

        /// <summary>
        /// Serializes the full list of entries as a JSON array.
        /// </summary>
        public string FormatEntries(IReadOnlyList<LogEntryDto> entries)
        {
            return JsonSerializer.Serialize(entries, _options);
        }

        /// <summary>
        /// Reads an existing JSON array from disk.
        ///
        /// If the file is empty or corrupted, an empty list is returned
        /// to avoid crashing the application during logging.
        /// </summary>
        public IReadOnlyList<LogEntryDto> ParseEntries(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new List<LogEntryDto>();

            try
            {
                return JsonSerializer.Deserialize<List<LogEntryDto>>(content, _options)
                    ?? new List<LogEntryDto>();
            }
            catch
            {
                return new List<LogEntryDto>();
            }
        }
    }
}
