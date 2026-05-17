// EasySave.WPF/ViewModels/MainViewModel.cs
using EasySave.Core.Entities;
using EasySave.Core.Interfaces;
using EasySave.Core.Services;
using EasySave.WPF.Commands;
using EasySave.WPF.Localization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EasySave.WPF.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly JobManager _jobManager;
        private readonly BackupService _backupService;
        private readonly IAppConfigRepository _configRepo;
        private readonly AppConfiguration _config;
        private readonly LocalizationService _localizationService;

        public LocalizationService L => _localizationService;

        // ── Bindable collections ───────────────────────────────────
        public ObservableCollection<BackupJobViewModel> Jobs { get; } = new();

        private BackupJobViewModel? _selectedJob;
        public BackupJobViewModel? SelectedJob
        {
            get => _selectedJob;
            set
            {
                SetField(ref _selectedJob, value);
                OnPropertyChanged(nameof(HasSelectedJob));
            }
        }

        public bool HasSelectedJob => SelectedJob != null;
        public SettingsViewModel SettingsVM { get; }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetField(ref _statusMessage, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                SetField(ref _isBusy, value);
                OnPropertyChanged(nameof(IsNotBusy));
            }
        }

        public bool IsNotBusy => !_isBusy;

        // ── Commands ───────────────────────────────────────────────
        public ICommand StartAllCommand { get; }
        public ICommand AddJobCommand { get; }
        public ICommand EditJobCommand { get; }
        public ICommand DeleteJobCommand { get; }
        public ICommand OpenSettingsCommand { get; }

        public event EventHandler<JobEditorViewModel>? OpenJobEditorRequested;
        public event EventHandler<SettingsViewModel>? OpenSettingsRequested;

        // ── Constructor ────────────────────────────────────────────
        public MainViewModel(
            JobManager jobManager,
            BackupService backupService,
            AppConfiguration config,
            IAppConfigRepository configRepo,
            SettingsViewModel settingsVM,
            LocalizationService localizationService)
        {
            _jobManager = jobManager;
            _backupService = backupService;
            _config = config;
            _configRepo = configRepo;
            SettingsVM = settingsVM;
            _localizationService = localizationService;

            StartAllCommand = new RelayCommand(async () => await StartAllAsync(), () => IsNotBusy);
            AddJobCommand = new RelayCommand(OnAddJob, () => IsNotBusy);
            EditJobCommand = new RelayCommand(OnEditJob, () => HasSelectedJob && IsNotBusy);
            DeleteJobCommand = new RelayCommand(OnDeleteJob, () => HasSelectedJob && IsNotBusy);
            OpenSettingsCommand = new RelayCommand(OnOpenSettings);

            _localizationService.LanguageChanged += (s, e) =>
            {
                OnPropertyChanged(string.Empty);
            };

            LoadJobs();
        }

        // ── Public methods ─────────────────────────────────────────
        public void LoadJobs()
        {
            Jobs.Clear();
            foreach (var job in _jobManager.GetAll())
                Jobs.Add(new BackupJobViewModel(job, _backupService));
        }

        public async Task StartAllAsync()
        {
            IsBusy = true;
            StatusMessage = _localizationService.Get("job.status.active");
            try
            {
                await Task.Run(() => _backupService.ExecuteAll());
                StatusMessage = _localizationService.Get("job.status.completed");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
                LoadJobs();
            }
        }

        // ── Private ────────────────────────────────────────────────
        private void OnAddJob()
        {
            var editorVM = new JobEditorViewModel(_jobManager);
            editorVM.Closed += (_, saved) => { if (saved) LoadJobs(); };
            OpenJobEditorRequested?.Invoke(this, editorVM);
        }

        private void OnEditJob()
        {
            if (SelectedJob == null) return;
            var job = _jobManager.GetById(SelectedJob.Id);
            if (job == null) return;

            var editorVM = new JobEditorViewModel(_jobManager, job);
            editorVM.Closed += (_, saved) => { if (saved) LoadJobs(); };
            OpenJobEditorRequested?.Invoke(this, editorVM);
        }

        private void OnDeleteJob()
        {
            if (SelectedJob == null) return;
            _jobManager.Delete(SelectedJob.Id);
            LoadJobs();
        }

        private void OnOpenSettings()
            => OpenSettingsRequested?.Invoke(this, SettingsVM);
    }
}