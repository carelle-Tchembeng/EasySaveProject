using EasySave.ConsoleApp.Controllers;
using EasySave.ConsoleApp.Localization;
using EasySave.ConsoleApp.Parsers;
using EasySave.ConsoleApp.Views;
using EasySave.Core.Services;

namespace EasySave.ConsoleApp
{
    /// <summary>
    /// Main application orchestrator.
    ///
    /// It decides whether EasySave runs in:
    /// - interactive console mode
    /// - command-line mode
    ///
    /// This class does not contain backup business logic.
    /// It only routes execution to the correct services/controllers.
    /// </summary>
    public class App
    {
        private readonly BackupService _backupService;
        private readonly JobManager _jobManager;
        private readonly IConsoleView _view;
        private readonly ILocalizer _localizer;
        private readonly MenuController _menuController;
        private readonly CliArgumentParser _parser;

        public App(
            BackupService backupService,
            JobManager jobManager,
            IConsoleView view,
            ILocalizer localizer,
            MenuController menuController,
            CliArgumentParser parser)
        {
            _backupService = backupService;
            _jobManager = jobManager;
            _view = view;
            _localizer = localizer;
            _menuController = menuController;
            _parser = parser;
        }

        /// <summary>
        /// Starts the application.
        ///
        /// If command-line arguments are provided, CLI mode is used.
        /// Otherwise, interactive menu mode is used.
        /// </summary>
        public void Run(string[] args)
        {
            if (args.Length == 0)
                RunInteractive();
            else
                RunCli(args);
        }

        /// <summary>
        /// Runs the interactive console menu.
        /// </summary>
        private void RunInteractive()
        {
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
                        _menuController.HandleSettings();
                        break;

                    case "8":
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

        /// <summary>
        /// Runs the command-line execution mode.
        ///
        /// Supported examples:
        /// EasySave.exe 1-3
        /// EasySave.exe 1;3
        /// </summary>
        private void RunCli(string[] args)
        {
            int maxJobs = _jobManager.Count;

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
