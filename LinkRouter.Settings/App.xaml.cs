using LinkRouter.Settings.Services;
using Microsoft.UI.Xaml;

namespace LinkRouter.Settings;

public partial class App : Application
{
    private Window? _window;

    public App()
    {
        InitializeComponent();
        UnhandledException += OnUnhandledException;
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            var document = AppServices.ConfigService.LoadAsync().GetAwaiter().GetResult();
            AppServices.ConfigurationState.Load(document);
        }
        catch
        {
            // Swallow config load errors for now; individual pages will show errors through bindings.
        }

        _window = new MainWindow();
        _window.Activate();
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        // TODO: Add centralized logging if desired
        e.Handled = true;
    }
}
