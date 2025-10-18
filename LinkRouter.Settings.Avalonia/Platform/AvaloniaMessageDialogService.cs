using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using LinkRouter.Settings.Core.Infrastructure;

namespace LinkRouter.Settings.Avalonia.Platform;

internal sealed class AvaloniaMessageDialogService : IMessageDialogService
{
    private readonly Func<Window?> _windowProvider;

    public AvaloniaMessageDialogService(Func<Window?> windowProvider)
    {
        _windowProvider = windowProvider;
    }

    public async Task ShowMessageAsync(string title, string message)
    {
        var host = _windowProvider();
        if (host is null)
        {
            return;
        }

        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = "OK"
        };

        await dialog.ShowAsync(host);
    }

    public async Task<bool> ShowConfirmationAsync(string title, string message, string confirmButtonText = "OK", string cancelButtonText = "Cancel")
    {
        var host = _windowProvider();
        if (host is null)
        {
            return false;
        }

        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = confirmButtonText,
            CloseButtonText = cancelButtonText
        };

        var result = await dialog.ShowAsync(host);
        return result == ContentDialogResult.Primary;
    }
}
