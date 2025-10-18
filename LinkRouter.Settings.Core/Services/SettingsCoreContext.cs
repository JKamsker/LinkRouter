namespace LinkRouter.Settings.Core.Services;

public sealed class SettingsCoreContext
{
    public SettingsCoreContext()
    {
        ConfigService = new ConfigService();
        RuleTestService = new RuleTestService();
        BrowserDetectionService = new BrowserDetectionService();
    }

    public ConfigService ConfigService { get; }
    public RuleTestService RuleTestService { get; }
    public BrowserDetectionService BrowserDetectionService { get; }
}
