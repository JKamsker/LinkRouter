using LinkRouter.Settings.Services;
using LinkRouter.Settings.Services.Abstractions;
using LinkRouter.Settings.ViewModels;
using Xunit;

namespace LinkRouter.Settings.Tests.Overview;

public class GeneralViewModelTests
{
    [Fact]
    public void CanSaveReflectsStateAndSavingFlag()
    {
        var configService = new ConfigService();
        var ruleTestService = new RuleTestService();
        var state = new ConfigurationState();
        var shell = new NullShellService();
        var clipboard = new NullClipboardService();

        var resolver = new NullRouterPathResolver();
        var autostart = new NullAutostartService();
        var browserDetection = new BrowserDetectionService();
        var viewModel = new GeneralViewModel(configService, ruleTestService, state, shell, clipboard, resolver, autostart, browserDetection);

        Assert.False(viewModel.CanSave);

        state.MarkDirty();
        Assert.True(viewModel.CanSave);

        viewModel.IsSaving = true;
        Assert.False(viewModel.CanSave);

        viewModel.IsSaving = false;
        Assert.True(viewModel.CanSave);
    }

    private sealed class NullShellService : IShellService
    {
        public void OpenFile(string path) { }
        public void OpenFolder(string path) { }
        public void OpenUri(string uri) { }
    }

    private sealed class NullClipboardService : IClipboardService
    {
        public void SetText(string text) { }
    }

    private sealed class NullRouterPathResolver : IRouterPathResolver
    {
        public bool TryGetRouterExecutable([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? path)
        {
            path = null;
            return false;
        }
    }

    private sealed class NullAutostartService : IAutostartService
    {
        public bool IsSupported => false;

        public bool IsEnabled() => false;

        public void SetEnabled(bool enabled) { }
    }
}
