using System;
using System.Threading.Tasks;
using LinkRouter.Settings.Core.Services;
using LinkRouter.Settings.Core.ViewModels;

namespace LinkRouter.Settings.Core.Infrastructure;

public static class SettingsServiceLocator
{
    private static IClipboardService _clipboard = new NullClipboardService();
    private static ILauncherService _launcher = new NullLauncherService();
    private static IFilePickerService _filePicker = new NullFilePickerService();
    private static IMessageDialogService _messageDialog = new NullMessageDialogService();

    public static ConfigService ConfigService { get; set; } = new();
    public static RuleTestService RuleTestService { get; set; } = new();
    public static BrowserDetectionService BrowserDetectionService { get; set; } = new();
    public static ConfigurationState ConfigurationState { get; set; } = new();

    public static IClipboardService Clipboard
    {
        get => _clipboard;
        set => _clipboard = value ?? throw new ArgumentNullException(nameof(value));
    }

    public static ILauncherService Launcher
    {
        get => _launcher;
        set => _launcher = value ?? throw new ArgumentNullException(nameof(value));
    }

    public static IFilePickerService FilePicker
    {
        get => _filePicker;
        set => _filePicker = value ?? throw new ArgumentNullException(nameof(value));
    }

    public static IMessageDialogService MessageDialog
    {
        get => _messageDialog;
        set => _messageDialog = value ?? throw new ArgumentNullException(nameof(value));
    }

    private sealed class NullClipboardService : IClipboardService
    {
        public void SetText(string text)
        {
            // no-op
        }
    }

    private sealed class NullLauncherService : ILauncherService
    {
        public void OpenFolder(string path)
        {
            // no-op
        }

        public void OpenFile(string path)
        {
            // no-op
        }

        public void OpenUri(string uri)
        {
            // no-op
        }
    }

    private sealed class NullFilePickerService : IFilePickerService
    {
        public Task<string?> PickFileAsync(FilePickerOptions options) => Task.FromResult<string?>(null);

        public Task<string?> PickFolderAsync(string? initialDirectory = null) => Task.FromResult<string?>(null);
    }

    private sealed class NullMessageDialogService : IMessageDialogService
    {
        public Task ShowMessageAsync(string title, string message) => Task.CompletedTask;

        public Task<bool> ShowConfirmationAsync(string title, string message, string confirmButtonText = "OK", string cancelButtonText = "Cancel")
            => Task.FromResult(false);
    }
}
