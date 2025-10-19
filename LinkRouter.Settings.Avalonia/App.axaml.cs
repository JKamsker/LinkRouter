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
    private IServiceProvider? _serviceProvider;

    public IServiceProvider Services => _serviceProvider ?? throw new InvalidOperationException("Application services have not been initialized.");

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = ConfigureServices(desktop);
            _serviceProvider = services.BuildServiceProvider();

            InitializeConfiguration(_serviceProvider);

            var viewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow(viewModel);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static IServiceCollection ConfigureServices(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var services = new ServiceCollection();

        services.AddSingleton<Func<Window?>>(_ => () => desktop.MainWindow);
        services.AddSingleton<IClipboardService, AvaloniaClipboardService>();
        services.AddSingleton<IShellService, AvaloniaShellService>();
        services.AddSingleton<IFilePickerService, AvaloniaFilePickerService>();
        services.AddSingleton<IMessageDialogService, AvaloniaMessageDialogService>();
        services.AddSingleton<IDialogService, DialogService>();

        services.AddSingleton<ConfigService>();
        services.AddSingleton<RuleTestService>();
        services.AddSingleton<BrowserDetectionService>();
        services.AddSingleton<ConfigurationState>();

        services.AddSingleton<GeneralViewModel>();
        services.AddSingleton<RulesViewModel>();
        services.AddSingleton<ProfilesViewModel>();
        services.AddSingleton<ImportExportViewModel>();
        services.AddSingleton<AdvancedViewModel>();
        services.AddSingleton<AboutViewModel>();
        services.AddSingleton<MainWindowViewModel>();

        return services;
    }

    private static void InitializeConfiguration(IServiceProvider provider)
    {
        var configService = provider.GetRequiredService<ConfigService>();
        var configurationState = provider.GetRequiredService<ConfigurationState>();

        try
        {
            var document = configService.LoadAsync().GetAwaiter().GetResult();
            configurationState.Load(document);
        }
        catch
        {
            // Allow the UI to surface issues as the user interacts with the app.
        }
    }
}
