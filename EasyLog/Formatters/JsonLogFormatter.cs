// EasyLog/Formatters/JsonLogFormatter.cs — unchanged from v1.1
using EasyLog.DTOs;
using System.Text.Json;
namespace EasyLog.Formatters
{
    public class JsonLogFormatter : ILogFormatter
    {
        private readonly JsonSerializerOptions _options;
        public JsonLogFormatter(bool indented = true) =>
            _options = new JsonSerializerOptions { WriteIndented = indented };
        public string Format(LogEntryDto entry) => JsonSerializer.Serialize(entry, _options);
        public string GetFileExtension() => ".json";
    }
}
