using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LinkRouter.Settings.Avalonia.Views;

public partial class BrowsersProfilesPage : UserControl
{
    public BrowsersProfilesPage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

