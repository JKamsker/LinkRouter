using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LinkRouter.Settings.Core.ViewModels;

namespace LinkRouter.Settings.Avalonia.Views;

public partial class RulesPage : UserControl
{
    public RulesPage()
    {
        InitializeComponent();
        DataContext = new RulesViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
