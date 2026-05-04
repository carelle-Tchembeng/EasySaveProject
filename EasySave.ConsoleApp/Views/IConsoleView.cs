// EasySave.ConsoleApp/Views/IConsoleView.cs

using EasySave.Core.Entities;
using EasySave.Core.ValueObjects;

namespace EasySave.ConsoleApp.Views
{
    /// <summary>
    /// Defines all display and input operations for the console interface.
    /// Isolating the UI behind this interface allows future replacement
    /// with a graphical interface (WPF/MVVM in v2.0) without touching
    /// the Core or Infrastructure layers.
    /// </summary>
    public interface IConsoleView
    {
        /// <summary>Displays the main navigation menu.</summary>
        void ShowMainMenu();

        /// <summary>
        /// Displays the list of configured backup jobs.
        /// Shows a message if the list is empty.
        /// </summary>
        /// <param name="jobs">List of backup jobs to display.</param>
        void ShowJobList(List<BackupJob> jobs);

        /// <summary>Displays a success message in green.</summary>
        /// <param name="message">Message to display.</param>
        void ShowSuccess(string message);

        /// <summary>Displays an error message in red.</summary>
        /// <param name="message">Error message to display.</param>
        void ShowError(string message);

        /// <summary>
        /// Displays the real-time progress of a running backup job.
        /// Overwrites the current console line for a dynamic effect.
        /// </summary>
        /// <param name="jobName">Name of the job currently running.</param>
        /// <param name="progress">Current progress data to display.</param>
        void ShowProgress(string jobName, BackupProgress progress);

        /// <summary>
        /// Prompts the user to fill in all fields for a backup job.
        /// Used for both creation and editing.
        /// </summary>
        /// <returns>A BackupJobFormData populated with user input.</returns>
        BackupJobFormData PromptJobForm();

        /// <summary>
        /// Prompts the user to enter a job index number.
        /// Validates that the entered value is between 1 and max.
        /// </summary>
        /// <param name="max">Maximum valid index (number of configured jobs).</param>
        /// <returns>Valid 1-based job index entered by the user.</returns>
        int PromptJobIndex(int max);

        /// <summary>
        /// Prompts the user to confirm deletion of a named job.
        /// </summary>
        /// <param name="jobName">Name of the job to delete.</param>
        /// <returns>True if the user confirmed deletion.</returns>
        bool PromptConfirmDelete(string jobName);

        /// <summary>
        /// Prompts the user to select a language (fr/en).
        /// Displayed at startup before the main menu.
        /// </summary>
        /// <returns>Language code entered by the user: "fr" or "en".</returns>
        string AskLanguage();

        /// <summary>
        /// Displays a "Press Enter to continue..." prompt and waits.
        /// Used after displaying results to pause before showing the menu again.
        /// </summary>
        void WaitForEnter();
    }
}
