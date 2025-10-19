using LinkRouter.Settings.Avalonia;
using LinkRouter.Settings.ViewModels;
using Xunit;

namespace LinkRouter.Settings.Avalonia.Tests.Startup;

public class ApplicationStartupTests
{
    [Fact]
    public void MainWindow_HasExpectedConstructor()
    {
        var constructor = typeof(MainWindow).GetConstructor(new[] { typeof(SettingsShellViewModel) });
        Assert.NotNull(constructor);
    }
}
