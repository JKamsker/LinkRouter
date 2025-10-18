using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Headless;
using Avalonia.Skia;
using LinkRouter.Settings.Avalonia;
using Xunit;

namespace LinkRouter.Settings.Avalonia.Tests.Startup;

public class ApplicationStartupTests
{
    [Fact]
    public void Application_CanInitializeMainWindow()
    {
        var lifetime = new ClassicDesktopStyleApplicationLifetime();
        var builder = AppBuilder.Configure<App>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
            .WithInterFont()
            .LogToTrace();

        builder.SetupWithLifetime(lifetime);

        Assert.NotNull(lifetime.MainWindow);
    }
}
