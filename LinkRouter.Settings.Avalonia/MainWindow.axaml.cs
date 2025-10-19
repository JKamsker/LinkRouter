using Avalonia.Controls;
using LinkRouter.Settings.Avalonia.ViewModels;

namespace LinkRouter.Settings.Avalonia;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
