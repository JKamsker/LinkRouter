using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Headless.XUnit;
using LinkRouter.Settings.Avalonia;
using LinkRouter.Settings.Services;
using LinkRouter.Settings.Services.Abstractions;
using LinkRouter.Settings.ViewModels;
using Xunit;

namespace LinkRouter.Settings.Avalonia.Tests.Startup;

public class ApplicationStartupTests
{
    [AvaloniaFact(Timeout = 30_000)]
    public Task MainWindow_CanBeConstructed()
    {
        var configService = new ConfigService();
        var ruleTester = new RuleTestService();
        var state = new ConfigurationState();
        var shellService = new StubShellService();
        var clipboard = new StubClipboardService();
        var dialog = new StubDialogService();

        var general = new GeneralViewModel(configService, ruleTester, state, shellService, clipboard);
        var rules = new RulesViewModel(state, ruleTester, dialog);
        var profiles = new ProfilesViewModel(state, new BrowserDetectionService());
        var importExport = new ImportExportViewModel(configService, state);
        var advanced = new AdvancedViewModel(configService, shellService);
        var about = new AboutViewModel(shellService);

        var shell = new SettingsShellViewModel(general, rules, profiles, importExport, advanced, about);
        var window = new MainWindow(shell);
        Assert.NotNull(window);
        return Task.CompletedTask;
    }

    private sealed class StubShellService : IShellService
    {
        public void OpenFile(string path) { }
        public void OpenFolder(string path) { }
        public void OpenUri(string uri) { }
    }

    private sealed class StubClipboardService : IClipboardService
    {
        public void SetText(string text) { }
    }

    private sealed class StubDialogService : IDialogService
    {
        public Task<bool> ShowRuleEditorAsync(RuleEditorViewModel rule, IReadOnlyList<string> matchTypes, IReadOnlyList<string> profileOptions, CancellationToken cancellationToken = default)
            => Task.FromResult(false);
    }
}
