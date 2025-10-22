using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using LinkRouter.Settings.Services.Abstractions;

namespace LinkRouter.Settings.Services;

internal sealed class AvaloniaFilePickerService : IFilePickerService
{
    private readonly Func<Window?> _getWindow;

    public AvaloniaFilePickerService(Func<Window?> getWindow)
    {
        _getWindow = getWindow;
    }

    public async Task<string?> PickOpenFileAsync(FilePickerOptions options, CancellationToken cancellationToken = default)
    {
        var window = _getWindow();
        if (window?.StorageProvider is not { } storageProvider)
        {
            return null;
        }

        var pickerOptions = new FilePickerOpenOptions
        {
            Title = options.Title,
            AllowMultiple = false,
            FileTypeFilter = BuildFileTypes(options.FileTypes)
        };

        var result = await storageProvider.OpenFilePickerAsync(pickerOptions);
        return result.Count > 0 ? result[0].Path.LocalPath : null;
    }

    public async Task<string?> PickSaveFileAsync(FilePickerOptions options, CancellationToken cancellationToken = default)
    {
        var window = _getWindow();
        if (window?.StorageProvider is not { } storageProvider)
        {
            return null;
        }

        var pickerOptions = new FilePickerSaveOptions
        {
            Title = options.Title,
            SuggestedFileName = options.SuggestedFileName,
            FileTypeChoices = BuildFileTypes(options.FileTypes)
        };

        var result = await storageProvider.SaveFilePickerAsync(pickerOptions);
        return result?.Path.LocalPath;
    }

    private static List<Avalonia.Platform.Storage.FilePickerFileType>? BuildFileTypes(IReadOnlyList<Services.Abstractions.FilePickerFileType> types)
    {
        if (types.Count == 0)
        {
            return null;
        }

        return types.Select(t => new Avalonia.Platform.Storage.FilePickerFileType(t.DisplayName)
        {
            Patterns = t.Patterns
        }).ToList();
    }
}
