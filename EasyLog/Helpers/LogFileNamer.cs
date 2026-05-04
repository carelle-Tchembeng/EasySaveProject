// EasyLog/Helpers/LogFileNamer.cs

namespace EasyLog.Helpers
{
    /// <summary>
    /// Static utility for generating and parsing daily log file names.
    /// Centralizes all naming logic to ensure consistency across the DLL.
    /// </summary>
    public static class LogFileNamer
    {
        // ─────────────────────────────────────────────────────────────
        // Constants
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// File extension used for all log files.
        /// </summary>
        public const string Extension = ".json";

        // ─────────────────────────────────────────────────────────────
        // Public methods
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the log file name for the specified date.
        /// Example: GetFileName(2024-11-15) → "2024-11-15.json"
        /// </summary>
        /// <param name="date">The date for which to generate a file name.</param>
        /// <param name="dateFormat">
        /// Format string for the date portion.
        /// Default: "yyyy-MM-dd"
        /// </param>
        /// <returns>File name including extension, without directory path.</returns>
        public static string GetFileName(DateTime date, string dateFormat = "yyyy-MM-dd")
        {
            return date.ToString(dateFormat) + Extension;
        }

        /// <summary>
        /// Returns the full absolute path to the log file for the specified date.
        /// Example: GetFullPath("C:\ProgramData\EasySave\logs", 2024-11-15)
        ///          → "C:\ProgramData\EasySave\logs\2024-11-15.json"
        /// </summary>
        /// <param name="logDirectory">Absolute path to the log directory.</param>
        /// <param name="date">The date for which to generate the path.</param>
        /// <param name="dateFormat">Format string for the date portion.</param>
        /// <returns>Full absolute path to the log file.</returns>
        public static string GetFullPath(
            string   logDirectory,
            DateTime date,
            string   dateFormat = "yyyy-MM-dd")
        {
            string fileName = GetFileName(date, dateFormat);
            return Path.Combine(logDirectory, fileName);
        }

        /// <summary>
        /// Attempts to parse the date from a log file name.
        /// Example: ParseDateFromFileName("2024-11-15.json") → DateTime(2024, 11, 15)
        /// </summary>
        /// <param name="fileName">File name to parse (with or without directory path).</param>
        /// <param name="dateFormat">Format string used when the file was created.</param>
        /// <param name="date">Parsed date if successful.</param>
        /// <returns>True if the date was successfully parsed from the file name.</returns>
        public static bool TryParseDateFromFileName(
            string       fileName,
            out DateTime date,
            string       dateFormat = "yyyy-MM-dd")
        {
            // Strip directory path and extension if present
            string baseName = Path.GetFileNameWithoutExtension(
                Path.GetFileName(fileName));

            return DateTime.TryParseExact(
                baseName,
                dateFormat,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out date);
        }
    }
}
