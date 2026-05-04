// EasySave.ConsoleApp/App.cs

using EasySave.ConsoleApp.Controllers;
using EasySave.ConsoleApp.Localization;
using EasySave.ConsoleApp.Parsers;
using EasySave.ConsoleApp.Views;
using EasySave.Core.Services;

namespace EasySave.ConsoleApp
{
    /// <summary>
    /// Main application orchestrator.
    /// Determines whether to run in interactive or CLI mode,
    /// then dispatches to the appropriate execution path.
    /// Contains no display logic and no business logic —
    /// delegates entirely to MenuController, BackupService, and IConsoleView.
    /// </summary>
    public class App
    {
        // ─────────────────────────────────────────────────────────────
        // Dependencies
        // ─────────────────────────────────────────────────────────────

        private readonly BackupService     _backupService;
        private readonly JobManager        _jobManager;
        private readonly IConsoleView      _view;
        private readonly ILocalizer        _localizer;
        private readonly MenuController    _menuController;
        private readonly CliArgumentParser _parser;

        // ─────────────────────────────────────────────────────────────
        // Constructor
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Initializes the application with all required dependencies.
        /// All parameters are injected from ServiceContainer.Build().
        /// </summary>
        public App(
            BackupService     backupService,
            JobManager        jobManager,
            IConsoleView      view,
            ILocalizer        localizer,
            MenuController    menuController,
            CliArgumentParser parser)
        {
            _backupService  = backupService;
            _jobManager     = jobManager;
            _view           = view;
            _localizer      = localizer;
            _menuController = menuController;
            _parser         = parser;
        }

        // ─────────────────────────────────────────────────────────────
        // Entry point
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Runs the application.
        /// Detects the execution mode (interactive vs CLI) and dispatches accordingly.
        /// </summary>
        /// <param name="args">Raw CLI arguments from Program.Main().</param>
        public void Run(string[] args)
        {
            if (args.Length == 0)
                RunInteractive();
            else
                RunCli(args);
        }

        // ─────────────────────────────────────────────────────────────
        // Interactive mode
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Runs the application in interactive console mode.
        /// 1. Asks the user to select a language
        /// 2. Displays the main menu in a loop
        /// 3. Handles the selected option
        /// 4. Exits when the user selects "Quit"
        /// </summary>
        private void RunInteractive()
        {
            // Language selection at startup
            string lang = _view.AskLanguage();
            _localizer.SetLanguage(lang);

            bool running = true;

            while (running)
            {
                _view.ShowMainMenu();
                string? input = Console.ReadLine()?.Trim();

                switch (input)
                {
                    case "1":
                        _menuController.HandleListJobs();
                        break;
                    case "2":
                        _menuController.HandleCreateJob();
                        break;
                    case "3":
                        _menuController.HandleEditJob();
                        break;
                    case "4":
                        _menuController.HandleDeleteJob();
                        break;
                    case "5":
                        _menuController.HandleExecuteOne();
                        break;
                    case "6":
                        _menuController.HandleExecuteAll();
                        break;
                    case "7":
                        running = false;
                        Console.WriteLine(_localizer.Get("app.exit"));
                        break;
                    default:
                        _view.ShowError(_localizer.Get("menu.invalid"));
                        _view.WaitForEnter();
                        break;
                }
            }
        }

        // ─────────────────────────────────────────────────────────────
        // CLI mode
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Runs the application in CLI mode.
        /// Parses the arguments, resolves the job indices, and executes them.
        /// No menu is shown — the program runs silently and exits.
        /// </summary>
        /// <param name="args">Raw CLI arguments (e.g. ["1-3"] or ["1;3"]).</param>
        private void RunCli(string[] args)
        {
            int maxJobs       = _jobManager.Count;
            List<int> indices = _parser.Parse(args, maxJobs);

            if (indices.Count == 0)
            {
                _view.ShowError(_localizer.Get("error.index.invalid"));
                return;
            }

            _backupService.ExecuteList(indices);
        }
    }
}
