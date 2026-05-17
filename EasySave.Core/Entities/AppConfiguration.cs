// EasySave.Core/Entities/AppConfiguration.cs
// UPDATED v3.0 — adds PriorityExtensions, MaxParallelFileSizeKb, LogStorageMode, LogServerUrl

namespace EasySave.Core.Entities
{
    /// <summary>
    /// All user-configurable settings for EasySave.
    /// v3.0 adds: priority extensions, large-file threshold, log storage mode, log server URL.
    /// Loaded at startup via IAppConfigRepository.
    /// </summary>
    public class AppConfiguration
    {
        // ── v2.0 settings ──────────────────────────────────────────

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

        // ── v3.0 settings ──────────────────────────────────────────

        /// <summary>
        /// NEW v3.0 — Extensions with scheduling priority (include leading dot).
        /// Non-priority files wait while any priority file is being processed across any job.
        /// Example: [".xlsx", ".docx"]
        /// </summary>
        public List<string> PriorityExtensions { get; set; } = new();

        /// <summary>
        /// NEW v3.0 — Maximum file size (in KB) that can be transferred simultaneously across jobs.
        /// Files larger than this threshold use a mutual-exclusion lock.
        /// 0 = feature disabled (no large-file restriction).
        /// </summary>
        public long MaxParallelFileSizeKb { get; set; } = 0;

        /// <summary>
        /// NEW v3.0 — Where to write daily log files.
        /// "Local"  = local machine only.
        /// "Remote" = Docker log server only.
        /// "Both"   = local + Docker log server.
        /// </summary>
        public string LogStorageMode { get; set; } = "Local";

        /// <summary>
        /// NEW v3.0 — Base URL of the Docker centralised log server.
        /// Example: "http://logserver:5000"
        /// Empty = remote logging disabled even if LogStorageMode is "Remote" or "Both".
        /// </summary>
        public string LogServerUrl { get; set; } = string.Empty;

        // ── Helpers ────────────────────────────────────────────────

        /// <summary>
        /// Returns true if the given extension is in the encrypted list.
        /// Case-insensitive. Accepts ".pdf" or "pdf".
        /// </summary>
        public bool IsExtensionEncrypted(string ext)
        {
            if (string.IsNullOrWhiteSpace(ext)) return false;
            string normalized = ext.StartsWith('.') ? ext.ToLowerInvariant() : $".{ext.ToLowerInvariant()}";
            return EncryptedExtensions.Any(e => e.ToLowerInvariant() == normalized);
        }

        /// <summary>
        /// Returns true if the given extension has transfer priority.
        /// Case-insensitive. Accepts ".xlsx" or "xlsx".
        /// </summary>
        public bool IsExtensionPriority(string ext)
        {
            if (string.IsNullOrWhiteSpace(ext)) return false;
            string normalized = ext.StartsWith('.') ? ext.ToLowerInvariant() : $".{ext.ToLowerInvariant()}";
            return PriorityExtensions.Any(e => e.ToLowerInvariant() == normalized);
        }

        /// <summary>Returns true when BusinessSoftwareName is set.</summary>
        public bool IsBusinessSoftwareDetectionEnabled()
            => !string.IsNullOrWhiteSpace(BusinessSoftwareName);

        /// <summary>Returns true when remote logging should be used.</summary>
        public bool IsRemoteLoggingEnabled()
            => (LogStorageMode == "Remote" || LogStorageMode == "Both")
               && !string.IsNullOrWhiteSpace(LogServerUrl);

        /// <summary>Returns true when local logging should be used.</summary>
        public bool IsLocalLoggingEnabled()
            => LogStorageMode != "Remote";
    }
}
