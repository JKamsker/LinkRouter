using Avalonia.Headless.XUnit;
using LinkRouter.Settings.Avalonia;
using LinkRouter.Settings.Avalonia.ViewModels;
using System.Threading.Tasks;
using Xunit;

namespace LinkRouter.Settings.Avalonia.Tests.Startup;

public class ApplicationStartupTests
{
    [AvaloniaFact(Timeout = 30_000)]
    public Task MainWindow_CanBeConstructed()
    {
        var viewModel = new MainWindowViewModel(new[]
        {
            new NavigationItemViewModel("test", "Test", new object())
        });

        var window = new MainWindow(viewModel);
        Assert.NotNull(window);
        return Task.CompletedTask;
    }
}
