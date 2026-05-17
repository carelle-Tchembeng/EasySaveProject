// EasySave.WPF/Views/MainWindow.xaml.cs
// Code-behind for MainWindow.xaml

using EasySave.WPF.ViewModels;
using System.Windows;

namespace EasySave.WPF.Views
{
    /// <summary>
    /// Code-behind for the main application window.
    /// DataContext is set to MainViewModel in App.OnStartup().
    /// The View listens to ViewModel events to open dialogs.
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel? _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is MainViewModel vm)
            {
                _viewModel = vm;
                _viewModel.OpenJobEditorRequested  += OnOpenJobEditor;
                _viewModel.OpenSettingsRequested   += OnOpenSettings;
            }
        }

        private void OnOpenJobEditor(object? sender, JobEditorViewModel editorVM)
        {
            var dialog = new JobEditorWindow { DataContext = editorVM, Owner = this };
            dialog.ShowDialog();
        }

        private void OnOpenSettings(object? sender, SettingsViewModel settingsVM)
        {
            var dialog = new SettingsWindow { DataContext = settingsVM, Owner = this };
            dialog.ShowDialog();
        }
    }
}
