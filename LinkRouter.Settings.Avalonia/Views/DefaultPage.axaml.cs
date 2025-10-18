using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LinkRouter.Settings.Core.ViewModels;

namespace LinkRouter.Settings.Avalonia.Views;

public partial class DefaultPage : UserControl
{
    public DefaultPage()
    {
        InitializeComponent();
        DataContext = new DefaultViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
