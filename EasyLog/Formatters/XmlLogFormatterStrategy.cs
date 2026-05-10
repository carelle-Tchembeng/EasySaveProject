using EasyLog.DTOs;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace EasyLog.Formatters
{
    /// <summary>
    /// Formats log entries as a valid XML document.
    ///
    /// Example:
    /// <LogEntries>
    ///   <LogEntry>
    ///     <Timestamp>...</Timestamp>
    ///   </LogEntry>
    /// </LogEntries>
    /// </summary>
    public class XmlLogFormatterStrategy : ILogFormatterStrategy
    {
        private readonly XmlSerializer _serializer;
        private readonly XmlWriterSettings _writerSettings;
        private readonly XmlSerializerNamespaces _namespaces;

        public string Extension => ".xml";

        public XmlLogFormatterStrategy(bool indent)
        {
            _serializer = new XmlSerializer(typeof(LogEntryCollection));

            _writerSettings = new XmlWriterSettings
            {
                Indent = indent,

                // Keep the XML declaration.
                // The custom Utf8StringWriter below ensures the declaration matches UTF-8.
                OmitXmlDeclaration = false
            };

            // Remove default xmlns attributes for cleaner XML output.
            _namespaces = new XmlSerializerNamespaces();
            _namespaces.Add(string.Empty, string.Empty);
        }

        /// <summary>
        /// Serializes the full list of entries as a valid XML document.
        /// </summary>
        public string FormatEntries(IReadOnlyList<LogEntryDto> entries)
        {
            var collection = new LogEntryCollection
            {
                Entries = entries.ToList()
            };

            using var stringWriter = new Utf8StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, _writerSettings);

            _serializer.Serialize(xmlWriter, collection, _namespaces);

            return stringWriter.ToString();
        }

        /// <summary>
        /// Parses an existing XML log file.
        ///
        /// If the file is empty or corrupted, an empty list is returned.
        /// </summary>
        public IReadOnlyList<LogEntryDto> ParseEntries(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new List<LogEntryDto>();

            try
            {
                using var stringReader = new StringReader(content);

                var result = _serializer.Deserialize(stringReader) as LogEntryCollection;

                return result?.Entries ?? new List<LogEntryDto>();
            }
            catch
            {
                return new List<LogEntryDto>();
            }
        }

        /// <summary>
        /// StringWriter uses UTF-16 by default.
        /// This custom implementation forces UTF-8 in the XML declaration.
        /// </summary>
        private sealed class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }
    }

    /// <summary>
    /// XML root collection for log entries.
    ///
    /// This wrapper is required because an XML document must have one root element.
    /// Multiple adjacent <LogEntry> fragments would not be valid XML.
    /// </summary>
    [XmlRoot("LogEntries")]
    public class LogEntryCollection
    {
        [XmlElement("LogEntry")]
        public List<LogEntryDto> Entries { get; set; } = new();
    }
}
