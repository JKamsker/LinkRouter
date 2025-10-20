using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinkRouter.Settings.Services;
using LinkRouter.Settings.Services.Abstractions;
using LinkRouter.Settings.ViewModels;
using Xunit;

namespace LinkRouter.Settings.Avalonia.Tests.ImportExport;

public class ImportExportViewModelTests
{
    [Fact]
    public async Task BrowseImportPath_UsesFilePickerAndUpdatesPath()
    {
        var state = new ConfigurationState();
        var picker = new StubFilePickerService
        {
            OpenResult = "config.json"
        };

        var viewModel = new ImportExportViewModel(new ConfigService(), state, picker);

        await viewModel.BrowseImportPathCommand.ExecuteAsync(null);

        Assert.Equal("config.json", viewModel.ImportPath);
        Assert.NotNull(picker.LastOpenOptions);
        Assert.Contains("*.json", picker.LastOpenOptions!.FileTypes.Single().Patterns);
    }

    [Fact]
    public async Task BrowseExportPath_UsesFilePickerAndUpdatesPath()
    {
        var state = new ConfigurationState();
        var picker = new StubFilePickerService
        {
            SaveResult = "export.json"
        };

        var viewModel = new ImportExportViewModel(new ConfigService(), state, picker)
        {
            Error = "Something went wrong"
        };

        await viewModel.BrowseExportPathCommand.ExecuteAsync(null);

        Assert.Equal("export.json", viewModel.ExportPath);
        Assert.NotNull(picker.LastSaveOptions);
        Assert.Equal("linkrouter-config.json", picker.LastSaveOptions!.SuggestedFileName);
        Assert.Null(viewModel.Error);
    }

    private sealed class StubFilePickerService : IFilePickerService
    {
        public string? OpenResult { get; set; }
        public string? SaveResult { get; set; }
        public FilePickerOptions? LastOpenOptions { get; private set; }
        public FilePickerOptions? LastSaveOptions { get; private set; }

        public Task<string?> PickOpenFileAsync(FilePickerOptions options, CancellationToken cancellationToken = default)
        {
            LastOpenOptions = options;
            return Task.FromResult(OpenResult);
        }

        public Task<string?> PickSaveFileAsync(FilePickerOptions options, CancellationToken cancellationToken = default)
        {
            LastSaveOptions = options;
            return Task.FromResult(SaveResult);
        }
    }
}
