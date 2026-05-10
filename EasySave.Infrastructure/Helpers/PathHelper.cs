namespace EasySave.Infrastructure.Helpers
{
    /// <summary>
    /// Centralizes path-related operations.
    ///
    /// This avoids duplicated path logic across repositories, strategies,
    /// file system adapters, and configuration services.
    /// </summary>
    public static class PathHelper
    {
        /// <summary>
        /// Base application directory.
        ///
        /// ProgramData is preferred here because:
        /// - it avoids forbidden temporary paths such as C:\temp
        /// - it is suitable for installed client software
        /// - it can be used by support teams to locate files consistently
        /// </summary>
        public static string BaseDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "EasySave");

        /// <summary>
        /// Returns the path of the global settings file.
        /// </summary>
        public static string GetSettingsPath()
        {
            return Path.Combine(BaseDirectory, "appsettings.json");
        }

        /// <summary>
        /// Returns the path of the backup jobs configuration file.
        /// </summary>
        public static string GetConfigPath()
        {
            return Path.Combine(BaseDirectory, "config.json");
        }

        /// <summary>
        /// Returns the path of the real-time state file.
        /// </summary>
        public static string GetStatePath()
        {
            return Path.Combine(BaseDirectory, "state.json");
        }

        /// <summary>
        /// Returns the directory where daily logs are stored.
        /// </summary>
        public static string GetLogDirectory()
        {
            return Path.Combine(BaseDirectory, "logs");
        }

        /// <summary>
        /// Converts local drive paths to UNC-like paths for logs.
        ///
        /// Examples:
        /// C:\Data\File.txt becomes \\localhost\C$\Data\File.txt
        /// \\server\share\File.txt remains unchanged.
        /// </summary>
        public static string ToUncPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;

            if (path.StartsWith(@"\\"))
                return path;

            if (path.Length >= 2 && path[1] == ':')
            {
                char driveLetter = char.ToUpper(path[0]);
                string remainingPath = path.Substring(2);

                return $@"\\localhost\{driveLetter}$\{remainingPath.TrimStart('\\')}";
            }

            return path;
        }

        /// <summary>
        /// Ensures that a directory exists.
        /// If it does not exist, it is created.
        /// </summary>
        public static void EnsureDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        /// <summary>
        /// Ensures that the parent directory of a file path exists.
        /// Useful before writing config, state, or log files.
        /// </summary>
        public static void EnsureParentDirectory(string filePath)
        {
            string? directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                EnsureDirectory(directory);
            }
        }

        /// <summary>
        /// Maps a source file path to its corresponding target path.
        ///
        /// Example:
        /// sourceRoot = C:\Source
        /// targetRoot = D:\Backup
        /// sourceFile = C:\Source\Folder\File.txt
        ///
        /// result = D:\Backup\Folder\File.txt
        /// </summary>
        public static string MapToTargetPath(
            string sourceRoot,
            string targetRoot,
            string sourceFile)
        {
            string relativePath = Path.GetRelativePath(sourceRoot, sourceFile);
            return Path.Combine(targetRoot, relativePath);
        }
    }
}
