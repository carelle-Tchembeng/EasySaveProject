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

namespace EasySave.ConsoleApp
{
    /// <summary>
    /// Application entry point.
    ///
    /// Program.cs is the composition root of the application.
    /// This means it is responsible for creating and wiring all dependencies.
    ///
    /// Other classes should receive dependencies through constructors
    /// and should not instantiate concrete infrastructure classes directly.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Main application entry point.
        /// </summary>
        static void Main(string[] args)
        {
            try
            {
                ServiceContainer container = BuildContainer();

                var app = container.Resolve<App>();

                app.Run(args);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[CRITICAL ERROR] Application failed to start: {ex.Message}");
                Console.ResetColor();

                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Builds the lightweight dependency injection container.
        /// </summary>
        private static ServiceContainer BuildContainer()
        {
            var container = new ServiceContainer();

            // ----------------------------------------------------------------
            // Configuration
            // ----------------------------------------------------------------

            var appSettings = AppSettings.LoadDefault();
            container.Register<AppSettings>(appSettings);

            // ----------------------------------------------------------------
            // Infrastructure services
            // ----------------------------------------------------------------

            var fileSystem = new WindowsFileSystem();
            container.Register<IFileSystem>(fileSystem);

            var configRepository = new JsonConfigRepository(appSettings.ConfigFilePath);
            container.Register<IConfigRepository>(configRepository);

            var stateRepository = new JsonStateRepository(appSettings.StateFilePath);
            container.Register<IStateRepository>(stateRepository);

            var logger = new EasyLogAdapter(appSettings);
            container.Register<ILogger>(logger);

            // ----------------------------------------------------------------
            // Backup strategies
            // ----------------------------------------------------------------

            var fullStrategy = new FullBackupStrategy();
            var differentialStrategy = new DifferentialBackupStrategy();

            // ----------------------------------------------------------------
            // Core services
            // ----------------------------------------------------------------

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

            // ----------------------------------------------------------------
            // Console application services
            // ----------------------------------------------------------------

            var localizer = new ResourceLocalizer();
            container.Register<ILocalizer>(localizer);

            var view = new ConsoleView(localizer);
            container.Register<IConsoleView>(view);

            var parser = new CliArgumentParser();
            container.Register<CliArgumentParser>(parser);

            var menuController = new MenuController(
                jobManager,
                backupService,
                view,
                localizer,
                appSettings);

            container.Register<MenuController>(menuController);

            var app = new App(
                backupService,
                jobManager,
                view,
                localizer,
                menuController,
                parser);

            container.Register<App>(app);

            // ----------------------------------------------------------------
            // Startup routine
            // ----------------------------------------------------------------
            // If the previous execution crashed, state.json may still contain
            // active jobs. We reset runtime states at startup.

            stateRepository.Clear(jobManager.GetAll());

            return container;
        }
    }
}
