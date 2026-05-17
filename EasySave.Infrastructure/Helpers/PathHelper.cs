// EasySave.Infrastructure/Helpers/PathHelper.cs
// UPDATED v2.0 — added GetAppConfigPath()

namespace EasySave.Infrastructure.Helpers
{
    public static class PathHelper
    {
        public static string BaseDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EasySave");

        public static string GetConfigPath()    => Path.Combine(BaseDirectory, "config.json");
        public static string GetStatePath()     => Path.Combine(BaseDirectory, "state.json");
        public static string GetLogDirectory()  => Path.Combine(BaseDirectory, "logs");

        /// <summary>Path to application configuration (v2.0 settings). NEW v2.0.</summary>
        public static string GetAppConfigPath() => Path.Combine(BaseDirectory, "appconfig.json");

        public static string ToUncPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path.StartsWith(@"\\")) return path;
            if (path.Length >= 2 && path[1] == ':')
            {
                char d = char.ToUpper(path[0]);
                return $@"\\localhost\{d}$\{path.Substring(2).TrimStart('\\')}";
            }
            return path;
        }

        public static void EnsureDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
        }

        public static void EnsureParentDirectory(string filePath)
        {
            string? dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir)) EnsureDirectory(dir);
        }

        public static string MapToTargetPath(string sourceRoot, string targetRoot, string sourceFile) =>
            Path.Combine(targetRoot, Path.GetRelativePath(sourceRoot, sourceFile));
    }
}
