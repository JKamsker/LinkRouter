using Xunit;
using LinkRouter.Settings.Avalonia.Tests;

namespace LinkRouter.Settings.Avalonia.Tests.Startup;

public class ApplicationStartupTests
{
    [Fact]
    public void Application_CanInitializeMainWindow()
    {
        var lifetime = TestAppHost.EnsureLifetime();
        Assert.NotNull(lifetime.MainWindow);
    }
}
