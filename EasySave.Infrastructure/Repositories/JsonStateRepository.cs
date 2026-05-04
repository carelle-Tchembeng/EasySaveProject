// EasySave.Infrastructure/Repositories/JsonStateRepository.cs

using EasySave.Core.Entities;
using EasySave.Core.Interfaces;
using EasySave.Infrastructure.Helpers;
using System.Text.Json;

namespace EasySave.Infrastructure.Repositories
{
    /// <summary>
    /// Writes real-time backup job state to a single JSON file (state.json).
    /// State is overwritten entirely after each file copy to reflect current progress.
    /// Uses atomic write (temp file + rename) to prevent corrupt state files
    /// in case of unexpected application termination.
    /// </summary>
    public class JsonStateRepository : IStateRepository
    {
        // ─────────────────────────────────────────────────────────────
        // Configuration
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Full path to the state.json file.
        /// Example: C:\ProgramData\EasySave\state.json
        /// </summary>
        private readonly string _stateFilePath;

        /// <summary>
        /// Full path to the temporary file used for atomic writes.
        /// Example: C:\ProgramData\EasySave\state.tmp
        /// </summary>
        private readonly string _tempFilePath;

        /// <summary>
        /// JSON serializer options — indented for Notepad readability.
        /// </summary>
        private readonly JsonSerializerOptions _serializerOptions;

        // ─────────────────────────────────────────────────────────────
        // Constructor
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Initializes the repository with the target state file path.
        /// </summary>
        /// <param name="stateFilePath">
        /// Full absolute path to the state.json file.
        /// Provided by AppSettings to avoid hardcoded paths.
        /// </param>
        public JsonStateRepository(string stateFilePath)
        {
            _stateFilePath  = stateFilePath;
            _tempFilePath   = Path.ChangeExtension(stateFilePath, ".tmp");
            _serializerOptions = new JsonSerializerOptions { WriteIndented = true };
        }

        // ─────────────────────────────────────────────────────────────
        // IStateRepository implementation
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Overwrites state.json with the current state of all backup jobs.
        /// Uses atomic write to prevent partial/corrupt reads by monitoring tools.
        /// </summary>
        /// <param name="jobs">Complete list of all backup jobs and their current state.</param>
        public void Update(List<BackupJob> jobs)
        {
            PathHelper.EnsureParentDirectory(_stateFilePath);

            // Map each job to a flat StateEntry for serialization
            var entries = jobs.Select(MapToStateEntry).ToList();
            string json = JsonSerializer.Serialize(entries, _serializerOptions);

            // Atomic write: write to temp file first, then rename
            // This guarantees state.json is never in a partially-written state
            WriteAtomic(json);
        }

        /// <summary>
        /// Resets all job states to Inactive in state.json.
        /// Called on startup to clear stale Active states from a previous crash.
        /// </summary>
        /// <param name="jobs">Complete list of all backup jobs.</param>
        public void Clear(List<BackupJob> jobs)
        {
            // Reset all jobs to Inactive before writing
            foreach (var job in jobs)
            {
                job.ResetState();
            }

            Update(jobs);
        }

        // ─────────────────────────────────────────────────────────────
        // Private helpers
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Maps a BackupJob entity to a flat StateEntry DTO for JSON serialization.
        /// Handles both active and inactive jobs.
        /// </summary>
        /// <param name="job">The backup job to map.</param>
        /// <returns>StateEntry representing the current state of the job.</returns>
        private static StateEntry MapToStateEntry(BackupJob job)
        {
            var entry = new StateEntry
            {
                JobName        = job.Name,
                LastActionTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                Status         = job.Status.ToString()
            };

            // Populate progress fields only when the job is active
            if (job.Progress != null)
            {
                entry.TotalFiles         = job.Progress.TotalFiles;
                entry.TotalSizeBytes     = job.Progress.TotalSizeBytes;
                entry.RemainingFiles     = job.Progress.RemainingFiles;
                entry.RemainingBytes     = job.Progress.RemainingBytes;
                entry.ProgressPercent    = job.Progress.ProgressPercent;
                entry.CurrentSourceFile  = job.Progress.CurrentSourceFile;
                entry.CurrentDestFile    = job.Progress.CurrentDestFile;
            }

            return entry;
        }

        /// <summary>
        /// Writes JSON content atomically:
        /// 1. Writes to a temporary .tmp file
        /// 2. Replaces the target file with the temp file using File.Move
        /// This ensures state.json is never observed in a half-written state.
        /// </summary>
        /// <param name="json">The JSON content to write.</param>
        private void WriteAtomic(string json)
        {
            // Step 1: Write to temp file
            File.WriteAllText(_tempFilePath, json, System.Text.Encoding.UTF8);

            // Step 2: Atomically replace the state file with the temp file
            // overwrite: true replaces the destination if it already exists
            File.Move(_tempFilePath, _stateFilePath, overwrite: true);
        }
    }
}
