using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using LinkRouter.Settings.Services.Abstractions;

namespace LinkRouter.Settings.Avalonia.Services;

internal sealed class AvaloniaDialogService : IDialogService
{
    private readonly Func<Window?> _getWindow;

    public AvaloniaDialogService(Func<Window?> getWindow)
    {
        _getWindow = getWindow;
    }

    public async Task<TResult?> ShowDialogAsync<TResult>(DialogRequest<TResult> request)
    {
        var dialog = new ContentDialog
        {
            Title = request.Title,
            PrimaryButtonText = request.PrimaryButtonText,
            SecondaryButtonText = request.SecondaryButtonText,
            CloseButtonText = request.CloseButtonText,
            DefaultButton = ContentDialogButton.Primary
        };

        if (request.ViewModel is not null)
        {
            var content = BuildContent(request.ViewModel);
            if (content is not null)
            {
                content.DataContext = request.ViewModel;
                dialog.Content = content;
            }
        }

        var owner = _getWindow();
        var result = owner is null
            ? await dialog.ShowAsync().ConfigureAwait(true)
            : await dialog.ShowAsync(owner).ConfigureAwait(true);

        var action = result switch
        {
            ContentDialogResult.Primary => DialogAction.Primary,
            ContentDialogResult.Secondary => DialogAction.Secondary,
            ContentDialogResult.None => DialogAction.Close,
            _ => DialogAction.None
        };

        return request.ResultSelector is null
            ? default
            : request.ResultSelector(action);
    }

    private static Control? BuildContent(object viewModel)
    {
        if (Application.Current?.DataTemplates is not { } templates)
        {
            return null;
        }

        foreach (var template in templates)
        {
            if (template.Match(viewModel) && template.Build(viewModel) is Control control)
            {
                return control;
            }
        }

        return null;
    }
}
