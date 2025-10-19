using Avalonia;
using Avalonia.Headless;
using Avalonia.Skia;

namespace LinkRouter.Settings.Avalonia.Tests;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions
            {
                UseHeadlessDrawing = true
            })
            .WithInterFont();
}
