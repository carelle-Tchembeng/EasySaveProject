using EasySave.Core.Entities;

namespace EasySave.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for backup execution strategies.
    ///
    /// The strategy pattern allows EasySave to support several backup algorithms
    /// without modifying BackupService:
    /// - Full backup
    /// - Differential backup
    /// - Future strategies if required
    /// </summary>
    public interface IBackupStrategy
    {
        /// <summary>
        /// Returns the list of files that are eligible for this strategy.
        ///
        /// This is important for accurate progress calculation.
        /// For example:
        /// - Full backup: all source files are eligible.
        /// - Differential backup: only modified files are eligible.
        /// </summary>
        /// <param name="job">The backup job to evaluate.</param>
        /// <param name="fileSystem">File system abstraction.</param>
        /// <returns>List of full file paths eligible for backup.</returns>
        List<string> GetEligibleFiles(
            BackupJob job,
            IFileSystem fileSystem);

        /// <summary>
        /// Executes the backup strategy.
        ///
        /// The strategy copies eligible files from the source directory
        /// to the target directory.
        ///
        /// Logging and state updates are not directly handled here.
        /// Instead, the strategy invokes the callback after each processed file.
        /// </summary>
        /// <param name="job">The backup job to execute.</param>
        /// <param name="fileSystem">File system abstraction.</param>
        /// <param name="logger">Logger abstraction. Currently unused by strategies.</param>
        /// <param name="onFileCopied">
        /// Callback called after each processed file.
        /// Parameters are:
        /// sourceFile, destinationFile, fileSizeBytes, transferTimeMs.
        /// </param>
        void Execute(
            BackupJob job,
            IFileSystem fileSystem,
            ILogger logger,
            Action<string, string, long, long> onFileCopied);
    }
}
