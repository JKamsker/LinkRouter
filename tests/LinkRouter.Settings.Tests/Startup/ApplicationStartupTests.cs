using Avalonia.Headless.XUnit;
using LinkRouter.Settings;
using System.Threading.Tasks;
using Xunit;

namespace LinkRouter.Settings.Tests.Startup;

public class ApplicationStartupTests
{
    [AvaloniaFact(Timeout = 30_000)]
    public Task MainWindow_CanBeConstructed()
    {
        var window = new MainWindow();
        Assert.NotNull(window);
        return Task.CompletedTask;
    }
}
