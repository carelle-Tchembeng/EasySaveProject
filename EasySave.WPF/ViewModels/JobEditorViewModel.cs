// EasySave.WPF/ViewModels/JobEditorViewModel.cs
// UPDATED — added LocalizationService (L property) and TypeIndex for localized ComboBox

using EasySave.Core.Entities;
using EasySave.Core.Enums;
using EasySave.Core.Services;
using EasySave.WPF.Commands;
using EasySave.WPF.Localization;
using System.Windows.Input;

namespace EasySave.WPF.ViewModels
{
    /// <summary>
    /// ViewModel for the job creation/editing form.
    /// Per corrected diagram: SaveCommand, CancelCommand, backed by JobManager.
    /// </summary>
    public class JobEditorViewModel : ViewModelBase
    {
        private readonly JobManager          _jobManager;
        private readonly Guid?               _editId;
        private readonly LocalizationService _localizationService;

        // ── Localization ───────────────────────────────────────────
        /// <summary>Exposes the localization service for {Binding L[key]} in XAML.</summary>
        public LocalizationService L => _localizationService;

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

        /// <summary>
        /// 0 = Full, 1 = Differential.
        /// Used by the ComboBox (SelectedIndex) so localized item labels can be arbitrary strings.
        /// </summary>
        public int TypeIndex
        {
            get => (int)_type;
            set
            {
                _type = (BackupType)value;
                OnPropertyChanged(nameof(TypeIndex));
                OnPropertyChanged(nameof(Type));
            }
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
        public JobEditorViewModel(JobManager jobManager, LocalizationService localizationService)
        {
            _jobManager          = jobManager;
            _localizationService = localizationService;
            _editId              = null;
            SaveCommand          = new RelayCommand(OnSave, () => CanSave);
            CancelCommand        = new RelayCommand(() => Closed?.Invoke(this, false));

            // Refresh all bindings on language change
            _localizationService.LanguageChanged += (_, _) => OnPropertyChanged(string.Empty);
        }

        /// <summary>Edit mode: pre-fills form with existing job.</summary>
        public JobEditorViewModel(JobManager jobManager, LocalizationService localizationService, BackupJob existing)
        {
            _jobManager          = jobManager;
            _localizationService = localizationService;
            _editId              = existing.Id;
            Name                 = existing.Name;
            SourcePath           = existing.SourcePath;
            TargetPath           = existing.TargetPath;
            Type                 = existing.Type;
            SaveCommand          = new RelayCommand(OnSave, () => CanSave);
            CancelCommand        = new RelayCommand(() => Closed?.Invoke(this, false));

            _localizationService.LanguageChanged += (_, _) => OnPropertyChanged(string.Empty);
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
