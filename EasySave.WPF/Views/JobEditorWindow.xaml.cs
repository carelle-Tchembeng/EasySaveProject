// EasySave.WPF/Views/JobEditorWindow.xaml.cs
using EasySave.WPF.ViewModels;
using System.Windows;
namespace EasySave.WPF.Views
{
    public partial class JobEditorWindow : Window
    {
        public JobEditorWindow()
        {
            InitializeComponent();
            DataContextChanged += (_, e) =>
            {
                if (e.NewValue is JobEditorViewModel vm)
                    vm.Closed += (_, _) => Close();
            };
        }
    }
}
