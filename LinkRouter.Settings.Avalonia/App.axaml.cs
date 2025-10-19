using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LinkRouter.Settings.Avalonia.Services;
using LinkRouter.Settings.Avalonia.ViewModels;
using LinkRouter.Settings.Services;
using LinkRouter.Settings.Services.Abstractions;
using LinkRouter.Settings.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LinkRouter.Settings.Avalonia;

public partial class App : Application
{
    public IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Services = ConfigureServices(desktop);

            var configService = Services.GetRequiredService<ConfigService>();
            var state = Services.GetRequiredService<ConfigurationState>();

            try
            {
                var document = configService.LoadAsync().GetAwaiter().GetResult();
                state.Load(document);
            }
            catch
            {
                // Allow UI to surface issues lazily.
            }

            var mainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>()
            };

            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private IServiceProvider ConfigureServices(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var services = new ServiceCollection();

        services.AddSingleton(desktop);
        services.AddSingleton<ConfigService>();
        services.AddSingleton<RuleTestService>();
        services.AddSingleton<BrowserDetectionService>();
        services.AddSingleton<ConfigurationState>();

        services.AddSingleton<IClipboardService>(_ => new AvaloniaClipboardService(() => desktop.MainWindow));
        services.AddSingleton<IShellService, AvaloniaShellService>();
        services.AddSingleton<IFilePickerService>(_ => new AvaloniaFilePickerService(() => desktop.MainWindow));
        services.AddSingleton<IMessageDialogService>(_ => new AvaloniaMessageDialogService(() => desktop.MainWindow));
        services.AddSingleton<IRuleEditorDialogService>(_ => new RuleEditorDialogService(() => desktop.MainWindow));

        services.AddSingleton<GeneralViewModel>();
        services.AddSingleton<RulesViewModel>();
        services.AddSingleton<ProfilesViewModel>();
        services.AddSingleton<ImportExportViewModel>();
        services.AddSingleton<AdvancedViewModel>();
        services.AddSingleton<AboutViewModel>();
        services.AddSingleton<MainWindowViewModel>();

        return services.BuildServiceProvider();
    }
}
