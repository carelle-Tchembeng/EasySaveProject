// EasySave.ConsoleApp/Views/BackupJobFormData.cs

using EasySave.Core.Enums;

namespace EasySave.ConsoleApp.Views
{
    /// <summary>
    /// Data Transfer Object carrying user input from the job creation/edit form.
    /// Produced by ConsoleView.PromptJobForm() and consumed by MenuController.
    /// Keeps ConsoleView and MenuController decoupled from each other.
    /// </summary>
    public class BackupJobFormData
    {
        /// <summary>User-provided job name.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>User-provided source directory path.</summary>
        public string SourcePath { get; set; } = string.Empty;

        /// <summary>User-provided target directory path.</summary>
        public string TargetPath { get; set; } = string.Empty;

        /// <summary>User-selected backup type: Full or Differential.</summary>
        public BackupType Type { get; set; } = BackupType.Full;
    }
}
