using LinkRouter.Settings.Services;
using LinkRouter.Settings.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LinkRouter.Settings.Views;

public sealed partial class ProfilesPage : Page
{
    public ProfilesViewModel ViewModel => (ProfilesViewModel)DataContext;

    public ProfilesPage()
    {
        InitializeComponent();
    }

    private void UseDetectedBrowser_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: BrowserInfo browser })
        {
            var command = ViewModel.UseDetectedBrowserCommand;
            if (command.CanExecute(browser))
            {
                command.Execute(browser);
            }
        }
    }

    private void UseChromiumProfile_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: string profile })
        {
            var command = ViewModel.UseChromiumProfileCommand;
            if (command.CanExecute(profile))
            {
                command.Execute(profile);
            }
        }
    }

    private void UseFirefoxProfile_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: FirefoxProfileInfo profile })
        {
            var command = ViewModel.UseFirefoxProfileCommand;
            if (command.CanExecute(profile))
            {
                command.Execute(profile);
            }
        }
    }
}
