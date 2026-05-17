// EasySave.WPF/ViewModels/SettingsViewModel.cs
// UPDATED v3.0 — adds PriorityExtensions, MaxParallelFileSizeKb, LogStorageMode, LogServerUrl

using EasySave.Core.Entities;
using EasySave.Core.Interfaces;
using EasySave.Infrastructure.Logging;
using EasySave.WPF.Commands;
using EasySave.WPF.Localization;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace EasySave.WPF.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly AppConfiguration    _config;
        private readonly IAppConfigRepository _configRepo;
        private readonly EasyLogAdapter       _logAdapter;
        private readonly LocalizationService  _localizationService;

        public event EventHandler?         RequestClose;
        public LocalizationService L => _localizationService;

        // ── v2.0 settings ──────────────────────────────────────────

        private string _language = "en";
        public  string Language { get => _language; set => SetField(ref _language, value); }

        private string _logFormat = "JSON";
        public  string LogFormat { get => _logFormat; set => SetField(ref _logFormat, value); }

        private string _businessSoftwareName = string.Empty;
        public  string BusinessSoftwareName { get => _businessSoftwareName; set => SetField(ref _businessSoftwareName, value); }

        private string _cryptoSoftPath = string.Empty;
        public  string CryptoSoftPath { get => _cryptoSoftPath; set => SetField(ref _cryptoSoftPath, value); }

        public ObservableCollection<string> EncryptedExtensions { get; } = new();

        private string _newExtension = string.Empty;
        public  string NewExtension
        {
            get => _newExtension;
            set { SetField(ref _newExtension, value); OnPropertyChanged(nameof(CanAddExtension)); }
        }
        public bool CanAddExtension =>
            !string.IsNullOrWhiteSpace(NewExtension) &&
            !EncryptedExtensions.Contains(NewExtension.ToLowerInvariant().Trim());

        // ── v3.0 settings ──────────────────────────────────────────

        /// <summary>NEW v3.0 — File extensions with transfer priority.</summary>
        public ObservableCollection<string> PriorityExtensions { get; } = new();

        private string _newPriorityExtension = string.Empty;
        public  string NewPriorityExtension
        {
            get => _newPriorityExtension;
            set { SetField(ref _newPriorityExtension, value); OnPropertyChanged(nameof(CanAddPriorityExtension)); }
        }
        public bool CanAddPriorityExtension =>
            !string.IsNullOrWhiteSpace(NewPriorityExtension) &&
            !PriorityExtensions.Contains(NewPriorityExtension.ToLowerInvariant().Trim());

        /// <summary>NEW v3.0 — Max file size (KB) that can be transferred simultaneously.</summary>
        private string _maxParallelFileSizeKb = "0";
        public  string MaxParallelFileSizeKb
        {
            get => _maxParallelFileSizeKb;
            set => SetField(ref _maxParallelFileSizeKb, value);
        }

        /// <summary>NEW v3.0 — Log storage destination: Local, Remote, or Both.</summary>
        private string _logStorageMode = "Local";
        public  string LogStorageMode
        {
            get => _logStorageMode;
            set => SetField(ref _logStorageMode, value);
        }

        /// <summary>NEW v3.0 — URL of the centralised Docker log server.</summary>
        private string _logServerUrl = string.Empty;
        public  string LogServerUrl
        {
            get => _logServerUrl;
            set => SetField(ref _logServerUrl, value);
        }

        // ── Commands ───────────────────────────────────────────────
        public ICommand SaveSettingsCommand             { get; }
        public ICommand AddExtensionCommand             { get; }
        public ICommand RemoveExtensionCommand          { get; }
        public ICommand AddPriorityExtensionCommand     { get; }  // NEW v3.0
        public ICommand RemovePriorityExtensionCommand  { get; }  // NEW v3.0

        // ── Constructor ────────────────────────────────────────────
        public SettingsViewModel(
            AppConfiguration     config,
            IAppConfigRepository configRepo,
            EasyLogAdapter       logAdapter,
            LocalizationService  localizationService)
        {
            _config              = config;
            _configRepo          = configRepo;
            _logAdapter          = logAdapter;
            _localizationService = localizationService;

            // Pre-fill v2 fields
            Language             = config.DefaultLanguage;
            LogFormat            = config.LogFormat;
            BusinessSoftwareName = config.BusinessSoftwareName;
            CryptoSoftPath       = config.CryptoSoftPath;
            foreach (var ext in config.EncryptedExtensions) EncryptedExtensions.Add(ext);

            // Pre-fill v3 fields
            foreach (var ext in config.PriorityExtensions) PriorityExtensions.Add(ext);
            MaxParallelFileSizeKb = config.MaxParallelFileSizeKb.ToString();
            LogStorageMode        = config.LogStorageMode;
            LogServerUrl          = config.LogServerUrl;

            SaveSettingsCommand            = new RelayCommand(OnSave);
            AddExtensionCommand            = new RelayCommand(OnAddExtension,         () => CanAddExtension);
            RemoveExtensionCommand         = new RelayCommand<string>(OnRemoveExtension);
            AddPriorityExtensionCommand    = new RelayCommand(OnAddPriorityExtension, () => CanAddPriorityExtension);
            RemovePriorityExtensionCommand = new RelayCommand<string>(OnRemovePriorityExtension);

            _localizationService.LanguageChanged += (_, _) => OnPropertyChanged(string.Empty);
        }

        // ── Save ───────────────────────────────────────────────────
        private void OnSave()
        {
            _config.DefaultLanguage      = Language;
            _config.LogFormat            = LogFormat;
            _config.BusinessSoftwareName = BusinessSoftwareName;
            _config.CryptoSoftPath       = CryptoSoftPath;
            _config.EncryptedExtensions  = EncryptedExtensions.ToList();

            // v3.0 settings
            _config.PriorityExtensions   = PriorityExtensions.ToList();
            _config.MaxParallelFileSizeKb = long.TryParse(MaxParallelFileSizeKb, out long kb) ? kb : 0;
            _config.LogStorageMode       = LogStorageMode;
            _config.LogServerUrl         = LogServerUrl;

            _configRepo.Save(_config);
            _localizationService.SetLanguage(Language);

            if (Enum.TryParse<EasyLog.LogFormat>(LogFormat, ignoreCase: true, out var fmt))
                _logAdapter.SetFormat(fmt);

            // Reconfigure remote logging
            _logAdapter.ConfigureRemoteLogging(
                _config.IsLocalLoggingEnabled(),
                _config.IsRemoteLoggingEnabled(),
                _config.LogServerUrl);

            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        // ── Extension management (encrypted) ───────────────────────
        private void OnAddExtension()
        {
            string ext = NormalizeExtension(NewExtension);
            if (!EncryptedExtensions.Contains(ext)) EncryptedExtensions.Add(ext);
            NewExtension = string.Empty;
        }

        private void OnRemoveExtension(string? ext)
        {
            if (!string.IsNullOrWhiteSpace(ext)) EncryptedExtensions.Remove(ext);
        }

        // ── Extension management (priority) ────────────────────────
        private void OnAddPriorityExtension()
        {
            string ext = NormalizeExtension(NewPriorityExtension);
            if (!PriorityExtensions.Contains(ext)) PriorityExtensions.Add(ext);
            NewPriorityExtension = string.Empty;
        }

        private void OnRemovePriorityExtension(string? ext)
        {
            if (!string.IsNullOrWhiteSpace(ext)) PriorityExtensions.Remove(ext);
        }

        private static string NormalizeExtension(string ext)
        {
            ext = ext.Trim().ToLowerInvariant();
            return ext.StartsWith('.') ? ext : $".{ext}";
        }
    }
}
