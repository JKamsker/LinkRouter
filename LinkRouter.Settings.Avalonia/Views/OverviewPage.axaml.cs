using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LinkRouter.Settings.Core.ViewModels;

namespace LinkRouter.Settings.Avalonia.Views;

public partial class OverviewPage : UserControl
{
    public OverviewPage()
    {
        InitializeComponent();
        DataContext = new GeneralViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
