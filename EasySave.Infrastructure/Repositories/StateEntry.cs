// EasySave.Infrastructure/Repositories/StateEntry.cs

using System.Text.Json.Serialization;

namespace EasySave.Infrastructure.Repositories
{
    /// <summary>
    /// Internal DTO representing the serialized state of a single backup job in state.json.
    /// Maps directly to the JSON structure required by the EasySave specification.
    /// All fields listed in the specification are represented here.
    /// Only used internally by JsonStateRepository — not exposed to the Core layer.
    /// </summary>
    internal class StateEntry
    {
        /// <summary>Name of the backup job.</summary>
        [JsonPropertyName("jobName")]
        public string JobName { get; set; } = string.Empty;

        /// <summary>Timestamp of the last state update for this job.</summary>
        [JsonPropertyName("lastActionTime")]
        public string LastActionTime { get; set; } = string.Empty;

        /// <summary>
        /// Current status of the job as a string.
        /// Values: "Inactive", "Active", "Completed", "Error"
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        // ─── Fields populated only when Status == "Active" ───────────

        /// <summary>Total number of files eligible for backup. 0 if inactive.</summary>
        [JsonPropertyName("totalFiles")]
        public int TotalFiles { get; set; }

        /// <summary>Total size in bytes of all eligible files. 0 if inactive.</summary>
        [JsonPropertyName("totalSizeBytes")]
        public long TotalSizeBytes { get; set; }

        /// <summary>Number of files not yet copied. 0 if inactive or completed.</summary>
        [JsonPropertyName("remainingFiles")]
        public int RemainingFiles { get; set; }

        /// <summary>Total size in bytes of files not yet copied. 0 if inactive or completed.</summary>
        [JsonPropertyName("remainingBytes")]
        public long RemainingBytes { get; set; }

        /// <summary>Completion percentage from 0 to 100.</summary>
        [JsonPropertyName("progressPercent")]
        public int ProgressPercent { get; set; }

        /// <summary>Full UNC path of the source file currently being copied. Empty if inactive.</summary>
        [JsonPropertyName("currentSourceFile")]
        public string CurrentSourceFile { get; set; } = string.Empty;

        /// <summary>Full UNC path of the destination file currently being copied. Empty if inactive.</summary>
        [JsonPropertyName("currentDestFile")]
        public string CurrentDestFile { get; set; } = string.Empty;
    }
}
