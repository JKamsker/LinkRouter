using LinkRouter.Settings.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace LinkRouter.Settings.Views;

public sealed partial class OverviewPage : Page
{
    public OverviewViewModel ViewModel => (OverviewViewModel)DataContext;

    public OverviewPage()
    {
        InitializeComponent();
    }
}
