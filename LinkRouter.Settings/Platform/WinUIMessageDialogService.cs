using System;
using System.Threading.Tasks;
using LinkRouter.Settings.Core.Infrastructure;
using Microsoft.UI.Xaml.Controls;

namespace LinkRouter.Settings.Platform;

internal sealed class WinUIMessageDialogService : IMessageDialogService
{
    private readonly Func<ContentDialog> _dialogFactory;

    public WinUIMessageDialogService(Func<ContentDialog> dialogFactory)
    {
        _dialogFactory = dialogFactory;
    }

    public async Task ShowMessageAsync(string title, string message)
    {
        var dialog = _dialogFactory();
        dialog.Title = title;
        dialog.Content = message;
        dialog.PrimaryButtonText = "OK";
        await dialog.ShowAsync();
    }

    public async Task<bool> ShowConfirmationAsync(string title, string message, string confirmButtonText = "OK", string cancelButtonText = "Cancel")
    {
        var dialog = _dialogFactory();
        dialog.Title = title;
        dialog.Content = message;
        dialog.PrimaryButtonText = confirmButtonText;
        dialog.CloseButtonText = cancelButtonText;
        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }
}
