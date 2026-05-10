using EasyLog;
using EasyLog.DTOs;
using EasyLog.Factory;
using EasySave.Core.Interfaces;
using EasySave.Core.ValueObjects;
using EasySave.Infrastructure.Configuration;

namespace EasySave.Infrastructure.Logging
{
    /// <summary>
    /// Adapter between the Core logger abstraction and the EasyLog DLL.
    ///
    /// The Core layer only knows ILogger and LogEntry.
    /// It must not depend directly on EasyLog.
    ///
    /// This adapter converts:
    /// EasySave.Core.ValueObjects.LogEntry
    /// into:
    /// EasyLog.DTOs.LogEntryDto
    ///
    /// It also creates the correct EasyLog writer depending on the current
    /// application settings, especially the selected log format: JSON or XML.
    /// </summary>
    public class EasyLogAdapter : ILogger
    {
        private readonly AppSettings _settings;

        public EasyLogAdapter(AppSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        /// Writes a domain log entry using the EasyLog library.
        ///
        /// The writer is created from current settings each time.
        /// This allows the user to switch log format during application usage
        /// without restarting the application.
        /// </summary>
        public void Log(LogEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            LogEntryDto dto = MapToDto(entry);

            IEasyLogWriter writer = CreateWriterFromCurrentSettings();

            writer.Write(dto);
        }

        /// <summary>
        /// Creates an EasyLog writer according to the current settings.
        /// If the configured format is invalid, JSON is used as fallback.
        /// </summary>
        private IEasyLogWriter CreateWriterFromCurrentSettings()
        {
            bool isValidFormat = Enum.TryParse<LogFormat>(
                _settings.LogFormat,
                ignoreCase: true,
                out LogFormat parsedFormat);

            var options = new LogWriterOptions
            {
                LogDirectory = _settings.LogDirectory,
                Format = isValidFormat ? parsedFormat : LogFormat.Json,
                IndentOutput = true
            };

            return LogWriterFactory.Create(options);
        }

        /// <summary>
        /// Converts the Core LogEntry object into an EasyLog DTO.
        /// </summary>
        private static LogEntryDto MapToDto(LogEntry entry)
        {
            if (entry.IsError)
            {
                return LogEntryDto.Failure(
                    jobName: entry.JobName,
                    sourceFile: entry.SourceFile,
                    destFile: entry.DestFile,
                    fileSizeBytes: entry.FileSizeBytes,
                    cryptoTimeMs: entry.CryptoTimeMs);
            }

            return LogEntryDto.Success(
                jobName: entry.JobName,
                sourceFile: entry.SourceFile,
                destFile: entry.DestFile,
                fileSizeBytes: entry.FileSizeBytes,
                transferTimeMs: entry.TransferTimeMs,
                cryptoTimeMs: entry.CryptoTimeMs);
        }
    }
}
