// EasySave.ConsoleApp/Program.cs

using EasySave.ConsoleApp.Controllers;
using EasySave.ConsoleApp.DI;
using EasySave.ConsoleApp.Localization;
using EasySave.ConsoleApp.Parsers;
using EasySave.ConsoleApp.Views;
using EasySave.Core.Interfaces;
using EasySave.Core.Services;
using EasySave.Infrastructure.Configuration;
using EasySave.Infrastructure.FileSystem;
using EasySave.Infrastructure.Logging;
using EasySave.Infrastructure.Repositories;
using EasySave.Infrastructure.Strategies;
using EasyLog;
using EasyLog.Factory;

namespace EasySave.ConsoleApp
{
    /// <summary>
    /// Application entry point.
    /// Responsibilities:
    ///   1. Load application settings
    ///   2. Wire all dependencies into the ServiceContainer
    ///   3. Create and run the App
    ///
    /// This is the ONLY class in the solution that knows about all layers.
    /// All other classes receive their dependencies via constructor injection.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args">
        /// CLI arguments passed by the OS.
        /// Empty → interactive mode.
        /// "1-3" or "1;3" → CLI mode.
        /// </param>
        static void Main(string[] args)
        {
            try
            {
                // ── Step 1: Load application settings ────────────────
                string settingsPath = Path.Combine(
                    AppContext.BaseDirectory, "appsettings.json");

                AppSettings settings = AppSettings.Load(settingsPath);

                // ── Step 2: Build service container ──────────────────
                ServiceContainer container = BuildContainer(settings);

                // ── Step 3: Run the application ───────────────────────
                App app = container.Resolve<App>();
                app.Run(args);
            }
            catch (Exception ex)
            {
                // Last-resort error handler — should never be reached
                // in normal operation if all services are correctly wired
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[FATAL] Unhandled exception: {ex.Message}");
                Console.ResetColor();
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Wires all application dependencies and returns a populated ServiceContainer.
        /// Dependencies are registered in order: no service is registered before
        /// the services it depends on.
        /// </summary>
        /// <param name="settings">Loaded application settings.</param>
        /// <returns>Fully populated ServiceContainer ready for resolution.</returns>
        private static ServiceContainer BuildContainer(AppSettings settings)
        {
            var container = new ServiceContainer();

            // ── Infrastructure: EasyLog DLL ───────────────────────────
            IEasyLogWriter logWriter = LogWriterFactory.Create(settings.LogDirectory);

            // ── Infrastructure: Repositories ─────────────────────────
            var configRepository = new JsonConfigRepository(settings.ConfigFilePath);
            var stateRepository  = new JsonStateRepository(settings.StateFilePath);
            container.Register<IConfigRepository>(configRepository);
            container.Register<IStateRepository>(stateRepository);

            // ── Infrastructure: File system ───────────────────────────
            var fileSystem = new WindowsFileSystem();
            container.Register<IFileSystem>(fileSystem);

            // ── Infrastructure: Logger adapter ────────────────────────
            var logger = new EasyLogAdapter(logWriter);
            container.Register<ILogger>(logger);

            // ── Infrastructure: Backup strategies ────────────────────
            var fullStrategy         = new FullBackupStrategy();
            var differentialStrategy = new DifferentialBackupStrategy();
            container.Register<IBackupStrategy>(fullStrategy); // default

            // ── Core: Services ────────────────────────────────────────
            var jobManager = new JobManager(configRepository, fileSystem);
            container.Register<JobManager>(jobManager);

            var backupService = new BackupService(
                jobManager,
                fileSystem,
                stateRepository,
                logger,
                fullStrategy,
                differentialStrategy);
            container.Register<BackupService>(backupService);

            // ── ConsoleApp: Localization ──────────────────────────────
            var localizer = new ResourceLocalizer();
            container.Register<ILocalizer>(localizer);

            // ── ConsoleApp: View ──────────────────────────────────────
            var view = new ConsoleView(localizer);
            container.Register<IConsoleView>(view);

            // ── ConsoleApp: Parser ────────────────────────────────────
            var parser = new CliArgumentParser();
            container.Register<CliArgumentParser>(parser);

            // ── ConsoleApp: Controller ────────────────────────────────
            var menuController = new MenuController(jobManager, backupService, view, localizer);
            container.Register<MenuController>(menuController);

            // ── ConsoleApp: App ───────────────────────────────────────
            var app = new App(backupService, jobManager, view, localizer, menuController, parser);
            container.Register<App>(app);

            // Clear any stale Active states from a previous crash
            stateRepository.Clear(jobManager.GetAll());

            return container;
        }
    }
}
