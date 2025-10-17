using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using LinkRouter;

namespace LinkRouter.Settings.ViewModels;

public sealed class RuleEditorViewModel : ObservableObject
{
    private readonly ObservableCollection<string> _validationErrors = new();
    public ReadOnlyObservableCollection<string> ValidationErrors { get; }

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
        ValidationErrors = new ReadOnlyObservableCollection<string>(_validationErrors);
        RecalculateValidation();
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
        ValidationErrors = new ReadOnlyObservableCollection<string>(_validationErrors);
        RecalculateValidation();
    }

    public Guid Id { get; } = Guid.NewGuid();

    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (SetProperty(ref _enabled, value))
            {
                RecalculateValidation();
            }
        }
    }

    public string Match
    {
        get => _match;
        set
        {
            if (SetProperty(ref _match, value))
            {
                OnPropertyChanged(nameof(DisplayName));
                RecalculateValidation();
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
                RecalculateValidation();
            }
        }
    }

    public string? Browser
    {
        get => _browser;
        set
        {
            if (SetProperty(ref _browser, value))
            {
                RecalculateValidation();
            }
        }
    }

    public string? ArgsTemplate
    {
        get => _argsTemplate;
        set => SetProperty(ref _argsTemplate, value);
    }

    public string? Profile
    {
        get => _profile;
        set
        {
            if (SetProperty(ref _profile, value))
            {
                RecalculateValidation();
            }
        }
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
        set
        {
            if (SetProperty(ref _useProfile, value))
            {
                RecalculateValidation();
            }
        }
    }

    public string DisplayName => string.IsNullOrWhiteSpace(Pattern) ? Match : $"{Match}: {Pattern}";

    public bool HasValidationErrors => _validationErrors.Count > 0;

    public string? PrimaryError => _validationErrors.FirstOrDefault();

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

    private void RecalculateValidation()
    {
        var issues = new List<string>();

        if (string.IsNullOrWhiteSpace(Pattern))
        {
            issues.Add("Enter a pattern to match.");
        }

        if (!string.Equals(Match, "regex", StringComparison.OrdinalIgnoreCase) && Pattern.Contains(" "))
        {
            issues.Add("Patterns are usually domains or paths without spaces.");
        }

        if (string.IsNullOrWhiteSpace(Browser) && string.IsNullOrWhiteSpace(Profile) && string.IsNullOrWhiteSpace(UseProfile))
        {
            issues.Add("Select a browser or reference a saved profile.");
        }

        if (string.Equals(Match, "regex", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(Pattern))
        {
            try
            {
                _ = Regex.IsMatch("https://example.com", Pattern);
            }
            catch (ArgumentException ex)
            {
                issues.Add($"Regex error: {ex.Message}");
            }
        }

        _validationErrors.Clear();
        foreach (var issue in issues)
        {
            _validationErrors.Add(issue);
        }

        OnPropertyChanged(nameof(HasValidationErrors));
        OnPropertyChanged(nameof(PrimaryError));
    }
}
