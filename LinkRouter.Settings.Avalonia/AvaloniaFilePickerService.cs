using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using LinkRouter.Settings.Services.Interfaces;

namespace LinkRouter.Settings.Avalonia;

internal sealed class AvaloniaFilePickerService : IFilePickerService
{
    private readonly Window _window;

    public AvaloniaFilePickerService(Window window)
    {
        _window = window;
    }

    public async Task<string?> PickOpenFileAsync(string title, string filter)
    {
        var options = new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = BuildFileTypes(filter)
        };

        var result = await _window.StorageProvider.OpenFilePickerAsync(options).ConfigureAwait(false);
        return result.FirstOrDefault()?.TryGetLocalPath();
    }

    public async Task<string?> PickSaveFileAsync(string title, string suggestedFileName, string filter)
    {
        var options = new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = suggestedFileName,
            FileTypeChoices = BuildFileTypes(filter)
        };

        var result = await _window.StorageProvider.SaveFilePickerAsync(options).ConfigureAwait(false);
        return result?.TryGetLocalPath();
    }

    public async Task<string?> PickFolderAsync(string title)
    {
        var options = new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        };

        var result = await _window.StorageProvider.OpenFolderPickerAsync(options).ConfigureAwait(false);
        return result.FirstOrDefault()?.TryGetLocalPath();
    }

    private static IReadOnlyList<FilePickerFileType> BuildFileTypes(string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return Array.Empty<FilePickerFileType>();
        }

        var parts = filter.Split('|');
        if (parts.Length == 2)
        {
            return new[]
            {
                new FilePickerFileType(parts[0])
                {
                    Patterns = new[] { parts[1] }
                }
            };
        }

        return Array.Empty<FilePickerFileType>();
    }
}
