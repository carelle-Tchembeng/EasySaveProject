using EasySave.Core.Entities;
using EasySave.Core.ValueObjects;

namespace EasySave.ConsoleApp.Views
{
    /// <summary>
    /// Abstraction of the console user interface.
    ///
    /// This interface keeps controllers independent from Console.WriteLine
    /// and Console.ReadLine calls.
    ///
    /// This separation will make the transition to WPF/MVVM easier in v2.0,
    /// because business logic does not depend directly on console operations.
    /// </summary>
    public interface IConsoleView
    {
        /// <summary>
        /// Displays the main menu.
        /// </summary>
        void ShowMainMenu();

        /// <summary>
        /// Displays all configured backup jobs.
        /// </summary>
        void ShowJobList(List<BackupJob> jobs);

        /// <summary>
        /// Displays a success message.
        /// </summary>
        void ShowSuccess(string message);

        /// <summary>
        /// Displays an error message.
        /// </summary>
        void ShowError(string message);

        /// <summary>
        /// Displays real-time backup progress.
        /// </summary>
        void ShowProgress(string jobName, BackupProgress progress);

        /// <summary>
        /// Prompts the user for backup job creation or edition.
        /// </summary>
        BackupJobFormData PromptJobForm();

        /// <summary>
        /// Prompts the user for a valid job index.
        /// </summary>
        int PromptJobIndex(int max);

        /// <summary>
        /// Asks for confirmation before deleting a job.
        /// </summary>
        bool PromptConfirmDelete(string jobName);

        /// <summary>
        /// Asks the user to choose the interface language.
        /// </summary>
        string AskLanguage();

        /// <summary>
        /// Prompts the user to choose the log format.
        /// </summary>
        string PromptLogFormat(string currentFormat);

        /// <summary>
        /// Waits for the user to press Enter.
        /// </summary>
        void WaitForEnter();
    }
}
