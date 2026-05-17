// EasySave.Infrastructure/Encryption/CryptoSoftAdapter.cs
// UPDATED v3.0 — CryptoSoft is now MONO-INSTANCE (named system Mutex)

using EasySave.Core.Entities;
using EasySave.Core.Interfaces;
using System.Diagnostics;

namespace EasySave.Infrastructure.Encryption
{
    /// <summary>
    /// Encrypts files by launching CryptoSoft.exe as a child process.
    /// v3.0: CryptoSoft is mono-instance — a named system Mutex ensures only one
    /// instance runs at a time across the entire machine, even with parallel jobs.
    ///
    /// The Mutex name "Global\\CryptoSoft_EasySave" is shared between all BackupService
    /// threads so they serialise naturally around encryption.
    /// </summary>
    public class CryptoSoftAdapter : IEncryptionService
    {
        /// <summary>System-wide Mutex name — prevents concurrent CryptoSoft instances.</summary>
        private const string MutexName = @"Global\CryptoSoft_EasySave";

        private readonly AppConfiguration _config;

        public CryptoSoftAdapter(AppConfiguration config)
            => _config = config ?? throw new ArgumentNullException(nameof(config));

        /// <summary>Returns true if CryptoSoft.exe exists at the configured path.</summary>
        public bool IsAvailable()
            => !string.IsNullOrWhiteSpace(_config.CryptoSoftPath)
               && File.Exists(_config.CryptoSoftPath);

        /// <summary>
        /// Encrypts the file at the given path.
        /// Acquires a system-wide named Mutex before launching CryptoSoft to guarantee
        /// mono-instance behaviour even when multiple backup jobs run in parallel.
        /// Returns: elapsed ms on success, -1 on unavailability, negative error code on failure.
        /// </summary>
        public long Encrypt(string filePath)
        {
            if (!IsAvailable()) return -1;

            // Named Mutex — system-wide, blocks other threads/processes trying to use CryptoSoft
            using var mutex = new Mutex(initiallyOwned: false, MutexName);
            bool acquired = false;

            try
            {
                // Wait up to 30 seconds for the mutex; -1 = wait indefinitely
                acquired = mutex.WaitOne(TimeSpan.FromSeconds(30));
                if (!acquired) return -1; // timed out waiting for CryptoSoft

                return MeasureEncryption(filePath);
            }
            catch (AbandonedMutexException)
            {
                // Previous holder crashed — we now own it; safe to continue
                return MeasureEncryption(filePath);
            }
            finally
            {
                if (acquired) mutex.ReleaseMutex();
            }
        }

        private long MeasureEncryption(string filePath)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName               = _config.CryptoSoftPath,
                    Arguments              = $"\"{filePath}\"",
                    UseShellExecute        = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    CreateNoWindow         = true
                };

                var sw = Stopwatch.StartNew();

                using var process = Process.Start(startInfo);
                if (process == null) return -1;

                process.WaitForExit();
                sw.Stop();

                return process.ExitCode == 0 ? sw.ElapsedMilliseconds : -Math.Abs(process.ExitCode);
            }
            catch
            {
                return -1;
            }
        }
    }
}
