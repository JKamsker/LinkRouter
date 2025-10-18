using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LinkRouter.Settings.ViewModels;

namespace LinkRouter.Settings.Avalonia.Views;

public partial class DefaultRulePage : UserControl
{
    public DefaultRulePage()
    {
        InitializeComponent();
        DataContext = new DefaultViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
