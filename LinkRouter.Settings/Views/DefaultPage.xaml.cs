using LinkRouter.Settings.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace LinkRouter.Settings.Views;

public sealed partial class DefaultPage : Page
{
    public DefaultViewModel ViewModel => (DefaultViewModel)DataContext;

    public DefaultPage()
    {
        InitializeComponent();
    }
}
