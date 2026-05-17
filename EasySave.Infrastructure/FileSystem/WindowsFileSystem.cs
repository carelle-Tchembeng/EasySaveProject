// EasySave.Infrastructure/FileSystem/WindowsFileSystem.cs — unchanged from v1.1
using EasySave.Core.Interfaces;
using EasySave.Infrastructure.Helpers;
using System.Diagnostics;

namespace EasySave.Infrastructure.FileSystem
{
    public class WindowsFileSystem : IFileSystem
    {
        public long CopyFile(string sourcePath, string destPath)
        {
            try
            {
                PathHelper.EnsureParentDirectory(destPath);
                var sw = Stopwatch.StartNew();
                File.Copy(sourcePath, destPath, overwrite: true);
                sw.Stop();
                return sw.ElapsedMilliseconds;
            }
            catch { return -1; }
        }

        public void CreateDirectory(string path) => Directory.CreateDirectory(path);

        public List<string> GetFiles(string directoryPath)
        {
            if (!Directory.Exists(directoryPath)) return new List<string>();
            return Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories).ToList();
        }

        public List<string> GetDirectories(string directoryPath)
        {
            if (!Directory.Exists(directoryPath)) return new List<string>();
            return Directory.GetDirectories(directoryPath, "*", SearchOption.AllDirectories).ToList();
        }

        public bool Exists(string path) => File.Exists(path) || Directory.Exists(path);
        public DateTime GetLastWriteTime(string filePath) => File.GetLastWriteTime(filePath);
        public long GetFileSize(string filePath) => File.Exists(filePath) ? new FileInfo(filePath).Length : 0;
        public string ToUncPath(string path) => PathHelper.ToUncPath(path);
    }
}
