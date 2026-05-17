// EasySave.WPF/App.xaml.cs
// Entry point — detects CLI mode, wires all dependencies
using System.IO;
using EasySave.Core.Entities;
using EasySave.Core.Interfaces;
using EasySave.Core.Services;
using EasySave.Infrastructure.Configuration;
using EasySave.Infrastructure.Detection;
using EasySave.Infrastructure.Encryption;
using EasySave.Infrastructure.FileSystem;
using EasySave.Infrastructure.Logging;
using EasySave.Infrastructure.Repositories;
using EasySave.Infrastructure.Strategies;
using EasySave.WPF.DI;
using EasySave.WPF.Localization;
using EasySave.WPF.ViewModels;
using EasySave.WPF.Views;
using EasyLog;
using EasyLog.Factory;
using System.Windows;

namespace EasySave.WPF
{
    public partial class App : Application
    {
        private ServiceContainer? _container;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            AppSettings settings = AppSettings.Load(settingsPath);

            _container = BuildContainer(settings);

            // CLI mode: run headless and exit
            if (IsCli(e.Args))
            {
                RunCli(e.Args);
                Shutdown();
                return;
            }

            // GUI mode: open main window
            var mainVM = _container.Resolve<MainViewModel>();
            var window = new MainWindow { DataContext = mainVM };
            MainWindow = window;
            window.Show();
        }

        private bool IsCli(string[] args) => args.Length > 0;

        private void RunCli(string[] args)
        {
            var backupService = _container!.Resolve<BackupService>();
            var jobManager    = _container!.Resolve<JobManager>();

            // Parse args: support "1-3" range and "1;3" list (same as v1.0/v1.1)
            var indices = ParseCliArgs(args, jobManager.Count);
            if (indices.Count > 0)
                backupService.ExecuteList(indices);
        }

        private static List<int> ParseCliArgs(string[] args, int maxJobs)
        {
            string token = string.Join("", args).Trim();
            if (string.IsNullOrEmpty(token)) return new List<int>();

            List<int> indices;
            int hyphenIdx = token.IndexOf('-');
            if (hyphenIdx > 0 && hyphenIdx < token.Length - 1)
            {
                // Range: 1-3
                if (int.TryParse(token[..hyphenIdx], out int from) &&
                    int.TryParse(token[(hyphenIdx+1)..], out int to))
                    indices = Enumerable.Range(from, to - from + 1).ToList();
                else indices = new List<int>();
            }
            else if (token.Contains(';'))
            {
                // List: 1;3
                indices = token.Split(';')
                    .Where(p => int.TryParse(p.Trim(), out _))
                    .Select(int.Parse).ToList();
            }
            else
            {
                indices = int.TryParse(token, out int single) ? new List<int> { single } : new List<int>();
            }

            return indices.Where(i => i >= 1 && i <= maxJobs).Distinct().OrderBy(i => i).ToList();
        }

        private static ServiceContainer BuildContainer(AppSettings settings)
        {
            var container = new ServiceContainer();

            // 1. D'ABORD : La Configuration (Indispensable pour tout le reste)
            var appConfigRepo = new JsonAppConfigRepository(settings.AppConfigFilePath);
            container.Register<IAppConfigRepository>(appConfigRepo);
            AppConfiguration config = appConfigRepo.Load();
            container.Register<AppConfiguration>(config);

            // 2. ENSUITE : La Localisation (On en a besoin pour les ViewModels)
            var localization = new LocalizationService();
            container.Register<LocalizationService>(localization);

            // 3. LE LOG : Configuration sécurisée du chemin
            if (Enum.TryParse<LogFormat>(config.LogFormat, ignoreCase: true, out var logFmt) == false)
                logFmt = LogFormat.JSON;

            string logDirectory = settings.LogDirectory;
            if (string.IsNullOrWhiteSpace(logDirectory))
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                logDirectory = Path.Combine(appData, "EasySave", "logs");
            }

            if (!Directory.Exists(logDirectory)) Directory.CreateDirectory(logDirectory);

            IEasyLogWriter logWriter = LogWriterFactory.Create(logDirectory, logFmt);
            var logAdapter = new EasyLogAdapter(logWriter);
            container.Register<ILogger>(logAdapter);
            container.Register<EasyLogAdapter>(logAdapter);

            // 4. INFRASTRUCTURE & CORE
            var configRepo = new JsonConfigRepository(settings.ConfigFilePath);
            var stateRepo = new JsonStateRepository(settings.StateFilePath);
            var fileSystem = new WindowsFileSystem();
            var cryptoAdapter = new CryptoSoftAdapter(config);
            var bizDetector = new BusinessSoftwareDetector();
            var fullStrategy = new FullBackupStrategy();
            var diffStrategy = new DifferentialBackupStrategy();

            container.Register<IConfigRepository>(configRepo);
            container.Register<IStateRepository>(stateRepo);
            container.Register<IFileSystem>(fileSystem);
            container.Register<IEncryptionService>(cryptoAdapter);
            container.Register<IBusinessSoftwareDetector>(bizDetector);

            var jobManager = new JobManager(configRepo, fileSystem);
            container.Register<JobManager>(jobManager);

            var backupService = new BackupService(
                jobManager, fileSystem, stateRepo, logAdapter,
                fullStrategy, diffStrategy, cryptoAdapter, bizDetector, config);
            container.Register<BackupService>(backupService);

            stateRepo.Clear(jobManager.GetAll());

            // 5. EN DERNIER : Les ViewModels (Une fois que toutes leurs dépendances existent)
            // On passe bien les 4 arguments : config, repo, adapter, ET localization
            var settingsVM = new SettingsViewModel(config, appConfigRepo, logAdapter, localization);
            container.Register<SettingsViewModel>(settingsVM);

            var mainVM = new MainViewModel(jobManager, backupService, config, appConfigRepo, settingsVM, localization);
            container.Register<MainViewModel>(mainVM);

            return container;
        }
    }
}
