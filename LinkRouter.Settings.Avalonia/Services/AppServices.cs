using LinkRouter.Settings.Core.Services;

namespace LinkRouter.Settings.Avalonia.Services;

public static class AppServices
{
    public static SettingsCoreContext CoreContext { get; } = new();

    public static ConfigService ConfigService => CoreContext.ConfigService;
    public static RuleTestService RuleTestService => CoreContext.RuleTestService;
    public static BrowserDetectionService BrowserDetectionService => CoreContext.BrowserDetectionService;
}

