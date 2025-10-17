using System.Threading.Tasks;
using LinkRouter.Settings.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LinkRouter.Settings.Views;

public sealed partial class OverviewPage : Page
{
    public OverviewViewModel ViewModel => (OverviewViewModel)DataContext;

    public OverviewPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        await ViewModel.InitializeAsync();
    }
}
