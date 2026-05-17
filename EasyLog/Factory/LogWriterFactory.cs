// EasyLog/Factory/LogWriterFactory.cs — unchanged from v1.1
using EasyLog.Writers;

namespace EasyLog.Factory
{
    public static class LogWriterFactory
    {
        public static IEasyLogWriter Create(LogWriterOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (!options.Validate(out string? error))
                throw new ArgumentException($"Invalid LogWriterOptions: {error}", nameof(options));
            return EasyLogWriter.GetInstance(options);
        }

        public static IEasyLogWriter Create(string logDirectory) =>
            Create(new LogWriterOptions { LogDirectory = logDirectory, DefaultFormat = LogFormat.JSON });

        public static IEasyLogWriter Create(string logDirectory, LogFormat format) =>
            Create(new LogWriterOptions { LogDirectory = logDirectory, DefaultFormat = format });
    }
}
