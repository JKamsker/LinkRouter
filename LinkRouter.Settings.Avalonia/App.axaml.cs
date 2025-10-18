using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LinkRouter.Settings.Avalonia.Services;
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
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Func<Window?> getWindow = () => desktop.MainWindow;
            AppServices.ClipboardService = new AvaloniaClipboardService(getWindow);
            AppServices.ShellService = new AvaloniaShellService();
            AppServices.FilePickerService = new AvaloniaFilePickerService(getWindow);
            AppServices.MessageDialogService = new AvaloniaMessageDialogService(getWindow);

            try
            {
                var document = AppServices.ConfigService.LoadAsync().GetAwaiter().GetResult();
                AppServices.ConfigurationState.Load(document);
            }
            catch
            {
                // Allow UI to surface issues lazily.
            }

            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}