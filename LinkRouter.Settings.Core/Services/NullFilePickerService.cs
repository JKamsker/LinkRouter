using System.Threading.Tasks;
using LinkRouter.Settings.Services.Interfaces;

namespace LinkRouter.Settings.Services;

internal sealed class NullFilePickerService : IFilePickerService
{
    public static IFilePickerService Instance { get; } = new NullFilePickerService();

    private NullFilePickerService()
    {
    }

    public Task<string?> PickOpenFileAsync(string title, string filter)
    {
        return Task.FromResult<string?>(null);
    }

    public Task<string?> PickSaveFileAsync(string title, string suggestedFileName, string filter)
    {
        return Task.FromResult<string?>(null);
    }

    public Task<string?> PickFolderAsync(string title)
    {
        return Task.FromResult<string?>(null);
    }
}
