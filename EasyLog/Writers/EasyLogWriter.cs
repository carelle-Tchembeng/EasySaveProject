// EasyLog/Writers/EasyLogWriter.cs

using EasyLog.DTOs;
using EasyLog.Formatters;
using EasyLog.Helpers;

namespace EasyLog.Writers
{
    /// <summary>
    /// Thread-safe singleton implementation of IEasyLogWriter.
    /// Writes log entries to daily JSON files in the configured directory.
    ///
    /// Singleton pattern is justified here because:
    /// — multiple backup jobs may run sequentially and must share the same log file
    /// — the file write lock (_lock) must be shared across all callers
    /// — instantiating multiple writers would cause file access conflicts
    ///
    /// Usage: EasyLogWriter.GetInstance(options) → IEasyLogWriter
    /// </summary>
    public sealed class EasyLogWriter : IEasyLogWriter
    {
        // ─────────────────────────────────────────────────────────────
        // Singleton infrastructure
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// The single instance of EasyLogWriter.
        /// Null until GetInstance() is called for the first time.
        /// </summary>
        private static EasyLogWriter? _instance;

        /// <summary>
        /// Lock object used to make singleton initialization thread-safe.
        /// Separate from _writeLock to avoid deadlocks.
        /// </summary>
        private static readonly object _instanceLock = new object();

        /// <summary>
        /// Lock object used to serialize file write operations.
        /// Prevents concurrent writes from corrupting the log file.
        /// </summary>
        private readonly object _writeLock = new object();

        // ─────────────────────────────────────────────────────────────
        // Configuration
        // ─────────────────────────────────────────────────────────────

        private readonly LogWriterOptions _options;

        // ─────────────────────────────────────────────────────────────
        // Constructor — private to enforce singleton pattern
        // ─────────────────────────────────────────────────────────────

        private EasyLogWriter(LogWriterOptions options)
        {
            _options = options;
        }

        // ─────────────────────────────────────────────────────────────
        // Singleton accessor
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the singleton EasyLogWriter instance.
        /// Creates it on the first call using the provided options.
        /// On subsequent calls, the existing instance is returned
        /// and the options parameter is ignored.
        /// Thread-safe via double-checked locking.
        /// </summary>
        /// <param name="options">Configuration options for the writer.</param>
        /// <returns>The singleton IEasyLogWriter instance.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if options are invalid on first initialization.
        /// </exception>
        public static IEasyLogWriter GetInstance(LogWriterOptions options)
        {
            if (_instance != null) return _instance;

            lock (_instanceLock)
            {
                if (_instance != null) return _instance;

                // Validate options before creating the instance
                if (!options.Validate(out string? error))
                    throw new ArgumentException($"Invalid LogWriterOptions: {error}");

                _instance = new EasyLogWriter(options);
            }

            return _instance;
        }

        /// <summary>
        /// Resets the singleton instance.
        /// FOR TESTING PURPOSES ONLY — do not call in production code.
        /// </summary>
        internal static void ResetForTesting()
        {
            lock (_instanceLock)
            {
                _instance = null;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // IEasyLogWriter implementation
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Writes a log entry to today's log file.
        /// Creates the log directory and file if they do not exist.
        /// Appends to an existing file if one already exists for today.
        /// This method is thread-safe.
        /// </summary>
        /// <param name="entry">The log entry to write. Must not be null.</param>
        public void Write(LogEntryDto entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            // Ensure the log directory exists before trying to write
            EnsureLogDirectory();

            // Build the full path to today's log file
            string filePath = LogFileNamer.GetFullPath(
                _options.LogDirectory,
                DateTime.Now,
                _options.DateFormat);

            // Serialize the entry to indented JSON
            string jsonEntry = LogFormatter.FormatEntry(entry);

            // Append newline separator if configured
            if (_options.AppendNewline)
                jsonEntry += Environment.NewLine;

            // Serialize file access to prevent concurrent write conflicts
            lock (_writeLock)
            {
                AppendToFile(filePath, jsonEntry);
            }
        }

        /// <summary>
        /// Returns the full path of the log file for the specified date.
        /// The file may not exist yet if no entries have been written for that date.
        /// </summary>
        /// <param name="date">Target date.</param>
        /// <returns>Full absolute path to the log file.</returns>
        public string GetLogFilePath(DateTime date)
        {
            return LogFileNamer.GetFullPath(
                _options.LogDirectory,
                date,
                _options.DateFormat);
        }

        // ─────────────────────────────────────────────────────────────
        // Private helpers
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates the log directory if it does not already exist.
        /// Uses CreateDirectory which is safe to call even if the directory exists.
        /// </summary>
        private void EnsureLogDirectory()
        {
            if (!Directory.Exists(_options.LogDirectory))
            {
                Directory.CreateDirectory(_options.LogDirectory);
            }
        }

        /// <summary>
        /// Appends a JSON string to the specified file.
        /// Creates the file if it does not exist.
        /// Uses File.AppendAllText which is atomic at the OS level for small writes.
        /// </summary>
        /// <param name="filePath">Full path of the target log file.</param>
        /// <param name="content">JSON string to append.</param>
        private static void AppendToFile(string filePath, string content)
        {
            File.AppendAllText(filePath, content, System.Text.Encoding.UTF8);
        }
    }
}
