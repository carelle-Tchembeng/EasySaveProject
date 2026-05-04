// EasySave.Infrastructure/Helpers/PathHelper.cs

namespace EasySave.Infrastructure.Helpers
{
    /// <summary>
    /// Static utility for path computation and normalization.
    /// Centralizes all path-related logic to avoid duplication across repositories.
    /// All paths returned by this class are absolute and safe for server environments.
    /// </summary>
    public static class PathHelper
    {
        // ─────────────────────────────────────────────────────────────
        // Base directory
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Base application data directory.
        /// Resolves to: C:\ProgramData\EasySave on standard Windows installations.
        /// This path is safe for server environments and does not require admin rights to read.
        /// </summary>
        public static string BaseDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "EasySave");

        // ─────────────────────────────────────────────────────────────
        // Standard application paths
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the default path for the configuration file.
        /// Example: C:\ProgramData\EasySave\config.json
        /// </summary>
        public static string GetConfigPath() =>
            Path.Combine(BaseDirectory, "config.json");

        /// <summary>
        /// Returns the default path for the real-time state file.
        /// Example: C:\ProgramData\EasySave\state.json
        /// </summary>
        public static string GetStatePath() =>
            Path.Combine(BaseDirectory, "state.json");

        /// <summary>
        /// Returns the default path for the log directory.
        /// Example: C:\ProgramData\EasySave\logs
        /// </summary>
        public static string GetLogDirectory() =>
            Path.Combine(BaseDirectory, "logs");

        // ─────────────────────────────────────────────────────────────
        // Path utilities
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Converts a local drive path to its UNC equivalent.
        /// UNC paths are required for log entries as per the specification.
        /// Examples:
        ///   C:\docs\file.txt     → \\localhost\C$\docs\file.txt
        ///   D:\backup\           → \\localhost\D$\backup\
        ///   \\server\share\file  → \\server\share\file (unchanged)
        /// </summary>
        /// <param name="path">Absolute local or UNC path to convert.</param>
        /// <returns>UNC representation of the path.</returns>
        public static string ToUncPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;

            // Already a UNC path — return as-is
            if (path.StartsWith(@"\\"))
                return path;

            // Local drive path (e.g. C:\docs\file.txt)
            if (path.Length >= 2 && path[1] == ':')
            {
                char driveLetter = char.ToUpper(path[0]);
                string remainingPath = path.Substring(2); // strip "C:"
                return $@"\\localhost\{driveLetter}$\{remainingPath.TrimStart('\\')}";
            }

            // Relative or unknown format — return unchanged
            return path;
        }

        /// <summary>
        /// Ensures the specified directory exists.
        /// Creates all intermediate directories if necessary.
        /// Does nothing if the directory already exists.
        /// </summary>
        /// <param name="directoryPath">Absolute path of the directory to ensure.</param>
        public static void EnsureDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        /// <summary>
        /// Ensures the parent directory of the specified file path exists.
        /// Useful before writing a file for the first time.
        /// </summary>
        /// <param name="filePath">Absolute path of the file whose parent directory to ensure.</param>
        public static void EnsureParentDirectory(string filePath)
        {
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                EnsureDirectory(directory);
            }
        }

        /// <summary>
        /// Computes the corresponding destination path for a source file,
        /// given the source root and target root directories.
        /// Preserves the relative sub-path within the source directory.
        /// Example:
        ///   sourceRoot = \\srv01\docs
        ///   targetRoot = \\srv02\backup
        ///   sourceFile = \\srv01\docs\reports\q3.pdf
        ///   → returns \\srv02\backup\reports\q3.pdf
        /// </summary>
        /// <param name="sourceRoot">Root source directory of the backup job.</param>
        /// <param name="targetRoot">Root target directory of the backup job.</param>
        /// <param name="sourceFile">Full path of the source file to map.</param>
        /// <returns>Full destination path preserving the relative structure.</returns>
        public static string MapToTargetPath(string sourceRoot, string targetRoot, string sourceFile)
        {
            // Get the relative path within the source directory
            string relativePath = Path.GetRelativePath(sourceRoot, sourceFile);

            // Combine with target root
            return Path.Combine(targetRoot, relativePath);
        }
    }
}
