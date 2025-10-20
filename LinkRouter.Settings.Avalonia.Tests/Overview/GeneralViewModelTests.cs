using System;
using System.Collections.Generic;
using LinkRouter;
using LinkRouter.Settings.Services;
using LinkRouter.Settings.Services.Abstractions;
using LinkRouter.Settings.ViewModels;
using Xunit;

namespace LinkRouter.Settings.Avalonia.Tests.Overview;

public class GeneralViewModelTests
{
    [Fact]
    public void SaveCommand_ReflectsUnsavedAndSavingState()
    {
        var configService = new ConfigService();
        var tester = new RuleTestService();
        var state = new ConfigurationState();
        var viewModel = new GeneralViewModel(configService, tester, state, new StubShellService(), new StubClipboardService());

        var document = new ConfigDocument(
            new Config(Array.Empty<Rule>(), null, null),
            configService.ConfigPath,
            DateTime.UtcNow,
            configService.ManifestPath,
            DateTime.UtcNow,
            Array.Empty<ConfigBackup>(),
            new Dictionary<string, ProfileUiState>());

        state.Load(document);

        Assert.False(viewModel.SaveCommand.CanExecute(null));

        state.MarkDirty();
        Assert.True(viewModel.SaveCommand.CanExecute(null));

        viewModel.IsSaving = true;
        Assert.False(viewModel.SaveCommand.CanExecute(null));

        viewModel.IsSaving = false;
        Assert.True(viewModel.SaveCommand.CanExecute(null));

        state.Load(document);
        Assert.False(viewModel.SaveCommand.CanExecute(null));
    }

    private sealed class StubShellService : IShellService
    {
        public void OpenFolder(string path) { }
        public void OpenFile(string path) { }
        public void OpenUri(string uri) { }
    }

    private sealed class StubClipboardService : IClipboardService
    {
        public void SetText(string text) { }
    }
}
