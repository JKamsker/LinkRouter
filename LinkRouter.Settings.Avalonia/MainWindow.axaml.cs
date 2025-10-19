using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using LinkRouter.Settings.ViewModels;

namespace LinkRouter.Settings.Avalonia;

public partial class MainWindow : Window
{
    public MainWindow(SettingsShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        TransparencyLevelHint = new[]
        {
            WindowTransparencyLevel.Mica,
            WindowTransparencyLevel.AcrylicBlur,
            WindowTransparencyLevel.Blur,
            WindowTransparencyLevel.Transparent
        };

        Background = Brushes.Transparent;
    }
}
