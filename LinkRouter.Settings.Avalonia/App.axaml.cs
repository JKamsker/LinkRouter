using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LinkRouter.Settings.Avalonia.Services;
using LinkRouter.Settings.Services;
using LinkRouter.Settings.Services.Abstractions;
using LinkRouter.Settings.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LinkRouter.Settings.Avalonia;

public partial class App : Application
{
    private IServiceProvider? _services;

    public IServiceProvider Services => _services ?? throw new InvalidOperationException("Services are not initialized.");

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _services = ConfigureServices(desktop);

            var configService = _services.GetRequiredService<ConfigService>();
            var state = _services.GetRequiredService<ConfigurationState>();

            try
            {
                var document = configService.LoadAsync().GetAwaiter().GetResult();
                state.Load(document);
            }
            catch
            {
                // Allow UI to surface issues lazily.
            }

            desktop.MainWindow = new MainWindow
            {
                DataContext = _services.GetRequiredService<SettingsShellViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private IServiceProvider ConfigureServices(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var services = new ServiceCollection();

        services.AddSingleton<ConfigService>();
        services.AddSingleton<RuleTestService>();
        services.AddSingleton<BrowserDetectionService>();
        services.AddSingleton<ConfigurationState>();

        services.AddSingleton<IClipboardService>(_ => new AvaloniaClipboardService(() => desktop.MainWindow));
        services.AddSingleton<IShellService>(_ => new AvaloniaShellService());
        services.AddSingleton<IFilePickerService>(_ => new AvaloniaFilePickerService(() => desktop.MainWindow));
        services.AddSingleton<IMessageDialogService>(_ => new AvaloniaMessageDialogService(() => desktop.MainWindow));
        services.AddSingleton<IDialogService>(_ => new AvaloniaDialogService(() => desktop.MainWindow));

        services.AddSingleton<GeneralViewModel>();
        services.AddSingleton<RulesViewModel>();
        services.AddSingleton<ProfilesViewModel>();
        services.AddSingleton<ImportExportViewModel>();
        services.AddSingleton<AdvancedViewModel>();
        services.AddSingleton<AboutViewModel>();
        services.AddSingleton<SettingsShellViewModel>();

        return services.BuildServiceProvider();
    }
}
