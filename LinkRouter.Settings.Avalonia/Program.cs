using System;
using Avalonia;
using Avalonia.Media;
using LinkRouter.Settings.Avalonia.Services;
using LinkRouter.Settings.Services;
using LinkRouter.Settings.Services.Abstractions;
using LinkRouter.Settings.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LinkRouter.Settings.Avalonia;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        using var host = CreateHostBuilder(args).Build();
        App.InitializeHost(host);
        host.Start();

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .With(FontConfiguration.Create())
            .LogToTrace();

    private static IHostBuilder CreateHostBuilder(string[] args)
        => Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddSingleton<ConfigService>();
                services.AddSingleton<RuleTestService>();
                services.AddSingleton<BrowserDetectionService>();
                services.AddSingleton<ConfigurationState>();

                services.AddSingleton<IClipboardService>(_ => new AvaloniaClipboardService(App.TryGetActiveWindow));
                services.AddSingleton<IShellService, AvaloniaShellService>();
                services.AddSingleton<IFilePickerService>(_ => new AvaloniaFilePickerService(App.TryGetActiveWindow));
                services.AddSingleton<IMessageDialogService>(_ => new AvaloniaMessageDialogService(App.TryGetActiveWindow));
                services.AddSingleton<IDialogService>(_ => new AvaloniaDialogService(App.TryGetActiveWindow));

                services.AddSingleton<GeneralViewModel>();
                services.AddSingleton<RulesViewModel>();
                services.AddSingleton<ProfilesViewModel>();
                services.AddSingleton<ImportExportViewModel>();
                services.AddSingleton<AdvancedViewModel>();
                services.AddSingleton<AboutViewModel>();
                services.AddSingleton<SettingsShellViewModel>();

                services.AddSingleton<MainWindow>();
            });
}