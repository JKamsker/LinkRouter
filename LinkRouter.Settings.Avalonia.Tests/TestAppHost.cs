using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Headless;
using Avalonia.Skia;
using LinkRouter.Settings.Avalonia;

namespace LinkRouter.Settings.Avalonia.Tests;

internal static class TestAppHost
{
    private static readonly object s_sync = new();
    private static ClassicDesktopStyleApplicationLifetime? s_lifetime;

    public static ClassicDesktopStyleApplicationLifetime EnsureLifetime()
    {
        lock (s_sync)
        {
            if (Application.Current?.ApplicationLifetime is ClassicDesktopStyleApplicationLifetime existingLifetime)
            {
                s_lifetime = existingLifetime;
                return existingLifetime;
            }

            if (s_lifetime is { } existing)
            {
                return existing;
            }

            var lifetime = new ClassicDesktopStyleApplicationLifetime();
            AppBuilder.Configure<App>()
                .UseSkia()
                .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
                .WithInterFont()
                .LogToTrace()
                .SetupWithLifetime(lifetime);

            s_lifetime = lifetime;
            return lifetime;
        }
    }
}
