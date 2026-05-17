// EasySave.WPF/Views/SettingsWindow.xaml.cs
using System.Windows;
using EasySave.WPF.ViewModels;

namespace EasySave.WPF.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            // On écoute quand le DataContext (le ViewModel) change
            this.DataContextChanged += (s, e) =>
            {
                if (DataContext is SettingsViewModel vm)
                {
                    // On s'abonne à la demande de fermeture du ViewModel
                    vm.RequestClose += (sender, args) => this.Close();
                }
            };
        }
    }
}