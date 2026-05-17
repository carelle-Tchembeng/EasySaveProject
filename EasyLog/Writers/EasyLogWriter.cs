// EasyLog/Writers/EasyLogWriter.cs — unchanged from v1.1 (formatter handles new fields)
using EasyLog.DTOs;
using EasyLog.Formatters;
using EasyLog.Helpers;

namespace EasyLog.Writers
{
    /// <summary>
    /// Thread-safe singleton log writer.
    /// v2.0: no changes — LogEntryDto and formatters handle new fields automatically.
    /// </summary>
    public sealed class EasyLogWriter : IEasyLogWriter
    {
        private static EasyLogWriter?     _instance;
        private static readonly object    _instanceLock = new object();
        private readonly object           _writeLock    = new object();
        private readonly LogWriterOptions _options;
        private ILogFormatter             _formatter;

        private EasyLogWriter(LogWriterOptions options)
        {
            _options   = options;
            _formatter = ResolveFormatter(options.DefaultFormat);
        }

        public static IEasyLogWriter GetInstance(LogWriterOptions options)
        {
            if (_instance != null) return _instance;
            lock (_instanceLock)
            {
                if (_instance != null) return _instance;
                if (!options.Validate(out string? error))
                    throw new ArgumentException($"Invalid LogWriterOptions: {error}");
                _instance = new EasyLogWriter(options);
            }
            return _instance;
        }

        internal static void ResetForTesting()
        {
            lock (_instanceLock) { _instance = null; }
        }

        public void Write(LogEntryDto entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            lock (_writeLock)
            {
                if (!Directory.Exists(_options.LogDirectory))
                    Directory.CreateDirectory(_options.LogDirectory);

                string filePath = LogFileNamer.GetFullPath(
                    _options.LogDirectory, DateTime.Now,
                    _options.DateFormat, _formatter.GetFileExtension());

                string content = _formatter.Format(entry);
                if (_options.AppendNewline) content += Environment.NewLine;

                File.AppendAllText(filePath, content, System.Text.Encoding.UTF8);
            }
        }

        public string GetLogFilePath(DateTime date)
        {
            lock (_writeLock)
                return LogFileNamer.GetFullPath(_options.LogDirectory, date,
                    _options.DateFormat, _formatter.GetFileExtension());
        }

        public void SetFormat(LogFormat format)
        {
            lock (_writeLock) { _formatter = ResolveFormatter(format); }
        }

        private ILogFormatter ResolveFormatter(LogFormat format) => format switch
        {
            LogFormat.XML => new XmlLogFormatter(_options.IndentOutput),
            _             => new JsonLogFormatter(_options.IndentOutput)
        };
    }
}
