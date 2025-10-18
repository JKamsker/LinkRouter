using System.Threading.Tasks;

namespace LinkRouter.Settings.Services.Interfaces;

public interface IFilePickerService
{
    Task<string?> PickOpenFileAsync(string title, string filter);
    Task<string?> PickSaveFileAsync(string title, string suggestedFileName, string filter);
    Task<string?> PickFolderAsync(string title);
}
