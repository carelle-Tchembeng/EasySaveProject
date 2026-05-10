// EasyLog/LogWriterOptions.cs

namespace EasyLog
{
    
    /// Supported log file formats for v1.1.
    
    public enum LogFormat
    {
        Json,
        Xml
    }

    
    /// Configuration settings for the EasyLog writer.
    /// Passed to LogWriterFactory.Create() to build an IEasyLogWriter instance.
    
    public class LogWriterOptions
    {
        
        /// Absolute path to the directory where daily log files will be stored.
        
        public string LogDirectory { get; set; } = string.Empty;

        
        /// The format of the log file (Json or Xml).
        /// Default is Json to maintain backward compatibility with v1.0.
        
        public LogFormat Format { get; set; } = LogFormat.Json;

        
        /// Whether to indent output (JSON/XML) for human readability.
        /// Default: true.
        
        public bool IndentOutput { get; set; } = true;

        
        /// Format string for the file name date. Default: yyyy-MM-dd
        
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        
        /// Format string for the timestamps inside the log.
        
        public string TimestampFormat { get; set; } = "yyyy-MM-dd HH:mm:ss.fff";

        
        /// Validates the current options.
        
        public bool Validate(out string? errorMessage)
        {
            if (string.IsNullOrWhiteSpace(LogDirectory))
            {
                errorMessage = "LogDirectory must not be empty.";
                return false;
            }

            if (LogDirectory.StartsWith(@"C:\Temp", System.StringComparison.OrdinalIgnoreCase) ||
                LogDirectory.StartsWith(@"C:\temp", System.StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = "LogDirectory must not be a temp path (C:\\temp is not allowed).";
                return false;
            }

            errorMessage = null;
            return true;
        }
    }
}