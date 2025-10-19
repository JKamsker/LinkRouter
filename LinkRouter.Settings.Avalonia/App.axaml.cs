using System;
using Avalonia;
using Avalonia.Controls;
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
    private ServiceProvider? _serviceProvider;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection();

            services.AddSingleton<Func<Window?>>(_ => () => desktop.MainWindow);
            services.AddSingleton<ConfigService>();
            services.AddSingleton<RuleTestService>();
            services.AddSingleton<BrowserDetectionService>();
            services.AddSingleton<ConfigurationState>();
            services.AddSingleton<IClipboardService>(sp => new AvaloniaClipboardService(sp.GetRequiredService<Func<Window?>>()));
            services.AddSingleton<IShellService, AvaloniaShellService>();
            services.AddSingleton<IFilePickerService>(sp => new AvaloniaFilePickerService(sp.GetRequiredService<Func<Window?>>()));
            services.AddSingleton<IMessageDialogService>(sp => new AvaloniaMessageDialogService(sp.GetRequiredService<Func<Window?>>()));
            services.AddSingleton<IDialogService>(sp => new AvaloniaDialogService(sp.GetRequiredService<Func<Window?>>()));

            services.AddSingleton<GeneralViewModel>();
            services.AddSingleton<RulesViewModel>();
            services.AddSingleton<ProfilesViewModel>();
            services.AddSingleton<ImportExportViewModel>();
            services.AddSingleton<AdvancedViewModel>();
            services.AddSingleton<AboutViewModel>();
            services.AddSingleton<SettingsShellViewModel>();

            services.AddTransient<MainWindow>();

            _serviceProvider = services.BuildServiceProvider();

            var configService = _serviceProvider.GetRequiredService<ConfigService>();
            var state = _serviceProvider.GetRequiredService<ConfigurationState>();

            try
            {
                var document = configService.LoadAsync().GetAwaiter().GetResult();
                state.Load(document);
            }
            catch
            {
                // Allow UI to surface issues lazily.
            }

            desktop.MainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            desktop.Exit += OnDesktopExit;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnDesktopExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        _serviceProvider?.Dispose();
    }
}