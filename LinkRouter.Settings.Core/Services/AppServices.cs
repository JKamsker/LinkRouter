using System.Threading;
using System.Threading.Tasks;
using LinkRouter.Settings.Services.Abstractions;
using LinkRouter.Settings.ViewModels;

namespace LinkRouter.Settings.Services;

public static class AppServices
{
    public static ConfigService ConfigService { get; } = new();
    public static RuleTestService RuleTestService { get; } = new();
    public static BrowserDetectionService BrowserDetectionService { get; } = new();
    public static ConfigurationState ConfigurationState { get; } = new();

    public static IClipboardService ClipboardService { get; set; } = NullClipboardService.Instance;
    public static IShellService ShellService { get; set; } = NullShellService.Instance;
    public static IFilePickerService FilePickerService { get; set; } = NullFilePickerService.Instance;
    public static IMessageDialogService MessageDialogService { get; set; } = NullMessageDialogService.Instance;

    private sealed class NullClipboardService : IClipboardService
    {
        public static NullClipboardService Instance { get; } = new();
        public void SetText(string text) { }
    }

    private sealed class NullShellService : IShellService
    {
        public static NullShellService Instance { get; } = new();
        public void OpenFolder(string path) { }
        public void OpenFile(string path) { }
        public void OpenUri(string uri) { }
    }

    private sealed class NullFilePickerService : IFilePickerService
    {
        public static NullFilePickerService Instance { get; } = new();

        public Task<string?> PickOpenFileAsync(FilePickerOptions options, CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);

        public Task<string?> PickSaveFileAsync(FilePickerOptions options, CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);
    }

    private sealed class NullMessageDialogService : IMessageDialogService
    {
        public static NullMessageDialogService Instance { get; } = new();

        public Task<MessageDialogResult> ShowMessageAsync(string title, string message, MessageDialogOptions options) => Task.FromResult(MessageDialogResult.None);
    }
}
