using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LinkRouter.Settings.Core.ViewModels;

namespace LinkRouter.Settings.Avalonia.Views;

public partial class AdvancedPage : UserControl
{
    public AdvancedPage()
    {
        InitializeComponent();
        DataContext = new AdvancedViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
