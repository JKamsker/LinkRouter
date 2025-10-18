using System.Threading;
using System.Threading.Tasks;
using LinkRouter.Settings.Services.Abstractions;

namespace LinkRouter.Settings.Platform;

internal sealed class WinUIFilePickerService : IFilePickerService
{
    public Task<string?> PickOpenFileAsync(FilePickerOptions options, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<string?>(null);
    }

    public Task<string?> PickSaveFileAsync(FilePickerOptions options, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<string?>(null);
    }
}
