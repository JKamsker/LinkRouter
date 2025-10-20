using System;
using CommunityToolkit.Mvvm.ComponentModel;
using LinkRouter;

namespace LinkRouter.Settings.ViewModels;

public sealed partial class RuleEditorViewModel : ObservableObject
{
    [ObservableProperty]
    private bool enabled = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    private string match = "domain";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    private string pattern = string.Empty;

    [ObservableProperty]
    private string? browser;

    [ObservableProperty]
    private string? argsTemplate;

    [ObservableProperty]
    private string? profile;

    [ObservableProperty]
    private string? userDataDir;

    [ObservableProperty]
    private string? workingDirectory;

    [ObservableProperty]
    private string? useProfile;

    public RuleEditorViewModel()
    {
    }

    public RuleEditorViewModel(Rule rule)
    {
        Enabled = rule.Enabled;
        Match = rule.match;
        Pattern = rule.pattern;
        Browser = rule.browser;
        ArgsTemplate = rule.argsTemplate;
        Profile = rule.profile;
        UserDataDir = rule.userDataDir;
        WorkingDirectory = rule.workingDirectory;
        UseProfile = rule.useProfile;
    }

    public Guid Id { get; } = Guid.NewGuid();

    public string DisplayName => string.IsNullOrWhiteSpace(Pattern) ? Match : $"{Match}: {Pattern}";

    public Rule ToRule()
    {
        return new Rule(
            match: Match,
            pattern: Pattern,
            browser: Browser,
            argsTemplate: ArgsTemplate,
            profile: Profile,
            userDataDir: UserDataDir,
            workingDirectory: WorkingDirectory,
            useProfile: UseProfile,
            Enabled: Enabled
        );
    }

    public RuleEditorViewModel Clone()
    {
        var clone = new RuleEditorViewModel
        {
            Enabled = Enabled,
            Match = Match,
            Pattern = Pattern,
            Browser = Browser,
            ArgsTemplate = ArgsTemplate,
            Profile = Profile,
            UserDataDir = UserDataDir,
            WorkingDirectory = WorkingDirectory,
            UseProfile = UseProfile
        };

        return clone;
    }

    public void CopyFrom(RuleEditorViewModel source)
    {
        Enabled = source.Enabled;
        Match = source.Match;
        Pattern = source.Pattern;
        Browser = source.Browser;
        ArgsTemplate = source.ArgsTemplate;
        Profile = source.Profile;
        UserDataDir = source.UserDataDir;
        WorkingDirectory = source.WorkingDirectory;
        UseProfile = source.UseProfile;
    }
}
