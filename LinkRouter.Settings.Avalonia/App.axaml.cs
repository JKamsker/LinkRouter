using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LinkRouter.Settings.Services;
using LinkRouter.Settings.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LinkRouter.Settings.Avalonia;

public partial class App : Application
{
    private static IHost? s_host;

    internal static void InitializeHost(IHost host)
    {
        s_host = host;
    }

    internal static Window? TryGetActiveWindow()
    {
        if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }

        return null;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (s_host is null)
        {
            throw new InvalidOperationException("Host has not been initialised.");
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = s_host.Services;
            var configService = services.GetRequiredService<ConfigService>();
            var state = services.GetRequiredService<ConfigurationState>();

            try
            {
                var document = configService.LoadAsync().GetAwaiter().GetResult();
                state.Load(document);
            }
            catch
            {
                // Allow UI to surface issues lazily.
            }

            desktop.MainWindow = services.GetRequiredService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }
}