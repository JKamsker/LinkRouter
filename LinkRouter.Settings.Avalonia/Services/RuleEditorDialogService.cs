using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using LinkRouter.Settings.Avalonia.Views;
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

    public async Task<bool> EditRuleAsync(
        RuleEditorViewModel rule,
        IReadOnlyList<string> matchTypes,
        IReadOnlyList<string> profileOptions)
    {
        var workingCopy = rule.Clone();
        using var dialogViewModel = new RuleEditorDialogViewModel(workingCopy, matchTypes, profileOptions);
        var dialog = new RuleEditorDialog
        {
            DataContext = dialogViewModel
        };

        var window = _getWindow();
        var result = window is null
            ? await dialog.ShowAsync()
            : await dialog.ShowAsync(window);

        if (result == ContentDialogResult.Primary)
        {
            rule.ApplyFrom(workingCopy);
            return true;
        }

        return false;
    }
}
