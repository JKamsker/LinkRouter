using System;
using Xunit;

namespace LinkRouter.Settings.UITests;

[Collection("WinUI-UIThread")]
public class UiInstantiationTests
{
    private readonly WinUiTestHost _host;

    public UiInstantiationTests(WinUiTestHost host)
    {
        _host = host;
    }

    private static void SkipIfNotWindows()
    {
        Skip.IfNot(OperatingSystem.IsWindows(), "UI tests run only on Windows.");
    }

    [SkippableFact]
    public void GeneralPage_Instantiates_On_UI_Thread()
    {
        SkipIfNotWindows();
        Skip.If(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("LINKROUTER_UI_TESTS")), "Set LINKROUTER_UI_TESTS=1 to run UI harness.");
        _host.EnsureStarted();
        Skip.If(!_host.IsReady, $"WinUI not initialized: {_host.StartupErrorMessage}");
        var ex = Record.Exception(() => _host.RunOnUIThreadAsync(() => new LinkRouter.Settings.Views.GeneralPage()).GetAwaiter().GetResult());
        Assert.Null(ex);
    }

    [SkippableFact]
    public void RulesPage_Instantiates_On_UI_Thread()
    {
        SkipIfNotWindows();
        Skip.If(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("LINKROUTER_UI_TESTS")), "Set LINKROUTER_UI_TESTS=1 to run UI harness.");
        _host.EnsureStarted();
        Skip.If(!_host.IsReady, $"WinUI not initialized: {_host.StartupErrorMessage}");
        var ex = Record.Exception(() => _host.RunOnUIThreadAsync(() => new LinkRouter.Settings.Views.RulesPage()).GetAwaiter().GetResult());
        Assert.Null(ex);
    }

    [SkippableFact]
    public void ProfilesPage_Instantiates_On_UI_Thread()
    {
        SkipIfNotWindows();
        Skip.If(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("LINKROUTER_UI_TESTS")), "Set LINKROUTER_UI_TESTS=1 to run UI harness.");
        _host.EnsureStarted();
        Skip.If(!_host.IsReady, $"WinUI not initialized: {_host.StartupErrorMessage}");
        var ex = Record.Exception(() => _host.RunOnUIThreadAsync(() => new LinkRouter.Settings.Views.ProfilesPage()).GetAwaiter().GetResult());
        Assert.Null(ex);
    }

    [SkippableFact]
    public void DefaultPage_Instantiates_On_UI_Thread()
    {
        SkipIfNotWindows();
        Skip.If(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("LINKROUTER_UI_TESTS")), "Set LINKROUTER_UI_TESTS=1 to run UI harness.");
        _host.EnsureStarted();
        Skip.If(!_host.IsReady, $"WinUI not initialized: {_host.StartupErrorMessage}");
        var ex = Record.Exception(() => _host.RunOnUIThreadAsync(() => new LinkRouter.Settings.Views.DefaultPage()).GetAwaiter().GetResult());
        Assert.Null(ex);
    }

    [SkippableFact]
    public void ImportExportPage_Instantiates_On_UI_Thread()
    {
        SkipIfNotWindows();
        Skip.If(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("LINKROUTER_UI_TESTS")), "Set LINKROUTER_UI_TESTS=1 to run UI harness.");
        _host.EnsureStarted();
        Skip.If(!_host.IsReady, $"WinUI not initialized: {_host.StartupErrorMessage}");
        var ex = Record.Exception(() => _host.RunOnUIThreadAsync(() => new LinkRouter.Settings.Views.ImportExportPage()).GetAwaiter().GetResult());
        Assert.Null(ex);
    }

    [SkippableFact]
    public void AdvancedPage_Instantiates_On_UI_Thread()
    {
        SkipIfNotWindows();
        Skip.If(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("LINKROUTER_UI_TESTS")), "Set LINKROUTER_UI_TESTS=1 to run UI harness.");
        _host.EnsureStarted();
        Skip.If(!_host.IsReady, $"WinUI not initialized: {_host.StartupErrorMessage}");
        var ex = Record.Exception(() => _host.RunOnUIThreadAsync(() => new LinkRouter.Settings.Views.AdvancedPage()).GetAwaiter().GetResult());
        Assert.Null(ex);
    }

    [SkippableFact]
    public void AboutPage_Instantiates_On_UI_Thread()
    {
        SkipIfNotWindows();
        Skip.If(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("LINKROUTER_UI_TESTS")), "Set LINKROUTER_UI_TESTS=1 to run UI harness.");
        _host.EnsureStarted();
        Skip.If(!_host.IsReady, $"WinUI not initialized: {_host.StartupErrorMessage}");
        var ex = Record.Exception(() => _host.RunOnUIThreadAsync(() => new LinkRouter.Settings.Views.AboutPage()).GetAwaiter().GetResult());
        Assert.Null(ex);
    }

    [SkippableFact]
    public void SettingsCard_Instantiates_On_UI_Thread()
    {
        SkipIfNotWindows();
        Skip.If(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("LINKROUTER_UI_TESTS")), "Set LINKROUTER_UI_TESTS=1 to run UI harness.");
        _host.EnsureStarted();
        Skip.If(!_host.IsReady, $"WinUI not initialized: {_host.StartupErrorMessage}");
        var ex = Record.Exception(() => _host.RunOnUIThreadAsync(() => new LinkRouter.Settings.Controls.SettingsCard()).GetAwaiter().GetResult());
        Assert.Null(ex);
    }
}
