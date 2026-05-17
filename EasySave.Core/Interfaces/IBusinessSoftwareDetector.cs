// EasySave.Core/Interfaces/IBusinessSoftwareDetector.cs
// NEW v2.0 — abstraction over OS process detection

namespace EasySave.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for detecting whether a business software process is running.
    /// Implemented by BusinessSoftwareDetector in the Infrastructure layer.
    /// Used by BackupService to block or interrupt backup jobs.
    /// </summary>
    public interface IBusinessSoftwareDetector
    {
        /// <summary>
        /// Returns true if a process with the given name is currently running.
        /// Uses Process.GetProcessesByName() internally.
        /// </summary>
        /// <param name="processName">
        /// Process name without extension. Example: "calc" for Calculator.
        /// </param>
        bool IsRunning(string processName);

        /// <summary>
        /// Waits for the specified process to terminate, up to the given timeout.
        /// Returns true if the process terminated within the timeout.
        /// Returns false if the process is still running after the timeout.
        /// </summary>
        /// <param name="processName">Process name without extension.</param>
        /// <param name="timeoutMs">Maximum wait time in milliseconds.</param>
        bool WaitForTermination(string processName, int timeoutMs);
    }
}
