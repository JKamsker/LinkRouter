using System.Linq;
using System.Threading.Tasks;
using Avalonia.Headless.XUnit;
using FluentAvalonia.UI.Controls;
using LinkRouter.Settings.Avalonia;
using Xunit;

namespace LinkRouter.Settings.Avalonia.Tests.Startup;

public class ApplicationStartupTests
{
    [AvaloniaFact(Timeout = 30_000)]
    public Task MainWindow_LoadsFirstPage()
    {
        var window = new MainWindow();
        var navView = window.NavView;
        var firstItem = navView.MenuItems.OfType<NavigationViewItem>().First();
        navView.SelectedItem = firstItem;

        Assert.NotNull(window.ContentHost.Content);

        return Task.CompletedTask;
    }
}
