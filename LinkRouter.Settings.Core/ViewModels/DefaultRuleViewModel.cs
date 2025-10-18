using CommunityToolkit.Mvvm.ComponentModel;
using LinkRouter;

namespace LinkRouter.Settings.ViewModels;

public sealed class DefaultRuleViewModel : ObservableObject
{
    private bool _enabled = true;
    private string? _browser;
    private string? _argsTemplate;
    private string? _profile;
    private string? _userDataDir;
    private string? _workingDirectory;
    private string? _useProfile;

    public bool Enabled
    {
        get => _enabled;
        set => SetProperty(ref _enabled, value);
    }

    public string? Browser
    {
        get => _browser;
        set => SetProperty(ref _browser, value);
    }

    public string? ArgsTemplate
    {
        get => _argsTemplate;
        set => SetProperty(ref _argsTemplate, value);
    }

    public string? Profile
    {
        get => _profile;
        set => SetProperty(ref _profile, value);
    }

    public string? UserDataDir
    {
        get => _userDataDir;
        set => SetProperty(ref _userDataDir, value);
    }

    public string? WorkingDirectory
    {
        get => _workingDirectory;
        set => SetProperty(ref _workingDirectory, value);
    }

    public string? UseProfile
    {
        get => _useProfile;
        set => SetProperty(ref _useProfile, value);
    }

    public void Load(Rule? rule)
    {
        if (rule is null)
        {
            Enabled = true;
            Browser = null;
            ArgsTemplate = null;
            Profile = null;
            UserDataDir = null;
            WorkingDirectory = null;
            UseProfile = null;
            return;
        }

        Enabled = rule.Enabled;
        Browser = rule.browser;
        ArgsTemplate = rule.argsTemplate;
        Profile = rule.profile;
        UserDataDir = rule.userDataDir;
        WorkingDirectory = rule.workingDirectory;
        UseProfile = rule.useProfile;
    }

    public Rule? ToRuleOrNull()
    {
        if (!Enabled && string.IsNullOrWhiteSpace(Browser) && string.IsNullOrWhiteSpace(ArgsTemplate) && string.IsNullOrWhiteSpace(UseProfile))
        {
            return null;
        }

        return new Rule(
            match: "default",
            pattern: ".*",
            browser: Browser,
            argsTemplate: ArgsTemplate,
            profile: Profile,
            userDataDir: UserDataDir,
            workingDirectory: WorkingDirectory,
            useProfile: UseProfile,
            Enabled: Enabled
        );
    }
}
