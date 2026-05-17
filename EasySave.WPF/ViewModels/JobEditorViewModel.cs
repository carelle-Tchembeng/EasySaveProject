// EasySave.WPF/ViewModels/JobEditorViewModel.cs
// Per corrected diagram: Name, SourcePath, TargetPath, Type, SaveCommand, CancelCommand

using EasySave.Core.Entities;
using EasySave.Core.Enums;
using EasySave.Core.Services;
using EasySave.WPF.Commands;
using System.Windows.Input;

namespace EasySave.WPF.ViewModels
{
    /// <summary>
    /// ViewModel for the job creation/editing form.
    /// Per corrected diagram: SaveCommand, CancelCommand, backed by JobManager.
    /// </summary>
    public class JobEditorViewModel : ViewModelBase
    {
        private readonly JobManager _jobManager;
        private readonly Guid?      _editId;

        // ── Bindable properties ────────────────────────────────────

        private string _name = string.Empty;
        public  string Name
        {
            get => _name;
            set { SetField(ref _name, value); OnPropertyChanged(nameof(CanSave)); }
        }

        private string _sourcePath = string.Empty;
        public  string SourcePath
        {
            get => _sourcePath;
            set { SetField(ref _sourcePath, value); OnPropertyChanged(nameof(CanSave)); }
        }

        private string _targetPath = string.Empty;
        public  string TargetPath
        {
            get => _targetPath;
            set { SetField(ref _targetPath, value); OnPropertyChanged(nameof(CanSave)); }
        }

        private BackupType _type = BackupType.Full;
        public  BackupType Type
        {
            get => _type;
            set => SetField(ref _type, value);
        }

        public bool CanSave =>
            !string.IsNullOrWhiteSpace(Name) &&
            !string.IsNullOrWhiteSpace(SourcePath) &&
            !string.IsNullOrWhiteSpace(TargetPath);

        public bool IsEditMode => _editId.HasValue;

        // ── Commands ───────────────────────────────────────────────
        public ICommand SaveCommand   { get; }
        public ICommand CancelCommand { get; }

        /// <summary>Raised when the form is saved or cancelled, to close the dialog.</summary>
        public event EventHandler<bool>? Closed;

        // ── Constructor ────────────────────────────────────────────

        /// <summary>Create mode: no existing job.</summary>
        public JobEditorViewModel(JobManager jobManager)
        {
            _jobManager   = jobManager;
            _editId       = null;
            SaveCommand   = new RelayCommand(OnSave, () => CanSave);
            CancelCommand = new RelayCommand(() => Closed?.Invoke(this, false));
        }

        /// <summary>Edit mode: pre-fills form with existing job.</summary>
        public JobEditorViewModel(JobManager jobManager, BackupJob existing)
        {
            _jobManager   = jobManager;
            _editId       = existing.Id;
            Name          = existing.Name;
            SourcePath    = existing.SourcePath;
            TargetPath    = existing.TargetPath;
            Type          = existing.Type;
            SaveCommand   = new RelayCommand(OnSave, () => CanSave);
            CancelCommand = new RelayCommand(() => Closed?.Invoke(this, false));
        }

        // ── Private ────────────────────────────────────────────────

        private void OnSave()
        {
            try
            {
                if (_editId.HasValue)
                    _jobManager.Update(_editId.Value, Name, SourcePath, TargetPath, Type);
                else
                    _jobManager.Add(Name, SourcePath, TargetPath, Type);

                Closed?.Invoke(this, true);
            }
            catch (ArgumentException ex)
            {
                // Surface validation error to the View via binding
                Name = $"ERROR: {ex.Message}";
            }
        }
    }
}
