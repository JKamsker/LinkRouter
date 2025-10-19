using System.Threading.Tasks;
using Xunit;
using LinkRouter.Settings.Avalonia.Tests;

namespace LinkRouter.Settings.Avalonia.Tests.Startup;

public class ApplicationStartupTests
{
    [AvaloniaFact]
    public Task Application_CanInitializeMainWindow()
    {
        var lifetime = TestAppHost.EnsureLifetime();
        Assert.NotNull(lifetime.MainWindow);
        return Task.CompletedTask;
    }
}
