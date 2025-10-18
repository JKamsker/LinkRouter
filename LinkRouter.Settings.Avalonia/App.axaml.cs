using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LinkRouter.Settings.Services;

namespace LinkRouter.Settings.Avalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        AppServices.ClipboardService = new AvaloniaClipboardService();

        try
        {
            var document = AppServices.ConfigService.LoadAsync().GetAwaiter().GetResult();
            AppServices.ConfigurationState.Load(document);
        }
        catch
        {
            // Individual view models surface errors through bindings.
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
            AppServices.FilePickerService = new AvaloniaFilePickerService(desktop.MainWindow);
        }

        base.OnFrameworkInitializationCompleted();
    }
}