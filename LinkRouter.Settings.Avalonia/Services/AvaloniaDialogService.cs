using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
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

    public async Task<DialogResult> ShowRuleEditorAsync(
        RuleEditorViewModel rule,
        IReadOnlyList<string> matchTypes,
        IReadOnlyList<string> profileOptions,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return DialogResult.None;
        }

        var dialog = new RuleEditorDialog
        {
            DataContext = new RuleEditorDialogViewModel(rule, matchTypes, profileOptions)
        };

        var owner = _getWindow();
        if (owner is null && Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            owner = lifetime.MainWindow;
        }

        var result = owner is not null
            ? await dialog.ShowAsync(owner)
            : await dialog.ShowAsync();

        return result switch
        {
            ContentDialogResult.Primary => DialogResult.Primary,
            ContentDialogResult.Secondary => DialogResult.Secondary,
            ContentDialogResult.None => DialogResult.None,
            _ => DialogResult.Close
        };
    }
}
