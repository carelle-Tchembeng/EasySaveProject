// EasySave.Core/Entities/AppConfiguration.cs
// NEW v2.0 — per corrected diagram

namespace EasySave.Core.Entities
{
    /// <summary>
    /// All user-configurable settings for EasySave v2.0.
    /// Loaded at startup via IAppConfigRepository.
    /// </summary>
    public class AppConfiguration
    {
        /// <summary>Full path to CryptoSoft.exe. Empty = encryption disabled.</summary>
        public string CryptoSoftPath { get; set; } = string.Empty;

        /// <summary>Process name to detect. Empty = detection disabled. Example: "calc"</summary>
        public string BusinessSoftwareName { get; set; } = string.Empty;

        /// <summary>Extensions to encrypt (include leading dot). Example: [".docx", ".pdf"]</summary>
        public List<string> EncryptedExtensions { get; set; } = new();

        /// <summary>"JSON" or "XML". Parsed to EasyLog.LogFormat at runtime.</summary>
        public string LogFormat { get; set; } = "JSON";

        /// <summary>UI language code: "fr" or "en".</summary>
        public string DefaultLanguage { get; set; } = "en";

        /// <summary>
        /// Returns true if the given file extension is in the encrypted list.
        /// Case-insensitive. Accepts ".pdf" or "pdf".
        /// </summary>
        public bool IsExtensionEncrypted(string ext)
        {
            if (string.IsNullOrWhiteSpace(ext)) return false;
            string normalized = ext.StartsWith('.') ? ext.ToLowerInvariant() : $".{ext.ToLowerInvariant()}";
            return EncryptedExtensions.Any(e => e.ToLowerInvariant() == normalized);
        }

        /// <summary>Returns true when BusinessSoftwareName is set.</summary>
        public bool IsBusinessSoftwareDetectionEnabled()
            => !string.IsNullOrWhiteSpace(BusinessSoftwareName);
    }
}
