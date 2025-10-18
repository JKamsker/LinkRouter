using Avalonia;
using Avalonia.Headless;
using LinkRouter.Settings.Avalonia;

namespace LinkRouter.Settings.Avalonia.Tests;

internal static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
            .LogToTrace();
    }
}
