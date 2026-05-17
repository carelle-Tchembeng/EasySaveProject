// EasySave.Infrastructure/Encryption/CryptoSoftAdapter.cs
// NEW v2.0 — implements IEncryptionService by launching CryptoSoft.exe

using EasySave.Core.Entities;
using EasySave.Core.Interfaces;
using System.Diagnostics;

namespace EasySave.Infrastructure.Encryption
{
    /// <summary>
    /// Encrypts files by launching CryptoSoft.exe as a child process.
    /// CryptoSoftPath is read from AppConfiguration.
    /// Returns encryption duration in ms, or negative value on error.
    /// </summary>
    public class CryptoSoftAdapter : IEncryptionService
    {
        private readonly AppConfiguration _config;

        public CryptoSoftAdapter(AppConfiguration config)
            => _config = config ?? throw new ArgumentNullException(nameof(config));

        /// <summary>
        /// Returns true if the CryptoSoft executable exists at the configured path.
        /// </summary>
        public bool IsAvailable()
            => !string.IsNullOrWhiteSpace(_config.CryptoSoftPath)
               && File.Exists(_config.CryptoSoftPath);

        /// <summary>
        /// Launches CryptoSoft.exe with the file path as argument.
        /// Waits for the process to complete and returns the elapsed time in ms.
        /// Returns -1 if CryptoSoft is unavailable, throws, or exits with a non-zero code.
        /// </summary>
        public long Encrypt(string filePath)
        {
            if (!IsAvailable()) return -1;
            return MeasureEncryption(filePath);
        }

        private long MeasureEncryption(string filePath)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = _config.CryptoSoftPath,
                    Arguments = $"\"{filePath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                var sw = Stopwatch.StartNew();

                using var process = Process.Start(startInfo);
                if (process == null) return -1;

                process.WaitForExit();
                sw.Stop();

                // Non-zero exit code means encryption failed
                return process.ExitCode == 0 ? sw.ElapsedMilliseconds : -process.ExitCode;
            }
            catch { return -1; }
        }
    }
}
