// EasySave.Core/Interfaces/IFileSystem.cs
// UNCHANGED from v1.1

namespace EasySave.Core.Interfaces
{
    /// <summary>File system operations abstraction. Unchanged from v1.1.</summary>
    public interface IFileSystem
    {
        long CopyFile(string sourcePath, string destPath);
        void CreateDirectory(string path);
        List<string> GetFiles(string directoryPath);
        List<string> GetDirectories(string directoryPath);
        bool Exists(string path);
        DateTime GetLastWriteTime(string filePath);
        long GetFileSize(string filePath);
        string ToUncPath(string path);
    }
}
