// EasySave.Infrastructure/Repositories/StateEntry.cs — unchanged
using System.Text.Json.Serialization;
namespace EasySave.Infrastructure.Repositories
{
    internal class StateEntry
    {
        [JsonPropertyName("jobName")]        public string JobName           { get; set; } = string.Empty;
        [JsonPropertyName("lastActionTime")] public string LastActionTime    { get; set; } = string.Empty;
        [JsonPropertyName("status")]         public string Status            { get; set; } = string.Empty;
        [JsonPropertyName("totalFiles")]     public int    TotalFiles        { get; set; }
        [JsonPropertyName("totalSizeBytes")] public long   TotalSizeBytes    { get; set; }
        [JsonPropertyName("remainingFiles")] public int    RemainingFiles    { get; set; }
        [JsonPropertyName("remainingBytes")] public long   RemainingBytes    { get; set; }
        [JsonPropertyName("progressPercent")]public int    ProgressPercent   { get; set; }
        [JsonPropertyName("currentSourceFile")] public string CurrentSourceFile { get; set; } = string.Empty;
        [JsonPropertyName("currentDestFile")]   public string CurrentDestFile   { get; set; } = string.Empty;
    }
}
