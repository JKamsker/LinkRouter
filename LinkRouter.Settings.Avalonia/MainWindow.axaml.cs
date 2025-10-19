using Avalonia.Controls;
using LinkRouter.Settings.ViewModels;

namespace LinkRouter.Settings.Avalonia;

public partial class MainWindow : Window
{
    public MainWindow(SettingsShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
