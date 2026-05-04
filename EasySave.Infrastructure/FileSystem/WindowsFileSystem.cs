// EasySave.Infrastructure/FileSystem/WindowsFileSystem.cs

using EasySave.Core.Interfaces;
using EasySave.Infrastructure.Helpers;
using System.Diagnostics;

namespace EasySave.Infrastructure.FileSystem
{
    /// <summary>
    /// Concrete implementation of IFileSystem using System.IO APIs.
    /// Adapter between the Core layer and the Windows file system.
    /// Handles local drives, external drives, and UNC network paths.
    /// All copy operations are timed using Stopwatch for log accuracy.
    /// </summary>
    public class WindowsFileSystem : IFileSystem
    {
        // ─────────────────────────────────────────────────────────────
        // IFileSystem implementation
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Copies a file from source to destination and measures transfer time.
        /// Creates the destination directory if it does not exist.
        /// Overwrites the destination file if it already exists.
        /// </summary>
        /// <param name="sourcePath">Full path of the source file.</param>
        /// <param name="destPath">Full path of the destination file.</param>
        /// <returns>
        /// Transfer time in milliseconds.
        /// Returns -1 if the copy failed due to an exception.
        /// </returns>
        public long CopyFile(string sourcePath, string destPath)
        {
            try
            {
                // Ensure the destination directory exists before copying
                PathHelper.EnsureParentDirectory(destPath);

                var stopwatch = Stopwatch.StartNew();
                File.Copy(sourcePath, destPath, overwrite: true);
                stopwatch.Stop();

                return stopwatch.ElapsedMilliseconds;
            }
            catch (Exception)
            {
                // Return negative value to signal error as per specification
                return -1;
            }
        }

        /// <summary>
        /// Creates a directory and all intermediate directories.
        /// Does nothing if the directory already exists.
        /// </summary>
        /// <param name="path">Full path of the directory to create.</param>
        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Returns all file paths in the specified directory, recursively.
        /// Traverses all subdirectories.
        /// Returns an empty list if the directory does not exist or is empty.
        /// </summary>
        /// <param name="directoryPath">Full path of the directory to scan.</param>
        /// <returns>List of full file paths. Never null.</returns>
        public List<string> GetFiles(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                return new List<string>();

            return Directory
                .GetFiles(directoryPath, "*", SearchOption.AllDirectories)
                .ToList();
        }

        /// <summary>
        /// Returns all subdirectory paths in the specified directory, recursively.
        /// Returns an empty list if the directory does not exist.
        /// </summary>
        /// <param name="directoryPath">Full path of the directory to scan.</param>
        /// <returns>List of full subdirectory paths. Never null.</returns>
        public List<string> GetDirectories(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                return new List<string>();

            return Directory
                .GetDirectories(directoryPath, "*", SearchOption.AllDirectories)
                .ToList();
        }

        /// <summary>
        /// Returns true if the specified path exists (file or directory).
        /// Works with local paths, UNC paths, and mapped drives.
        /// </summary>
        /// <param name="path">Full path to check.</param>
        public bool Exists(string path)
        {
            return File.Exists(path) || Directory.Exists(path);
        }

        /// <summary>
        /// Returns the last write time (UTC) of the specified file.
        /// Used by DifferentialBackupStrategy to compare against LastFullBackupDate.
        /// </summary>
        /// <param name="filePath">Full path of the file.</param>
        /// <returns>UTC DateTime of the last write.</returns>
        public DateTime GetLastWriteTime(string filePath)
        {
            return File.GetLastWriteTime(filePath);
        }

        /// <summary>
        /// Returns the size of the specified file in bytes.
        /// Returns 0 if the file does not exist.
        /// </summary>
        /// <param name="filePath">Full path of the file.</param>
        public long GetFileSize(string filePath)
        {
            if (!File.Exists(filePath))
                return 0;

            return new FileInfo(filePath).Length;
        }

        /// <summary>
        /// Converts a local drive path to its UNC equivalent.
        /// Delegates to PathHelper.ToUncPath() for consistent behavior.
        /// </summary>
        /// <param name="path">Local or UNC path to convert.</param>
        /// <returns>UNC representation of the path.</returns>
        public string ToUncPath(string path)
        {
            return PathHelper.ToUncPath(path);
        }
    }
}
