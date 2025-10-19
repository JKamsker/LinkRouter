using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using LinkRouter.Settings.Avalonia.ViewModels;
using LinkRouter.Settings.Services.Abstractions;
using LinkRouter.Settings.ViewModels;

namespace LinkRouter.Settings.Avalonia.Services;

internal sealed class RuleEditorDialogService : IRuleEditorDialogService
{
    private readonly Func<Window?> _getWindow;

    public RuleEditorDialogService(Func<Window?> getWindow)
    {
        _getWindow = getWindow;
    }

    public async Task ShowEditorAsync(
        RuleEditorViewModel rule,
        IReadOnlyList<string> matchTypes,
        IReadOnlyList<string> profileOptions)
    {
        var dialog = new Views.RuleEditorDialog
        {
            DataContext = new RuleEditorDialogContext(rule, matchTypes, profileOptions)
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
