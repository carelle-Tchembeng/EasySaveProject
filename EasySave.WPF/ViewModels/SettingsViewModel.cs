// EasySave.WPF/ViewModels/SettingsViewModel.cs
using EasySave.Core.Entities;
using EasySave.Core.Interfaces;
using EasySave.Infrastructure.Logging;
using EasySave.WPF.Commands;
using EasySave.WPF.Localization;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace EasySave.WPF.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly AppConfiguration _config;
        private readonly IAppConfigRepository _configRepo;
        private readonly EasyLogAdapter _logAdapter;
        private readonly LocalizationService _localizationService;

        // ── Evenement pour fermer la fenêtre ───────────────────────
        public event EventHandler? RequestClose;

        // ── Localization Proxy ──────────────────────────────────────
        public LocalizationService L => _localizationService;

        // ── Bindable properties ────────────────────────────────────
        private string _language = "en";
        public string Language
        {
            get => _language;
            set => SetField(ref _language, value);
        }

        private string _logFormat = "JSON";
        public string LogFormat
        {
            get => _logFormat;
            set => SetField(ref _logFormat, value);
        }

        private string _businessSoftwareName = string.Empty;
        public string BusinessSoftwareName
        {
            get => _businessSoftwareName;
            set => SetField(ref _businessSoftwareName, value);
        }

        private string _cryptoSoftPath = string.Empty;
        public string CryptoSoftPath
        {
            get => _cryptoSoftPath;
            set => SetField(ref _cryptoSoftPath, value);
        }

        public ObservableCollection<string> EncryptedExtensions { get; } = new();

        private string _newExtension = string.Empty;
        public string NewExtension
        {
            get => _newExtension;
            set { SetField(ref _newExtension, value); OnPropertyChanged(nameof(CanAddExtension)); }
        }

        public bool CanAddExtension =>
            !string.IsNullOrWhiteSpace(NewExtension) &&
            !EncryptedExtensions.Contains(NewExtension.ToLowerInvariant().Trim());

        // ── Commands ───────────────────────────────────────────────
        public ICommand SaveSettingsCommand { get; }
        public ICommand AddExtensionCommand { get; }
        public ICommand RemoveExtensionCommand { get; }

        // ── Constructor ────────────────────────────────────────────
        public SettingsViewModel(
            AppConfiguration config,
            IAppConfigRepository configRepo,
            EasyLogAdapter logAdapter,
            LocalizationService localizationService)
        {
            _config = config;
            _configRepo = configRepo;
            _logAdapter = logAdapter;
            _localizationService = localizationService;

            // Pre-fill
            Language = config.DefaultLanguage;
            LogFormat = config.LogFormat;
            BusinessSoftwareName = config.BusinessSoftwareName;
            CryptoSoftPath = config.CryptoSoftPath;
            foreach (var ext in config.EncryptedExtensions)
                EncryptedExtensions.Add(ext);

            SaveSettingsCommand = new RelayCommand(OnSave);
            AddExtensionCommand = new RelayCommand(OnAddExtension, () => CanAddExtension);
            RemoveExtensionCommand = new RelayCommand<string>(OnRemoveExtension);

            _localizationService.LanguageChanged += (s, e) => OnPropertyChanged(string.Empty);
        }

        private void OnSave()
        {
            _config.DefaultLanguage = Language;
            _config.LogFormat = LogFormat;
            _config.BusinessSoftwareName = BusinessSoftwareName;
            _config.CryptoSoftPath = CryptoSoftPath;
            _config.EncryptedExtensions = EncryptedExtensions.ToList();

            _configRepo.Save(_config);
            _localizationService.SetLanguage(Language);

            if (Enum.TryParse<EasyLog.LogFormat>(LogFormat, ignoreCase: true, out var fmt))
                _logAdapter.SetFormat(fmt);

            // 🛠️ DÉCLENCHEMENT DE LA FERMETURE
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        private void OnAddExtension()
        {
            string ext = NewExtension.Trim().ToLowerInvariant();
            if (!ext.StartsWith('.')) ext = $".{ext}";
            if (!EncryptedExtensions.Contains(ext))
                EncryptedExtensions.Add(ext);
            NewExtension = string.Empty;
        }

        private void OnRemoveExtension(string? ext)
        {
            if (!string.IsNullOrWhiteSpace(ext))
                EncryptedExtensions.Remove(ext);
        }
    }
}