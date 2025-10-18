using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using LinkRouter.Settings.Services.Interfaces;

namespace LinkRouter.Settings.Avalonia;

internal sealed class AvaloniaClipboardService : IClipboardService
{
    public void SetText(string text)
    {
        var clipboard = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow?.Clipboard;

        if (clipboard is null)
        {
            return;
        }

        _ = clipboard.SetTextAsync(text);
    }
}
