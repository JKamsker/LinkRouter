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
    public IServiceProvider Services { get; private set; } = default!;

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

            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private IServiceProvider ConfigureServices(IClassicDesktopStyleApplicationLifetime desktop)
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
        services.AddSingleton<IRuleEditorDialogService>(sp => new RuleEditorDialogService(sp.GetRequiredService<Func<Window?>>()));

        services.AddSingleton<GeneralViewModel>();
        services.AddSingleton<RulesViewModel>();
        services.AddSingleton<ProfilesViewModel>();
        services.AddSingleton<ImportExportViewModel>();
        services.AddSingleton<AdvancedViewModel>();
        services.AddSingleton<AboutViewModel>();

        services.AddSingleton<NavigationItemViewModel>(sp => new NavigationItemViewModel("overview", "Overview", sp.GetRequiredService<GeneralViewModel>()));
        services.AddSingleton<NavigationItemViewModel>(sp => new NavigationItemViewModel("rules", "Rules", sp.GetRequiredService<RulesViewModel>()));
        services.AddSingleton<NavigationItemViewModel>(sp => new NavigationItemViewModel("profiles", "Browsers & Profiles", sp.GetRequiredService<ProfilesViewModel>()));
        services.AddSingleton<NavigationItemViewModel>(sp => new NavigationItemViewModel("import", "Import / Export", sp.GetRequiredService<ImportExportViewModel>()));
        services.AddSingleton<NavigationItemViewModel>(sp => new NavigationItemViewModel("advanced", "Advanced", sp.GetRequiredService<AdvancedViewModel>()));
        services.AddSingleton<NavigationItemViewModel>(sp => new NavigationItemViewModel("about", "About", sp.GetRequiredService<AboutViewModel>()));

        services.AddSingleton<MainWindowViewModel>();

        return services.BuildServiceProvider();
    }
}
