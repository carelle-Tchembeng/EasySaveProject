using EasyLog.Formatters;
using EasyLog.Writers;

namespace EasyLog.Factory
{
    /// <summary>
    /// Factory responsible for creating EasyLog writers.
    ///
    /// The factory chooses the correct formatting strategy
    /// according to LogWriterOptions.Format.
    /// </summary>
    public static class LogWriterFactory
    {
        /// <summary>
        /// Creates an EasyLog writer using the provided options.
        /// </summary>
        public static IEasyLogWriter Create(LogWriterOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (!options.Validate(out string? error))
                throw new ArgumentException($"Invalid LogWriterOptions: {error}", nameof(options));

            ILogFormatterStrategy strategy = options.Format switch
            {
                LogFormat.Xml => new XmlLogFormatterStrategy(options.IndentOutput),
                LogFormat.Json => new JsonLogFormatterStrategy(options.IndentOutput),

                // Defensive fallback.
                _ => new JsonLogFormatterStrategy(options.IndentOutput)
            };

            return new EasyLogWriter(options, strategy);
        }

        /// <summary>
        /// Creates a JSON writer using the specified directory.
        /// This overload preserves backward compatibility with v1.0 usage.
        /// </summary>
        public static IEasyLogWriter Create(string logDirectory)
        {
            return Create(new LogWriterOptions
            {
                LogDirectory = logDirectory,
                Format = LogFormat.Json
            });
        }
    }
}
