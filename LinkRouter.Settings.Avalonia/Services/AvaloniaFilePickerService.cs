using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using LinkRouter.Settings.Services.Abstractions;

namespace LinkRouter.Settings.Avalonia.Services;

internal sealed class AvaloniaFilePickerService : IFilePickerService
{
    private readonly Func<Window?> _getWindow;

    public AvaloniaFilePickerService(Func<Window?> getWindow)
    {
        _getWindow = getWindow;
    }

    public async Task<string?> PickOpenFileAsync(FilePickerOptions options, CancellationToken cancellationToken = default)
    {
        var dialog = new OpenFileDialog
        {
            AllowMultiple = false,
            Title = options.Title
        };

        dialog.Filters = BuildFilters(options.FileTypes);
        var window = _getWindow();
        var result = window is null
            ? await dialog.ShowAsync(null)
            : await dialog.ShowAsync(window);
        return result?.FirstOrDefault();
    }

    public async Task<string?> PickSaveFileAsync(FilePickerOptions options, CancellationToken cancellationToken = default)
    {
        var dialog = new SaveFileDialog
        {
            Title = options.Title,
            InitialFileName = options.SuggestedFileName
        };

        dialog.Filters = BuildFilters(options.FileTypes);
        var window = _getWindow();
        return window is null
            ? await dialog.ShowAsync(null)
            : await dialog.ShowAsync(window);
    }

    private static List<FileDialogFilter>? BuildFilters(IReadOnlyList<FilePickerFileType> types)
    {
        if (types.Count == 0)
        {
            return null;
        }

        return types.Select(t => new FileDialogFilter
        {
            Name = t.DisplayName,
            Extensions = t.Patterns.Select(p => p.TrimStart('.')).ToList()
        }).ToList();
    }
}
