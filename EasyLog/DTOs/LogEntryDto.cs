using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace EasyLog.DTOs
{
    /// <summary>
    /// Data Transfer Object used by the EasyLog DLL.
    ///
    /// This object is intentionally independent from EasySave.Core.
    /// EasyLog must remain reusable by other applications.
    ///
    /// The class contains both JSON and XML serialization attributes
    /// because EasyLog v1.1 supports both formats.
    /// </summary>
    [XmlRoot("LogEntry")]
    public class LogEntryDto
    {
        /// <summary>
        /// Timestamp formatted as a human-readable string.
        /// </summary>
        [JsonPropertyName("timestamp")]
        [XmlElement("Timestamp")]
        public string Timestamp { get; set; } = string.Empty;

        /// <summary>
        /// Name of the backup job.
        /// </summary>
        [JsonPropertyName("jobName")]
        [XmlElement("JobName")]
        public string JobName { get; set; } = string.Empty;

        /// <summary>
        /// UNC source file path.
        /// </summary>
        [JsonPropertyName("sourceFile")]
        [XmlElement("SourceFile")]
        public string SourceFile { get; set; } = string.Empty;

        /// <summary>
        /// UNC destination file path.
        /// </summary>
        [JsonPropertyName("destFile")]
        [XmlElement("DestFile")]
        public string DestFile { get; set; } = string.Empty;

        /// <summary>
        /// File size in bytes.
        /// </summary>
        [JsonPropertyName("fileSizeBytes")]
        [XmlElement("FileSizeBytes")]
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// File transfer time in milliseconds.
        /// Negative value means a transfer error occurred.
        /// </summary>
        [JsonPropertyName("transferTimeMs")]
        [XmlElement("TransferTimeMs")]
        public long TransferTimeMs { get; set; }

        /// <summary>
        /// Encryption time in milliseconds.
        ///
        /// This field is used by EasySave v2.0.
        /// In EasySave v1.1, it stays at 0.
        ///
        /// 0  = no encryption.
        /// >0 = encryption duration.
        /// <0 = encryption error.
        /// </summary>
        [JsonPropertyName("cryptoTimeMs")]
        [XmlElement("CryptoTimeMs")]
        public long CryptoTimeMs { get; set; } = 0;

        /// <summary>
        /// Factory method for a successful transfer entry.
        /// </summary>
        public static LogEntryDto Success(
            string jobName,
            string sourceFile,
            string destFile,
            long fileSizeBytes,
            long transferTimeMs,
            long cryptoTimeMs = 0)
        {
            return new LogEntryDto
            {
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                JobName = jobName,
                SourceFile = sourceFile,
                DestFile = destFile,
                FileSizeBytes = fileSizeBytes,
                TransferTimeMs = transferTimeMs,
                CryptoTimeMs = cryptoTimeMs
            };
        }

        /// <summary>
        /// Factory method for a failed transfer entry.
        /// </summary>
        public static LogEntryDto Failure(
            string jobName,
            string sourceFile,
            string destFile,
            long fileSizeBytes,
            long cryptoTimeMs = 0)
        {
            return new LogEntryDto
            {
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                JobName = jobName,
                SourceFile = sourceFile,
                DestFile = destFile,
                FileSizeBytes = fileSizeBytes,
                TransferTimeMs = -1,
                CryptoTimeMs = cryptoTimeMs
            };
        }
    }
}
