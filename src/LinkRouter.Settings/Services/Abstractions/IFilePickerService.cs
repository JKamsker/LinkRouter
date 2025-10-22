using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LinkRouter.Settings.Services.Abstractions;

public interface IFilePickerService
{
    Task<string?> PickOpenFileAsync(FilePickerOptions options, CancellationToken cancellationToken = default);
    Task<string?> PickSaveFileAsync(FilePickerOptions options, CancellationToken cancellationToken = default);
}

public sealed record FilePickerOptions(string Title, IReadOnlyList<FilePickerFileType> FileTypes, string? SuggestedFileName = null);

public sealed record FilePickerFileType(string DisplayName, IReadOnlyList<string> Patterns);
