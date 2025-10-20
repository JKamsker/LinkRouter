using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using LinkRouter.Settings.Services.Abstractions;

namespace LinkRouter.Settings.Services;

internal sealed class AvaloniaClipboardService : IClipboardService
{
    private readonly Func<Window?> _getWindow;

    public AvaloniaClipboardService(Func<Window?> getWindow)
    {
        _getWindow = getWindow;
    }

    public void SetText(string text)
    {
        var window = _getWindow();
        if (window?.Clipboard is { } clipboard)
        {
            clipboard.SetTextAsync(text).GetAwaiter().GetResult();
            return;
        }

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime && lifetime.MainWindow?.Clipboard is { } appClipboard)
        {
            appClipboard.SetTextAsync(text).GetAwaiter().GetResult();
        }
    }
}
