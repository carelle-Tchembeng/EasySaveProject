// EasyLog/IEasyLogWriter.cs — unchanged from v1.1
using EasyLog.DTOs;
namespace EasyLog
{
    public interface IEasyLogWriter
    {
        void Write(LogEntryDto entry);
        string GetLogFilePath(DateTime date);
        void SetFormat(LogFormat format);
    }
}
