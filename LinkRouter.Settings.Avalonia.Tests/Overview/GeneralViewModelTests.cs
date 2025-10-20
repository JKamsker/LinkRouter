using LinkRouter.Settings.Services;
using LinkRouter.Settings.Services.Abstractions;
using LinkRouter.Settings.ViewModels;
using Xunit;

namespace LinkRouter.Settings.Avalonia.Tests.Overview;

public class GeneralViewModelTests
{
    [Fact]
    public void CanSaveReflectsStateAndSaving()
    {
        var configService = new ConfigService();
        var tester = new RuleTestService();
        var state = new ConfigurationState();
        var shell = new StubShellService();
        var clipboard = new StubClipboardService();

        var viewModel = new GeneralViewModel(configService, tester, state, shell, clipboard);

        Assert.False(viewModel.CanSave);

        state.MarkDirty();
        Assert.True(viewModel.CanSave);

        viewModel.IsSaving = true;
        Assert.False(viewModel.CanSave);

        viewModel.IsSaving = false;
        Assert.True(viewModel.CanSave);
    }

    private sealed class StubShellService : IShellService
    {
        public void OpenFolder(string path)
        {
        }

        public void OpenFile(string path)
        {
        }

        public void OpenUri(string uri)
        {
        }
    }

    private sealed class StubClipboardService : IClipboardService
    {
        public void SetText(string text)
        {
        }
    }
}
