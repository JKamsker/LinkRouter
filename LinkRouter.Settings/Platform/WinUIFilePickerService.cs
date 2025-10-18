using System.Threading.Tasks;
using LinkRouter.Settings.Core.Infrastructure;

namespace LinkRouter.Settings.Platform;

internal sealed class WinUIFilePickerService : IFilePickerService
{
    public Task<string?> PickFileAsync(FilePickerOptions options) => Task.FromResult<string?>(null);

    public Task<string?> PickFolderAsync(string? initialDirectory = null) => Task.FromResult<string?>(null);
}
