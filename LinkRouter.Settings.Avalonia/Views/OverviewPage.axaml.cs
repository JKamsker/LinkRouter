using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LinkRouter.Settings.Avalonia.Views;

public partial class OverviewPage : UserControl
{
    public OverviewPage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

