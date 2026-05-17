// EasySave.WPF/ViewModels/BackupJobViewModel.cs
// UPDATED v3.0 — adds PauseCommand, ResumeCommand, StopCommand + real-time progress polling

using EasySave.Core.Entities;
using EasySave.Core.Enums;
using EasySave.Core.Services;
using EasySave.WPF.Commands;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace EasySave.WPF.ViewModels
{
    /// <summary>
    /// ViewModel wrapping a single BackupJob for display in the jobs list.
    /// v3.0: adds PauseCommand, ResumeCommand, StopCommand and real-time progress via DispatcherTimer.
    /// </summary>
    public class BackupJobViewModel : ViewModelBase
    {
        // ── Model ──────────────────────────────────────────────────
        private BackupJob         _model;
        private readonly BackupService _backupService;
        private DispatcherTimer?  _progressTimer;

        // ── Bindable properties ────────────────────────────────────
        public Guid Id => _model.Id;

        private string _name = string.Empty;
        public  string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        private string _sourcePath = string.Empty;
        public  string SourcePath
        {
            get => _sourcePath;
            set => SetField(ref _sourcePath, value);
        }

        private string _targetPath = string.Empty;
        public  string TargetPath
        {
            get => _targetPath;
            set => SetField(ref _targetPath, value);
        }

        private string _status = string.Empty;
        public  string Status
        {
            get => _status;
            set => SetField(ref _status, value);
        }

        private double _progressPercent;
        public  double ProgressPercent
        {
            get => _progressPercent;
            set => SetField(ref _progressPercent, value);
        }

        // ── Commands ───────────────────────────────────────────────
        public ICommand RunCommand    { get; }
        public ICommand PauseCommand  { get; }
        public ICommand ResumeCommand { get; }
        public ICommand StopCommand   { get; }

        // ── Constructor ────────────────────────────────────────────
        public BackupJobViewModel(BackupJob model, BackupService backupService)
        {
            _model         = model;
            _backupService = backupService;

            RunCommand    = new RelayCommand(async () => await RunAsync());
            PauseCommand  = new RelayCommand(OnPause,  CanPause);
            ResumeCommand = new RelayCommand(OnResume, CanResume);
            StopCommand   = new RelayCommand(OnStop,   CanStop);

            RefreshFromModel();
        }

        // ── Command implementations ────────────────────────────────

        public async Task RunAsync()
        {
            StartProgressTimer();
            try
            {
                await Task.Run(() => _backupService.ExecuteOne(_model.Id));
            }
            finally
            {
                StopProgressTimer();
                RefreshFromModel();
            }
        }

        private void OnPause()  => _backupService.PauseJob(_model.Id);
        private void OnResume() => _backupService.ResumeJob(_model.Id);
        private void OnStop()   => _backupService.StopJob(_model.Id);

        private bool CanPause()  => _model.Status == BackupStatus.Active;
        private bool CanResume() => _model.Status == BackupStatus.Paused;
        private bool CanStop()   => _model.Status == BackupStatus.Active || _model.Status == BackupStatus.Paused;

        // ── Progress timer ─────────────────────────────────────────

        private void StartProgressTimer()
        {
            _progressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            _progressTimer.Tick += (_, _) => RefreshFromModel();
            _progressTimer.Start();
        }

        private void StopProgressTimer()
        {
            _progressTimer?.Stop();
            _progressTimer = null;
        }

        // ── Refresh ────────────────────────────────────────────────

        /// <summary>Refreshes all bound properties from the underlying model.</summary>
        public void RefreshFromModel()
        {
            Name           = _model.Name;
            SourcePath     = _model.SourcePath;
            TargetPath     = _model.TargetPath;
            Status         = _model.Status.ToString();
            ProgressPercent = _model.Progress?.ProgressPercent ?? 0;
        }

        /// <summary>Updates the underlying model reference and refreshes bindings.</summary>
        public void UpdateModel(BackupJob updated)
        {
            _model = updated;
            RefreshFromModel();
        }
    }
}
