using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LinkRouter.Settings.Services;
using LinkRouter.Settings.Services.Abstractions;
using LinkRouter.Settings.Services.Common;
using LinkRouter.Settings.Services.Windows;
using LinkRouter.Settings.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LinkRouter.Settings;

public partial class App : Application
{
    public IServiceProvider Services { get; private set; } = default!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection();
            ConfigureServices(services, desktop);
            Services = services.BuildServiceProvider();

            Services.GetRequiredService<AppInitializationService>()
                .InitializeAsync()
                .GetAwaiter()
                .GetResult();

            var startHidden = desktop.Args?.Any(arg =>
                string.Equals(arg, "--minimized", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(arg, "--background", StringComparison.OrdinalIgnoreCase)) == true;

            var mainWindow = Services.GetRequiredService<MainWindow>();

            // Only set as MainWindow if not starting hidden
            // This prevents the window from being shown when using --background
            if (!startHidden)
            {
                desktop.MainWindow = mainWindow;
            }

            Services.GetRequiredService<SettingsTrayIconService>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public static void ConfigureServices(IServiceCollection services, IClassicDesktopStyleApplicationLifetime desktop)
    {
        services.AddSingleton<ConfigService>();
        services.AddSingleton<RuleTestService>();
        services.AddSingleton<BrowserDetectionService>();
        services.AddSingleton<ConfigurationState>();

        services.AddSingleton<IClassicDesktopStyleApplicationLifetime>(desktop);

        services.AddSingleton<IClipboardService>(_ => new AvaloniaClipboardService(() => desktop.MainWindow));
        services.AddSingleton<IShellService, AvaloniaShellService>();
        services.AddSingleton<IFilePickerService>(_ => new AvaloniaFilePickerService(() => desktop.MainWindow));
        services.AddSingleton<IMessageDialogService>(_ => new AvaloniaMessageDialogService(() => desktop.MainWindow));
        services.AddSingleton<IRuleEditorDialogService>(_ => new RuleEditorDialogService(() => desktop.MainWindow));

        // Platform-specific services
        if (OperatingSystem.IsWindows())
        {
            services.AddSingleton<IRouterPathResolver, WindowsRouterPathResolver>();
            services.AddSingleton<IAutostartService, WindowsAutostartService>();
            services.AddSingleton<IDefaultAppRegistrar, WindowsDefaultAppRegistrar>();
        }
        else
        {
            services.AddSingleton<IRouterPathResolver, RouterPathResolver>(); // Fallback
            services.AddSingleton<IAutostartService, NoOpAutostartService>();
        }

        services.AddSingleton<AppInitializationService>();
        services.AddSingleton<SettingsTrayIconService>();

        services.AddSingleton<GeneralViewModel>();
        services.AddSingleton<RulesViewModel>();
        services.AddSingleton<ProfilesViewModel>();
        services.AddSingleton<ImportExportViewModel>();
        services.AddSingleton<AdvancedViewModel>();
        services.AddSingleton<AboutViewModel>();
        services.AddSingleton<MainWindowViewModel>();

        services.AddSingleton<MainWindow>(sp =>
        {
            var window = new MainWindow
            {
                DataContext = sp.GetRequiredService<MainWindowViewModel>()
            };

            return window;
        });
    }
}
