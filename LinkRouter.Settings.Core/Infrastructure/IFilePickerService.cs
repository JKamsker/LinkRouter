using System.Threading.Tasks;

namespace LinkRouter.Settings.Core.Infrastructure;

public interface IFilePickerService
{
    Task<string?> PickFileAsync(FilePickerOptions options);
    Task<string?> PickFolderAsync(string? initialDirectory = null);
}

public sealed record FilePickerOptions(string Title, string? InitialDirectory = null, string[]? Filters = null);
