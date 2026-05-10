using EasySave.ConsoleApp.Localization;
using EasySave.Core.Entities;
using EasySave.Core.Enums;
using EasySave.Core.ValueObjects;

namespace EasySave.ConsoleApp.Views
{
    /// <summary>
    /// Console implementation of IConsoleView.
    ///
    /// This class is the only place where direct console input/output
    /// should happen.
    ///
    /// It does not contain business logic.
    /// It only displays information and collects user input.
    /// </summary>
    public class ConsoleView : IConsoleView
    {
        private readonly ILocalizer _localizer;

        public ConsoleView(ILocalizer localizer)
        {
            _localizer = localizer;
        }

        /// <summary>
        /// Displays the main menu.
        /// </summary>
        public void ShowMainMenu()
        {
            Console.Clear();

            Console.WriteLine(_localizer.Get("app.title"));
            Console.WriteLine();

            Console.WriteLine(_localizer.Get("menu.title"));
            Console.WriteLine(_localizer.Get("menu.list"));
            Console.WriteLine(_localizer.Get("menu.create"));
            Console.WriteLine(_localizer.Get("menu.edit"));
            Console.WriteLine(_localizer.Get("menu.delete"));
            Console.WriteLine(_localizer.Get("menu.execute.one"));
            Console.WriteLine(_localizer.Get("menu.execute.all"));
            Console.WriteLine(_localizer.Get("menu.settings"));
            Console.WriteLine(_localizer.Get("menu.quit"));

            Console.WriteLine();
            Console.Write(_localizer.Get("menu.choice") + " ");
        }

        /// <summary>
        /// Displays the list of configured jobs.
        /// </summary>
        public void ShowJobList(List<BackupJob> jobs)
        {
            Console.WriteLine();
            Console.WriteLine(_localizer.Get("job.list.title"));
            Console.WriteLine(new string('-', 60));

            if (jobs.Count == 0)
            {
                Console.WriteLine(_localizer.Get("job.list.empty"));
            }
            else
            {
                Console.WriteLine(_localizer.Get("job.list.header"));
                Console.WriteLine(new string('-', 60));

                foreach (var job in jobs)
                {
                    Console.WriteLine(
                        $"  {job.Id,2} | {job.Name,-16} | {job.Type,-13} | {job.SourcePath} → {job.TargetPath}");
                }
            }

            Console.WriteLine(new string('-', 60));
        }

        /// <summary>
        /// Displays a success message in green.
        /// </summary>
        public void ShowSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n✔ {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Displays an error message in red.
        /// </summary>
        public void ShowError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n✘ {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// Displays backup progress on the current console line.
        /// </summary>
        public void ShowProgress(string jobName, BackupProgress progress)
        {
            ClearCurrentLine();

            string line = string.Format(
                _localizer.Get("exec.progress"),
                progress.ProgressPercent,
                progress.TotalFiles - progress.RemainingFiles,
                progress.TotalFiles,
                progress.RemainingFiles);

            Console.Write(line);
        }

        /// <summary>
        /// Prompts the user for all fields required to create or edit a job.
        /// </summary>
        public BackupJobFormData PromptJobForm()
        {
            Console.WriteLine();

            return new BackupJobFormData
            {
                Name = ReadNonEmpty(_localizer.Get("job.form.name")),
                SourcePath = ReadNonEmpty(_localizer.Get("job.form.source")),
                TargetPath = ReadNonEmpty(_localizer.Get("job.form.target")),
                Type = ReadBackupType()
            };
        }

        /// <summary>
        /// Prompts the user until a valid job index is entered.
        /// </summary>
        public int PromptJobIndex(int max)
        {
            while (true)
            {
                Console.Write($"\n{_localizer.Get("job.select.index")} ");
                string? input = Console.ReadLine();

                if (int.TryParse(input, out int index) && index >= 1 && index <= max)
                    return index;

                ShowError(_localizer.Get("error.index.invalid"));
            }
        }

        /// <summary>
        /// Asks for deletion confirmation.
        /// Accepts both French and English confirmations.
        /// </summary>
        public bool PromptConfirmDelete(string jobName)
        {
            Console.Write($"\n{string.Format(_localizer.Get("job.confirm.delete"), jobName)} ");
            string? input = Console.ReadLine()?.Trim().ToLower();

            return input == "o" || input == "y";
        }

        /// <summary>
        /// Asks the user to select the language.
        /// </summary>
        public string AskLanguage()
        {
            Console.WriteLine(_localizer.Get("app.language.prompt"));
            Console.Write("> ");

            string? input = Console.ReadLine()?.Trim().ToLower();

            return input == "fr" ? "fr" : "en";
        }

        /// <summary>
        /// Prompts the user to select the daily log file format.
        /// </summary>
        public string PromptLogFormat(string currentFormat)
        {
            while (true)
            {
                Console.WriteLine();

                Console.WriteLine(string.Format(
                    _localizer.Get("settings.current.log.format"),
                    currentFormat));

                Console.WriteLine(_localizer.Get("settings.log.format.prompt"));
                Console.Write("> ");

                string? input = Console.ReadLine()?.Trim();

                if (input == "1")
                    return "Json";

                if (input == "2")
                    return "Xml";

                ShowError(_localizer.Get("settings.log.format.invalid"));
            }
        }

        /// <summary>
        /// Waits for the user to press Enter.
        /// </summary>
        public void WaitForEnter()
        {
            Console.WriteLine();
            Console.WriteLine(_localizer.Get("prompt.press.enter"));
            Console.ReadLine();
        }

        /// <summary>
        /// Reads a non-empty string from the console.
        /// </summary>
        private string ReadNonEmpty(string prompt)
        {
            while (true)
            {
                Console.Write($"{prompt} ");
                string? value = Console.ReadLine()?.Trim();

                if (!string.IsNullOrEmpty(value))
                    return value;

                ShowError(_localizer.Get("error.empty.input"));
            }
        }

        /// <summary>
        /// Reads the backup type selected by the user.
        /// </summary>
        private BackupType ReadBackupType()
        {
            while (true)
            {
                Console.Write($"{_localizer.Get("job.form.type")} ");
                string? input = Console.ReadLine()?.Trim();

                if (input == "1")
                    return BackupType.Full;

                if (input == "2")
                    return BackupType.Differential;

                ShowError(_localizer.Get("job.form.type.invalid"));
            }
        }

        /// <summary>
        /// Clears the current console line.
        /// Used when displaying dynamic progress.
        /// </summary>
        private static void ClearCurrentLine()
        {
            Console.Write($"\r{new string(' ', Console.WindowWidth - 1)}\r");
        }
    }
}
