// EasySave.ConsoleApp/Views/ConsoleView.cs

using EasySave.ConsoleApp.Localization;
using EasySave.Core.Entities;
using EasySave.Core.Enums;
using EasySave.Core.ValueObjects;

namespace EasySave.ConsoleApp.Views
{
    /// <summary>
    /// Console implementation of IConsoleView.
    /// All Console.WriteLine and Console.ReadLine calls are contained here.
    /// Uses ILocalizer for all user-facing strings (FR/EN support).
    /// No business logic — only display and input collection.
    /// </summary>
    public class ConsoleView : IConsoleView
    {
        // ─────────────────────────────────────────────────────────────
        // Dependencies
        // ─────────────────────────────────────────────────────────────

        private readonly ILocalizer _localizer;

        // ─────────────────────────────────────────────────────────────
        // Constructor
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Initializes ConsoleView with the localizer for translated strings.
        /// </summary>
        public ConsoleView(ILocalizer localizer)
        {
            _localizer = localizer;
        }

        // ─────────────────────────────────────────────────────────────
        // Menu and navigation
        // ─────────────────────────────────────────────────────────────

        /// <inheritdoc/>
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
            Console.WriteLine(_localizer.Get("menu.quit"));
            Console.WriteLine();
            Console.Write(_localizer.Get("menu.choice") + " ");
        }

        // ─────────────────────────────────────────────────────────────
        // Job list
        // ─────────────────────────────────────────────────────────────

        /// <inheritdoc/>
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
                    Console.WriteLine($"  {job.Id,2} | {job.Name,-16} | {job.Type,-13} | {job.SourcePath} → {job.TargetPath}");
                }
            }

            Console.WriteLine(new string('-', 60));
        }

        // ─────────────────────────────────────────────────────────────
        // Status messages
        // ─────────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public void ShowSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n✔ {message}");
            Console.ResetColor();
        }

        /// <inheritdoc/>
        public void ShowError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n✘ {message}");
            Console.ResetColor();
        }

        // ─────────────────────────────────────────────────────────────
        // Progress display
        // ─────────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public void ShowProgress(string jobName, BackupProgress progress)
        {
            // Overwrite current line for a dynamic progress effect
            ClearCurrentLine();

            string line = string.Format(
                _localizer.Get("exec.progress"),
                progress.ProgressPercent,
                progress.TotalFiles - progress.RemainingFiles,
                progress.TotalFiles,
                progress.RemainingFiles);

            Console.Write(line);
        }

        // ─────────────────────────────────────────────────────────────
        // Input prompts
        // ─────────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public BackupJobFormData PromptJobForm()
        {
            Console.WriteLine();
            return new BackupJobFormData
            {
                Name       = ReadNonEmpty(_localizer.Get("job.form.name")),
                SourcePath = ReadNonEmpty(_localizer.Get("job.form.source")),
                TargetPath = ReadNonEmpty(_localizer.Get("job.form.target")),
                Type       = ReadBackupType()
            };
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public bool PromptConfirmDelete(string jobName)
        {
            Console.Write($"\n{string.Format(_localizer.Get("job.confirm.delete"), jobName)} ");
            string? input = Console.ReadLine()?.Trim().ToLower();

            // Accept "o" (French oui) and "y" (English yes)
            return input == "o" || input == "y";
        }

        /// <inheritdoc/>
        public string AskLanguage()
        {
            Console.WriteLine(_localizer.Get("app.language.prompt"));
            Console.Write("> ");

            string? input = Console.ReadLine()?.Trim().ToLower();

            // Accept "fr" or "en" — default to "en" for anything else
            return input == "fr" ? "fr" : "en";
        }

        /// <inheritdoc/>
        public void WaitForEnter()
        {
            Console.WriteLine();
            Console.WriteLine(_localizer.Get("prompt.press.enter"));
            Console.ReadLine();
        }

        // ─────────────────────────────────────────────────────────────
        // Private helpers
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Prompts the user with the given label and reads a non-empty string.
        /// Re-prompts if the user submits an empty value.
        /// </summary>
        /// <param name="prompt">Label to display before the input field.</param>
        /// <returns>Non-empty, trimmed string entered by the user.</returns>
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
        /// Prompts the user to select a backup type by entering 1 or 2.
        /// Re-prompts if the input is invalid.
        /// </summary>
        /// <returns>The selected BackupType (Full or Differential).</returns>
        private BackupType ReadBackupType()
        {
            while (true)
            {
                Console.Write($"{_localizer.Get("job.form.type")} ");
                string? input = Console.ReadLine()?.Trim();

                if (input == "1") return BackupType.Full;
                if (input == "2") return BackupType.Differential;

                ShowError(_localizer.Get("job.form.type.invalid"));
            }
        }

        /// <summary>
        /// Clears the current console line by overwriting it with spaces.
        /// Used to update progress display in place.
        /// </summary>
        private static void ClearCurrentLine()
        {
            Console.Write($"\r{new string(' ', Console.WindowWidth - 1)}\r");
        }
    }
}
