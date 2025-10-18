using System;
using Avalonia.Controls;
using LinkRouter.Settings.Core.Infrastructure;

namespace LinkRouter.Settings.Avalonia.Platform;

internal sealed class AvaloniaClipboardService : IClipboardService
{
    private readonly Func<Window?> _windowProvider;

    public AvaloniaClipboardService(Func<Window?> windowProvider)
    {
        _windowProvider = windowProvider;
    }

    public void SetText(string text)
    {
        var window = _windowProvider();
        window?.Clipboard?.SetTextAsync(text);
    }
}
