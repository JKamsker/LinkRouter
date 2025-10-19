using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using LinkRouter.Settings.Avalonia.Views;
using LinkRouter.Settings.Services.Abstractions;
using LinkRouter.Settings.ViewModels;

namespace LinkRouter.Settings.Avalonia.Services;

internal sealed class AvaloniaDialogService : IDialogService
{
    private readonly Func<Window?> _getWindow;

    public AvaloniaDialogService(Func<Window?> getWindow)
    {
        _getWindow = getWindow;
    }

    public async Task<bool> ShowRuleEditorAsync(
        RuleEditorViewModel editor,
        IReadOnlyList<string> matchTypes,
        IReadOnlyList<string> profileOptions)
    {
        var dialog = new RuleEditorDialog
        {
            DataContext = new RuleEditorDialogContext(editor, matchTypes, profileOptions)
        };

        var owner = _getWindow();
        var result = owner is Window window
            ? await dialog.ShowAsync(window).ConfigureAwait(false)
            : await dialog.ShowAsync().ConfigureAwait(false);

        return result == ContentDialogResult.Primary;
    }
}
