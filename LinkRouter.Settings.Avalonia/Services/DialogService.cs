using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using LinkRouter.Settings.Services.Abstractions;
using LinkRouter.Settings.ViewModels;
using LinkRouter.Settings.Avalonia.Views;

namespace LinkRouter.Settings.Avalonia.Services;

internal sealed class DialogService : IDialogService
{
    private readonly Func<Window?> _getWindow;

    public DialogService(Func<Window?> getWindow)
    {
        _getWindow = getWindow;
    }

    public async Task ShowRuleEditorAsync(RuleEditorDialogViewModel dialogViewModel)
    {
        var dialog = new RuleEditorDialog
        {
            DataContext = dialogViewModel
        };

        var owner = _getWindow();
        if (owner is not null)
        {
            await dialog.ShowAsync(owner);
        }
        else
        {
            await dialog.ShowAsync();
        }
    }
}
