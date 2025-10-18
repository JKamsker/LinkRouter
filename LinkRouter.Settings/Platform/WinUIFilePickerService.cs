using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkRouter.Settings.Services.Interfaces;
using Microsoft.UI.Xaml;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace LinkRouter.Settings.Platform;

internal sealed class WinUIFilePickerService : IFilePickerService
{
    private readonly Window _window;

    public WinUIFilePickerService(Window window)
    {
        _window = window;
    }

    public async Task<string?> PickOpenFileAsync(string title, string filter)
    {
        var picker = new FileOpenPicker
        {
            ViewMode = PickerViewMode.List,
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            CommitButtonText = title
        };

        InitializeWithWindow(picker);
        ApplyFilter(picker.FileTypeFilter, filter);

        var file = await picker.PickSingleFileAsync();
        return file?.Path;
    }

    public async Task<string?> PickSaveFileAsync(string title, string suggestedFileName, string filter)
    {
        var picker = new FileSavePicker
        {
            SuggestedFileName = suggestedFileName,
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            CommitButtonText = title
        };

        InitializeWithWindow(picker);
        ApplyFilter(picker.FileTypeChoices, filter);

        var file = await picker.PickSaveFileAsync();
        return file?.Path;
    }

    public async Task<string?> PickFolderAsync(string title)
    {
        var picker = new FolderPicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            CommitButtonText = title
        };

        InitializeWithWindow(picker);
        picker.FileTypeFilter.Add("*");
        var folder = await picker.PickSingleFolderAsync();
        return folder?.Path;
    }

    private void InitializeWithWindow(object picker)
    {
        var hwnd = WindowNative.GetWindowHandle(_window);
        InitializeWithWindow.Initialize(picker, hwnd);
    }

    private static void ApplyFilter(IList<string> filterList, string filter)
    {
        filterList.Clear();
        foreach (var pattern in ParseFilterPatterns(filter))
        {
            var normalized = pattern.StartsWith('.') ? pattern : pattern.TrimStart('*');
            if (string.IsNullOrWhiteSpace(normalized))
            {
                normalized = "*";
            }

            filterList.Add(normalized);
        }
    }

    private static void ApplyFilter(IDictionary<string, IList<string>> filterMap, string filter)
    {
        filterMap.Clear();
        var patterns = ParseFilterPatterns(filter).ToList();
        if (patterns.Count == 0)
        {
            filterMap.Add("All files", new List<string> { "*" });
            return;
        }

        filterMap.Add("Files", patterns);
    }

    private static IEnumerable<string> ParseFilterPatterns(string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            yield break;
        }

        var parts = filter.Split('|');
        if (parts.Length == 2)
        {
            yield return parts[1];
        }
    }
}
