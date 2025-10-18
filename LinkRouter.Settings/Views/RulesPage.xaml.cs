using LinkRouter.Settings.Core.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace LinkRouter.Settings.Views;

public sealed partial class RulesPage : Page
{
    public RulesViewModel ViewModel => (RulesViewModel)DataContext;

    public RulesPage()
    {
        InitializeComponent();
    }
}
