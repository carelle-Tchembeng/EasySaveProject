// EasyLog/LogWriterOptions.cs

namespace EasyLog
{
    /// <summary>
    /// Configuration settings for the EasyLog writer.
    /// Passed to LogWriterFactory.Create() to build an IEasyLogWriter instance.
    /// All paths must be absolute. Relative paths are not supported.
    /// </summary>
    public class LogWriterOptions
    {
        // ─────────────────────────────────────────────────────────────
        // Required settings
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Absolute path to the directory where daily log files will be stored.
        /// The directory is created automatically if it does not exist.
        /// Must not be a temp path (e.g. C:\temp) as required by the specification.
        /// Example: C:\ProgramData\EasySave\logs
        /// </summary>
        public string LogDirectory { get; set; } = string.Empty;

        // ─────────────────────────────────────────────────────────────
        // Optional settings — with sensible defaults
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Whether to indent JSON output for human readability.
        /// Default: true (required by specification for Notepad readability).
        /// </summary>
        public bool IndentJson { get; set; } = true;

        /// <summary>
        /// Whether to append a newline character after each JSON entry.
        /// Default: true (required for correct formatting in Notepad).
        /// </summary>
        public bool AppendNewline { get; set; } = true;

        /// <summary>
        /// Format string for the daily log file date portion.
        /// Default: "yyyy-MM-dd" → produces files like "2024-11-15.json".
        /// Must produce a valid file name (no slashes or special characters).
        /// </summary>
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        /// <summary>
        /// Format string for entry timestamps inside the log file.
        /// Default: "yyyy-MM-dd HH:mm:ss.fff" for millisecond precision.
        /// </summary>
        public string TimestampFormat { get; set; } = "yyyy-MM-dd HH:mm:ss.fff";

        // ─────────────────────────────────────────────────────────────
        // Validation
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Validates the options and returns true if they are usable.
        /// Checks that LogDirectory is non-empty and format strings are valid.
        /// </summary>
        /// <param name="errorMessage">
        /// Set to a human-readable error description if validation fails.
        /// Null if validation succeeds.
        /// </param>
        public bool Validate(out string? errorMessage)
        {
            if (string.IsNullOrWhiteSpace(LogDirectory))
            {
                errorMessage = "LogDirectory must not be empty.";
                return false;
            }

            // Reject temp paths as required by the specification
            if (LogDirectory.StartsWith(@"C:\temp", StringComparison.OrdinalIgnoreCase) ||
                LogDirectory.StartsWith(@"C:\Temp", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = "LogDirectory must not be a temp path (C:\\temp is not allowed).";
                return false;
            }

            if (string.IsNullOrWhiteSpace(DateFormat))
            {
                errorMessage = "DateFormat must not be empty.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(TimestampFormat))
            {
                errorMessage = "TimestampFormat must not be empty.";
                return false;
            }

            // Verify format strings produce valid output
            try
            {
                string testDate      = DateTime.Now.ToString(DateFormat);
                string testTimestamp = DateTime.Now.ToString(TimestampFormat);

                // A valid file name produced by DateFormat must not contain path separators
                if (testDate.Contains('/') || testDate.Contains('\\'))
                {
                    errorMessage = $"DateFormat '{DateFormat}' produces an invalid file name: '{testDate}'.";
                    return false;
                }
            }
            catch (FormatException ex)
            {
                errorMessage = $"Invalid format string: {ex.Message}";
                return false;
            }

            errorMessage = null;
            return true;
        }
    }
}
