// EasyLog/Formatters/ILogFormatter.cs — unchanged from v1.1
using EasyLog.DTOs;
namespace EasyLog.Formatters
{
    public interface ILogFormatter
    {
        string Format(LogEntryDto entry);
        string GetFileExtension();
    }
}
