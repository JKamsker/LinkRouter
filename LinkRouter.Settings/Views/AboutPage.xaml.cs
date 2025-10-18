using LinkRouter.Settings.Core.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace LinkRouter.Settings.Views;

public sealed partial class AboutPage : Page
{
    public AboutViewModel ViewModel => (AboutViewModel)DataContext;

    public AboutPage()
    {
        InitializeComponent();
    }
}
