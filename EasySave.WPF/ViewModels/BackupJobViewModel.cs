// EasySave.WPF/ViewModels/BackupJobViewModel.cs
// Per corrected diagram: wraps BackupJob model, exposes RunAsync(), OnPropertyChanged()

using EasySave.Core.Entities;
using EasySave.Core.Interfaces;
using EasySave.Core.Services;
using EasySave.WPF.Commands;
using System.Windows.Input;

namespace EasySave.WPF.ViewModels
{
    /// <summary>
    /// ViewModel wrapping a single BackupJob for display in the jobs list.
    /// Per corrected diagram: exposes Name, SourcePath, TargetPath, Status, Progress, RunCommand.
    /// </summary>
    public class BackupJobViewModel : ViewModelBase
    {
        // ── Model ──────────────────────────────────────────────────
        private BackupJob         _model;
        private readonly BackupService _backupService;

        // ── Bindable properties ────────────────────────────────────
        public Guid   Id         => _model.Id;

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

        private double _progress;
        public  double Progress
        {
            get => _progress;
            set => SetField(ref _progress, value);
        }

        // ── Commands ───────────────────────────────────────────────
        public ICommand RunCommand { get; }

        // ── Constructor ────────────────────────────────────────────

        public BackupJobViewModel(BackupJob model, BackupService backupService)
        {
            _model         = model;
            _backupService = backupService;
            RunCommand     = new RelayCommand(async () => await RunAsync());
            RefreshFromModel();
        }

        // ── Methods ────────────────────────────────────────────────

        /// <summary>
        /// Executes this backup job asynchronously.
        /// Updates the UI via property bindings during execution.
        /// </summary>
        public async Task RunAsync()
        {
            await Task.Run(() => _backupService.ExecuteOne(_model.Id));
            RefreshFromModel();
        }

        /// <summary>Refreshes all bound properties from the underlying model.</summary>
        public void RefreshFromModel()
        {
            Name       = _model.Name;
            SourcePath = _model.SourcePath;
            TargetPath = _model.TargetPath;
            Status     = _model.Status.ToString();
            Progress   = _model.Progress?.ProgressPercent ?? 0;
        }

        /// <summary>Updates the underlying model reference and refreshes bindings.</summary>
        public void UpdateModel(BackupJob updated)
        {
            _model = updated;
            RefreshFromModel();
        }
    }
}
