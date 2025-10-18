using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LinkRouter.Settings.Avalonia.Views;

public partial class DefaultRulePage : UserControl
{
    public DefaultRulePage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

