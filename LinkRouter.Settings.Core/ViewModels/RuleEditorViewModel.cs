using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LinkRouter;

namespace LinkRouter.Settings.ViewModels;

public sealed class RuleEditorViewModel : ObservableObject
{
    private bool _enabled;
    private string _match;
    private string _pattern;
    private string? _browser;
    private string? _argsTemplate;
    private string? _profile;
    private string? _userDataDir;
    private string? _workingDirectory;
    private string? _useProfile;

    public RuleEditorViewModel()
    {
        _enabled = true;
        _match = "domain";
        _pattern = string.Empty;
    }

    public RuleEditorViewModel(Rule rule)
    {
        _enabled = rule.Enabled;
        _match = rule.match;
        _pattern = rule.pattern;
        _browser = rule.browser;
        _argsTemplate = rule.argsTemplate;
        _profile = rule.profile;
        _userDataDir = rule.userDataDir;
        _workingDirectory = rule.workingDirectory;
        _useProfile = rule.useProfile;
    }

    public Guid Id { get; } = Guid.NewGuid();

    public bool Enabled
    {
        get => _enabled;
        set => SetProperty(ref _enabled, value);
    }

    public string Match
    {
        get => _match;
        set
        {
            if (SetProperty(ref _match, value))
            {
                OnPropertyChanged(nameof(DisplayName));
            }
        }
    }

    public string Pattern
    {
        get => _pattern;
        set
        {
            if (SetProperty(ref _pattern, value))
            {
                OnPropertyChanged(nameof(DisplayName));
            }
        }
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
        return new RuleEditorViewModel
        {
            _enabled = _enabled,
            _match = _match,
            _pattern = _pattern,
            _browser = _browser,
            _argsTemplate = _argsTemplate,
            _profile = _profile,
            _userDataDir = _userDataDir,
            _workingDirectory = _workingDirectory,
            _useProfile = _useProfile
        };
    }

    public void UpdateFrom(RuleEditorViewModel source)
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
