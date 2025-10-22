using Avalonia;
using Avalonia.Headless;
using Avalonia.Skia;
using LinkRouter.Settings;

namespace LinkRouter.Settings.Tests;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions
            {
                UseHeadlessDrawing = true
            })
            .WithInterFont()
            .With(FontConfiguration.Create());
}
