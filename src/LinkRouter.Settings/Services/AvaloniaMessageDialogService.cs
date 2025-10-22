using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using LinkRouter.Settings.Services.Abstractions;

namespace LinkRouter.Settings.Services;

internal sealed class AvaloniaMessageDialogService : IMessageDialogService
{
    private readonly Func<Window?> _getWindow;

    public AvaloniaMessageDialogService(Func<Window?> getWindow)
    {
        _getWindow = getWindow;
    }

    public async Task<MessageDialogResult> ShowMessageAsync(string title, string message, MessageDialogOptions options)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = options.PrimaryButtonText,
            SecondaryButtonText = options.SecondaryButtonText,
            CloseButtonText = options.CloseButtonText ?? "Close"
        };

        var window = _getWindow();
        var result = window is null
            ? await dialog.ShowAsync()
            : await dialog.ShowAsync(window);

        return result switch
        {
            ContentDialogResult.Primary => MessageDialogResult.Primary,
            ContentDialogResult.Secondary => MessageDialogResult.Secondary,
            ContentDialogResult.None => MessageDialogResult.Close,
            _ => MessageDialogResult.None
        };
    }
}
