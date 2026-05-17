// EasySave.Infrastructure/Detection/BusinessSoftwareDetector.cs
// NEW v2.0 — implements IBusinessSoftwareDetector via Process API

using EasySave.Core.Interfaces;
using System.Diagnostics;

namespace EasySave.Infrastructure.Detection
{
    /// <summary>
    /// Detects running processes by name using System.Diagnostics.Process.
    /// Used by BackupService to block or interrupt backups when business software is detected.
    /// </summary>
    public class BusinessSoftwareDetector : IBusinessSoftwareDetector
    {
        /// <summary>
        /// Returns true if a process with the given name is currently running.
        /// </summary>
        /// <param name="processName">Process name without extension. Example: "calc"</param>
        public bool IsRunning(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName)) return false;
            return Process.GetProcessesByName(processName).Length > 0;
        }

        /// <summary>
        /// Waits for the process to terminate, polling every 500ms.
        /// Returns true if the process stopped within the timeout.
        /// Returns false if it is still running after timeoutMs.
        /// </summary>
        public bool WaitForTermination(string processName, int timeoutMs)
        {
            if (string.IsNullOrWhiteSpace(processName)) return true;

            int elapsed = 0;
            const int pollIntervalMs = 500;

            while (elapsed < timeoutMs)
            {
                if (!IsRunning(processName)) return true;
                Thread.Sleep(pollIntervalMs);
                elapsed += pollIntervalMs;
            }

            return !IsRunning(processName);
        }

        private Process[] GetProcesses(string name) => Process.GetProcessesByName(name);
    }
}
