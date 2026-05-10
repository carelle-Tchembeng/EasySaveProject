using EasyLog.DTOs;
using EasyLog.Formatters;
using EasyLog.Helpers;

namespace EasyLog.Writers
{
    /// <summary>
    /// Default EasyLog writer implementation.
    ///
    /// This class writes daily log files using the selected formatter strategy.
    ///
    /// Important v1.1 design choice:
    /// The writer is not a singleton anymore.
    /// This avoids format conflicts when switching between JSON and XML.
    /// </summary>
    public sealed class EasyLogWriter : IEasyLogWriter
    {
        /// <summary>
        /// Shared lock used to protect concurrent write access.
        /// </summary>
        private static readonly object WriteLock = new();

        private readonly LogWriterOptions _options;
        private readonly ILogFormatterStrategy _strategy;

        public EasyLogWriter(LogWriterOptions options, ILogFormatterStrategy strategy)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));

            EnsureLogDirectory();
        }

        /// <summary>
        /// Writes a log entry to the daily log file.
        ///
        /// To keep JSON and XML files valid, the method:
        /// 1. Loads existing entries.
        /// 2. Adds the new entry.
        /// 3. Rewrites the full document.
        ///
        /// This is simpler and safer for v1.1 than appending raw fragments.
        /// </summary>
        public void Write(LogEntryDto entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            lock (WriteLock)
            {
                string filePath = GetLogFilePath(DateTime.Now);

                List<LogEntryDto> entries = LoadExistingEntries(filePath);

                entries.Add(entry);

                string content = _strategy.FormatEntries(entries);

                File.WriteAllText(filePath, content, System.Text.Encoding.UTF8);
            }
        }

        /// <summary>
        /// Returns the full path of the daily log file for a given date.
        /// </summary>
        public string GetLogFilePath(DateTime date)
        {
            return LogFileNamer.GetFullPath(
                _options.LogDirectory,
                date,
                _strategy.Extension,
                _options.DateFormat);
        }

        /// <summary>
        /// Loads entries already present in the daily log file.
        /// </summary>
        private List<LogEntryDto> LoadExistingEntries(string filePath)
        {
            if (!File.Exists(filePath))
                return new List<LogEntryDto>();

            try
            {
                string content = File.ReadAllText(filePath, System.Text.Encoding.UTF8);

                return _strategy.ParseEntries(content).ToList();
            }
            catch
            {
                // Logging should never crash the whole application.
                // If the current log file is corrupted, we restart with an empty list.
                return new List<LogEntryDto>();
            }
        }

        /// <summary>
        /// Ensures that the log directory exists before writing.
        /// </summary>
        private void EnsureLogDirectory()
        {
            if (!Directory.Exists(_options.LogDirectory))
            {
                Directory.CreateDirectory(_options.LogDirectory);
            }
        }
    }
}
