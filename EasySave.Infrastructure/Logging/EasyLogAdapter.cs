// EasySave.Infrastructure/Logging/EasyLogAdapter.cs
// UPDATED v2.0 — MapToDto handles EncryptionTimeMs and IsEncrypted

using EasySave.Core.Interfaces;
using EasySave.Core.ValueObjects;
using EasyLog;
using EasyLog.DTOs;

namespace EasySave.Infrastructure.Logging
{
    /// <summary>
    /// Adapter between Core ILogger and EasyLog DLL IEasyLogWriter.
    /// v2.0: MapToDto now maps EncryptionTimeMs and IsEncrypted.
    /// </summary>
    public class EasyLogAdapter : ILogger
    {
        private readonly IEasyLogWriter _writer;

        public EasyLogAdapter(IEasyLogWriter writer)
            => _writer = writer ?? throw new ArgumentNullException(nameof(writer));

        public void Log(LogEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            _writer.Write(MapToDto(entry));
        }

        public void SetFormat(LogFormat format) => _writer.SetFormat(format);

        private static LogEntryDto MapToDto(LogEntry entry)
        {
            if (entry.EncryptionTimeMs != 0)
            {
                return LogEntryDto.SuccessWithEncryption(
                    entry.JobName, entry.SourceFile, entry.DestFile,
                    entry.FileSizeBytes, entry.TransferTimeMs, entry.EncryptionTimeMs);
            }
            return entry.IsError
                ? LogEntryDto.Failure(entry.JobName, entry.SourceFile, entry.DestFile, entry.FileSizeBytes)
                : LogEntryDto.Success(entry.JobName, entry.SourceFile, entry.DestFile, entry.FileSizeBytes, entry.TransferTimeMs);
        }
    }
}
