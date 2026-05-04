// EasySave.Core/Interfaces/IFileSystem.cs

namespace EasySave.Core.Interfaces
{
    /// <summary>
    /// Abstraction over file system operations.
    /// Allows the Core to remain independent of System.IO.
    /// Implemented by WindowsFileSystem in the Infrastructure layer.
    /// </summary>
    public interface IFileSystem
    {
        /// <summary>
        /// Copies a file from source to destination.
        /// Creates the destination directory if it does not exist.
        /// </summary>
        /// <param name="sourcePath">Full path of the source file.</param>
        /// <param name="destPath">Full path of the destination file.</param>
        /// <returns>Transfer time in milliseconds. Negative if an error occurred.</returns>
        long CopyFile(string sourcePath, string destPath);

        /// <summary>
        /// Creates a directory and all intermediate directories if they do not exist.
        /// Does nothing if the directory already exists.
        /// </summary>
        /// <param name="path">Full path of the directory to create.</param>
        void CreateDirectory(string path);

        /// <summary>
        /// Returns all file paths in the specified directory, recursively.
        /// </summary>
        /// <param name="directoryPath">Full path of the directory to scan.</param>
        /// <returns>List of full file paths found in the directory tree.</returns>
        List<string> GetFiles(string directoryPath);

        /// <summary>
        /// Returns all subdirectory paths in the specified directory, recursively.
        /// </summary>
        /// <param name="directoryPath">Full path of the directory to scan.</param>
        /// <returns>List of full subdirectory paths found in the directory tree.</returns>
        List<string> GetDirectories(string directoryPath);

        /// <summary>
        /// Returns true if the specified path (file or directory) exists.
        /// </summary>
        /// <param name="path">Full path to check.</param>
        bool Exists(string path);

        /// <summary>
        /// Returns the last write time of the specified file.
        /// Used by DifferentialBackupStrategy to compare against LastFullBackupDate.
        /// </summary>
        /// <param name="filePath">Full path of the file.</param>
        DateTime GetLastWriteTime(string filePath);

        /// <summary>
        /// Returns the size of the specified file in bytes.
        /// </summary>
        /// <param name="filePath">Full path of the file.</param>
        long GetFileSize(string filePath);

        /// <summary>
        /// Converts a local or mapped drive path to its UNC equivalent.
        /// Example: C:\docs\file.txt → \\localhost\C$\docs\file.txt
        /// Returns UNC paths unchanged.
        /// </summary>
        /// <param name="path">Local or UNC path to convert.</param>
        string ToUncPath(string path);
    }
}
