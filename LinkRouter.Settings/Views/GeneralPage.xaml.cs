using LinkRouter.Settings.Core.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace LinkRouter.Settings.Views;

public sealed partial class GeneralPage : Page
{
    public GeneralViewModel ViewModel => (GeneralViewModel)DataContext;

    public GeneralPage()
    {
        InitializeComponent();
    }
}
