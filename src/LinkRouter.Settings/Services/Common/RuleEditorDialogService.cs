using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using LinkRouter.Settings.Views;
using LinkRouter.Settings.Services.Abstractions;
using LinkRouter.Settings.ViewModels;

namespace LinkRouter.Settings.Services.Common;

public sealed class RuleEditorDialogService : IRuleEditorDialogService
{
    private readonly Func<Window?> _windowAccessor;

    public RuleEditorDialogService(Func<Window?> windowAccessor)
    {
        _windowAccessor = windowAccessor;
    }

    public async Task<bool> EditRuleAsync(RuleEditorDialogViewModel viewModel)
    {
        var dialog = new RuleEditorDialog
        {
            DataContext = viewModel
        };

        var owner = _windowAccessor();
        var result = owner is null
            ? await dialog.ShowAsync().ConfigureAwait(false)
            : await dialog.ShowAsync(owner).ConfigureAwait(false);

        return result == ContentDialogResult.Primary;
    }
}
