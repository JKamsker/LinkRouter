using LinkRouter.Settings.Core.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace LinkRouter.Settings.Views;

public sealed partial class AdvancedPage : Page
{
    public AdvancedViewModel ViewModel => (AdvancedViewModel)DataContext;

    public AdvancedPage()
    {
        InitializeComponent();
    }
}
