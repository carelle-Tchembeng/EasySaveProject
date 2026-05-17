// EasyLog/Formatters/XmlLogFormatter.cs
// UPDATED v2.0 — serializes new EncryptionTimeMs and IsEncrypted fields

using EasyLog.DTOs;
using System.Text;
using System.Xml;

namespace EasyLog.Formatters
{
    /// <summary>
    /// Serializes log entries to indented XML.
    /// v2.0: added EncryptionTimeMs and IsEncrypted XML elements.
    /// </summary>
    public class XmlLogFormatter : ILogFormatter
    {
        private readonly bool _indented;

        public XmlLogFormatter(bool indented = true) => _indented = indented;

        public string Format(LogEntryDto entry)
        {
            var settings = new XmlWriterSettings
            {
                Indent             = _indented,
                IndentChars        = "  ",
                OmitXmlDeclaration = true,
                Encoding           = Encoding.UTF8
            };

            var builder = new StringBuilder();
            using (var writer = XmlWriter.Create(builder, settings))
            {
                writer.WriteStartElement("LogEntry");
                writer.WriteElementString("Timestamp",        entry.Timestamp);
                writer.WriteElementString("JobName",          entry.JobName);
                writer.WriteElementString("SourceFile",       entry.SourceFile);
                writer.WriteElementString("DestFile",         entry.DestFile);
                writer.WriteElementString("FileSizeBytes",    entry.FileSizeBytes.ToString());
                writer.WriteElementString("TransferTimeMs",   entry.TransferTimeMs.ToString());
                writer.WriteElementString("EncryptionTimeMs", entry.EncryptionTimeMs.ToString()); // NEW v2.0
                writer.WriteElementString("IsError",          entry.IsError.ToString().ToLower());
                writer.WriteElementString("IsEncrypted",      entry.IsEncrypted.ToString().ToLower()); // NEW v2.0
                writer.WriteEndElement();
            }
            return builder.ToString();
        }

        private string BuildXmlElement(LogEntryDto entry) => Format(entry);
        public string GetFileExtension() => ".xml";
    }
}
