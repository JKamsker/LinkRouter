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
    public async Task BrowseImportCommand_PopulatesPath()
    {
        var configService = new ConfigService();
        var state = new ConfigurationState();
        var picker = new StubFilePickerService { OpenResult = "/tmp/config.json" };
        var viewModel = new ImportExportViewModel(configService, state, picker);

        await viewModel.BrowseImportCommand.ExecuteAsync(null);

        Assert.Equal("/tmp/config.json", viewModel.ImportPath);
        Assert.Null(viewModel.Error);
    }

    [Fact]
    public async Task BrowseExportCommand_PopulatesPath()
    {
        var configService = new ConfigService();
        var state = new ConfigurationState();
        var picker = new StubFilePickerService { SaveResult = "/tmp/export.json" };
        var viewModel = new ImportExportViewModel(configService, state, picker)
        {
            Error = "Previous error"
        };

        await viewModel.BrowseExportCommand.ExecuteAsync(null);

        Assert.Equal("/tmp/export.json", viewModel.ExportPath);
        Assert.Null(viewModel.Error);
    }

    private sealed class StubFilePickerService : IFilePickerService
    {
        public string? OpenResult { get; set; }
        public string? SaveResult { get; set; }

        public Task<string?> PickOpenFileAsync(FilePickerOptions options, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(OpenResult);
        }

        public Task<string?> PickSaveFileAsync(FilePickerOptions options, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(SaveResult);
        }
    }
}
