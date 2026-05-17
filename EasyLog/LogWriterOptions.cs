// EasyLog/LogWriterOptions.cs — unchanged from v1.1
namespace EasyLog
{
    public class LogWriterOptions
    {
        public string    LogDirectory   { get; set; } = string.Empty;
        public LogFormat DefaultFormat  { get; set; } = LogFormat.JSON;
        public bool      IndentOutput   { get; set; } = true;
        public bool      AppendNewline  { get; set; } = true;
        public string    DateFormat     { get; set; } = "yyyy-MM-dd";
        public string    TimestampFormat{ get; set; } = "yyyy-MM-dd HH:mm:ss.fff";

        public bool Validate(out string? errorMessage)
        {
            if (string.IsNullOrWhiteSpace(LogDirectory))
            { errorMessage = "LogDirectory must not be empty."; return false; }
            if (LogDirectory.StartsWith(@"C:\temp", StringComparison.OrdinalIgnoreCase))
            { errorMessage = "LogDirectory must not be a temp path."; return false; }
            errorMessage = null;
            return true;
        }
    }
}
