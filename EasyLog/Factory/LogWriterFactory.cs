// EasyLog/Factory/LogWriterFactory.cs

using EasyLog.Writers;

namespace EasyLog.Factory
{
    /// <summary>
    /// Factory for creating IEasyLogWriter instances.
    /// This is the recommended entry point for all consumers of the EasyLog DLL.
    ///
    /// Usage:
    ///   var options = new LogWriterOptions { LogDirectory = @"C:\ProgramData\EasySave\logs" };
    ///   IEasyLogWriter writer = LogWriterFactory.Create(options);
    ///   writer.Write(LogEntryDto.Success(...));
    ///
    /// The factory ensures:
    /// — options are validated before the writer is created
    /// — consumers always receive an IEasyLogWriter interface, never a concrete type
    /// — the singleton lifecycle of EasyLogWriter is fully encapsulated
    /// </summary>
    public static class LogWriterFactory
    {
        /// <summary>
        /// Creates or returns the singleton IEasyLogWriter configured with the given options.
        /// On the first call, validates options and initializes the writer.
        /// On subsequent calls, returns the existing instance (options are ignored).
        /// </summary>
        /// <param name="options">
        /// Configuration for the log writer.
        /// LogDirectory is mandatory. All other settings have sensible defaults.
        /// </param>
        /// <returns>A thread-safe IEasyLogWriter instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if options is null.</exception>
        /// <exception cref="ArgumentException">Thrown if options fail validation.</exception>
        public static IEasyLogWriter Create(LogWriterOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options),
                    "LogWriterOptions must not be null.");

            // Validate before delegating to the singleton
            if (!options.Validate(out string? error))
                throw new ArgumentException($"Invalid LogWriterOptions: {error}", nameof(options));

            return EasyLogWriter.GetInstance(options);
        }

        /// <summary>
        /// Creates an IEasyLogWriter using only the required log directory path.
        /// All other settings use their defaults (indented JSON, daily files, etc.).
        /// </summary>
        /// <param name="logDirectory">
        /// Absolute path to the directory where log files will be stored.
        /// Example: @"C:\ProgramData\EasySave\logs"
        /// </param>
        /// <returns>A thread-safe IEasyLogWriter instance.</returns>
        public static IEasyLogWriter Create(string logDirectory)
        {
            return Create(new LogWriterOptions
            {
                LogDirectory = logDirectory
            });
        }
    }
}
