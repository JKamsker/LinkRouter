using LinkRouter.Settings.ViewModels;

namespace LinkRouter.Settings.Services;

public static class AppServices
{
    public static ConfigService ConfigService { get; } = new();
    public static RuleTestService RuleTestService { get; } = new();
    public static BrowserDetectionService BrowserDetectionService { get; } = new();
    public static ConfigurationState ConfigurationState { get; } = new();
}
