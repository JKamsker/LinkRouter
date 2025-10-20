using LinkRouter.Settings.Services;
using LinkRouter.Settings.Services.Abstractions;
using LinkRouter.Settings.ViewModels;
using Xunit;

namespace LinkRouter.Settings.Avalonia.Tests.Overview;

public class GeneralViewModelTests
{
    [Fact]
    public void CanSaveReflectsUnsavedChangesAndSavingState()
    {
        var state = new ConfigurationState();
        var viewModel = new GeneralViewModel(
            new ConfigService(),
            new RuleTestService(),
            state,
            new StubShellService(),
            new StubClipboardService());

        Assert.False(viewModel.CanSave);

        state.MarkDirty();
        Assert.True(viewModel.CanSave);

        viewModel.IsSaving = true;
        Assert.False(viewModel.CanSave);

        viewModel.IsSaving = false;
        Assert.True(viewModel.CanSave);

        state.MarkSaved();
        Assert.False(viewModel.CanSave);
    }

    private sealed class StubShellService : IShellService
    {
        public void OpenFile(string path)
        {
        }

        public void OpenFolder(string path)
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
