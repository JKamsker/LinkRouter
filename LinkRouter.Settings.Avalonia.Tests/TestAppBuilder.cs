using Avalonia;
using Avalonia.Headless;
using Avalonia.Skia;
using LinkRouter.Settings.Avalonia;

namespace LinkRouter.Settings.Avalonia.Tests;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions
            {
                UseHeadlessDrawing = false
            })
            .WithInterFont()
            .LogToTrace();
}
