using LinkRouter.Settings.Core.Infrastructure;
using LinkRouter.Settings.Core.Services;
using LinkRouter.Settings.Platform;
using Microsoft.UI.Xaml;

namespace LinkRouter.Settings;

public partial class App : Application
{
    private Window? _window;

    public App()
    {
        InitializeComponent();
        UnhandledException += OnUnhandledException;
        SettingsServiceLocator.ConfigService = new ConfigService();
        SettingsServiceLocator.RuleTestService = new RuleTestService();
        SettingsServiceLocator.BrowserDetectionService = new BrowserDetectionService();
        SettingsServiceLocator.ConfigurationState = new ConfigurationState();
        SettingsServiceLocator.Clipboard = new WinUIClipboardService();
        SettingsServiceLocator.Launcher = new WinUILauncherService();
        SettingsServiceLocator.FilePicker = new WinUIFilePickerService();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            var document = SettingsServiceLocator.ConfigService.LoadAsync().GetAwaiter().GetResult();
            SettingsServiceLocator.ConfigurationState.Load(document);
        }
        catch
        {
            // Swallow config load errors for now; individual pages will show errors through bindings.
        }

        _window = new MainWindow();
        SettingsServiceLocator.MessageDialog = new WinUIMessageDialogService(() => new Microsoft.UI.Xaml.Controls.ContentDialog
        {
            XamlRoot = _window.Content.XamlRoot
        });
        _window.Activate();
    }

    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Add centralized logging if desired
        e.Handled = true;
    }
}
