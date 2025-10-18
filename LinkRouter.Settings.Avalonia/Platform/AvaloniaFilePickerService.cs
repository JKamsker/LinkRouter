using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using LinkRouter.Settings.Core.Infrastructure;

namespace LinkRouter.Settings.Avalonia.Platform;

internal sealed class AvaloniaFilePickerService : IFilePickerService
{
    private readonly Func<Window?> _windowProvider;

    public AvaloniaFilePickerService(Func<Window?> windowProvider)
    {
        _windowProvider = windowProvider;
    }

    public async Task<string?> PickFileAsync(FilePickerOptions options)
    {
        var dialog = new OpenFileDialog
        {
            Title = options.Title,
            AllowMultiple = false
        };

        if (options.Filters is { Length: > 0 })
        {
            dialog.Filters = new List<FileDialogFilter>
            {
                new()
                {
                    Name = "Files",
                    Extensions = options.Filters.Select(f => f.TrimStart('.')).ToList()
                }
            };
        }

        var window = _windowProvider();
        if (window is null)
        {
            return null;
        }

        var result = await dialog.ShowAsync(window);
        return result?.FirstOrDefault();
    }

    public async Task<string?> PickFolderAsync(string? initialDirectory = null)
    {
        var window = _windowProvider();
        if (window is null)
        {
            return null;
        }

        var dialog = new OpenFolderDialog
        {
            Directory = initialDirectory
        };

        return await dialog.ShowAsync(window);
    }
}
