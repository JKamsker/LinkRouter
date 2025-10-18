using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LinkRouter.Settings.Avalonia.Platform;
using LinkRouter.Settings.Core.Infrastructure;
using LinkRouter.Settings.Core.Services;
using LinkRouter.Settings.Core.ViewModels;

namespace LinkRouter.Settings.Avalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            ConfigureServices(desktop);
            TryLoadConfiguration();
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IClassicDesktopStyleApplicationLifetime desktop)
    {
        SettingsServiceLocator.ConfigService = new ConfigService();
        SettingsServiceLocator.RuleTestService = new RuleTestService();
        SettingsServiceLocator.BrowserDetectionService = new BrowserDetectionService();
        SettingsServiceLocator.ConfigurationState = new ConfigurationState();
        SettingsServiceLocator.Launcher = new AvaloniaLauncherService();
        SettingsServiceLocator.FilePicker = new AvaloniaFilePickerService(() => desktop.MainWindow);
        SettingsServiceLocator.MessageDialog = new AvaloniaMessageDialogService(() => desktop.MainWindow);
        SettingsServiceLocator.Clipboard = new AvaloniaClipboardService(() => desktop.MainWindow);
    }

    private static void TryLoadConfiguration()
    {
        try
        {
            var document = SettingsServiceLocator.ConfigService.LoadAsync().GetAwaiter().GetResult();
            SettingsServiceLocator.ConfigurationState.Load(document);
        }
        catch
        {
            // Individual view models will surface errors via bound properties.
        }
    }
}