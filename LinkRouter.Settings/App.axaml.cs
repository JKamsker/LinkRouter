using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LinkRouter.Settings.Services;
using LinkRouter.Settings.Services.Abstractions;
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

            desktop.MainWindow = Services.GetRequiredService<MainWindow>();
            Services.GetRequiredService<SettingsTrayIconService>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    internal static void ConfigureServices(IServiceCollection services, IClassicDesktopStyleApplicationLifetime desktop)
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
