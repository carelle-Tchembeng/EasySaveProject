// EasyLog/Formatters/LogFormatter.cs

using EasyLog.DTOs;
using System.Text.Json;

namespace EasyLog.Formatters
{
    /// <summary>
    /// Static utility for formatting log entry data before serialization.
    /// Centralizes all display formatting to ensure consistency across log files.
    /// </summary>
    public static class LogFormatter
    {
        // ─────────────────────────────────────────────────────────────
        // Default serializer options — reused for performance
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Default JSON serializer options used by FormatEntry().
        /// WriteIndented is true for Notepad readability as required by the specification.
        /// </summary>
        private static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        // ─────────────────────────────────────────────────────────────
        // Entry formatting
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Serializes a LogEntryDto to an indented JSON string ready to append to the log file.
        /// </summary>
        /// <param name="entry">The log entry to format.</param>
        /// <param name="options">Optional custom serializer options. Uses indented defaults if null.</param>
        /// <returns>Indented JSON string representation of the entry.</returns>
        public static string FormatEntry(LogEntryDto entry, JsonSerializerOptions? options = null)
        {
            return JsonSerializer.Serialize(entry, options ?? DefaultOptions);
        }

        // ─────────────────────────────────────────────────────────────
        // Individual field formatters
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Formats a DateTime as a log timestamp string.
        /// Default format: "yyyy-MM-dd HH:mm:ss.fff" for millisecond precision.
        /// </summary>
        /// <param name="dateTime">The date and time to format.</param>
        /// <param name="format">Custom format string. Uses default if null.</param>
        /// <returns>Formatted timestamp string.</returns>
        public static string FormatTimestamp(DateTime dateTime, string? format = null)
        {
            return dateTime.ToString(format ?? "yyyy-MM-dd HH:mm:ss.fff");
        }

        /// <summary>
        /// Formats a file size in bytes as a human-readable string.
        /// Example: 1024 → "1.00 KB", 1048576 → "1.00 MB"
        /// Values under 1024 are shown as bytes: 512 → "512 B"
        /// </summary>
        /// <param name="bytes">File size in bytes.</param>
        /// <returns>Human-readable size string.</returns>
        public static string FormatSize(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes} B";

            if (bytes < 1024 * 1024)
                return $"{bytes / 1024.0:F2} KB";

            if (bytes < 1024 * 1024 * 1024)
                return $"{bytes / (1024.0 * 1024):F2} MB";

            return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
        }

        /// <summary>
        /// Formats a transfer duration in milliseconds as a human-readable string.
        /// Negative values are shown with an ERROR prefix as required by the specification.
        /// Example: 128 → "128 ms", -1 → "ERROR (-1 ms)"
        /// </summary>
        /// <param name="transferTimeMs">Transfer time in milliseconds. Negative indicates an error.</param>
        /// <returns>Formatted duration string.</returns>
        public static string FormatDuration(long transferTimeMs)
        {
            if (transferTimeMs < 0)
                return $"ERROR ({transferTimeMs} ms)";

            if (transferTimeMs < 1000)
                return $"{transferTimeMs} ms";

            return $"{transferTimeMs / 1000.0:F2} s";
        }
    }
}
