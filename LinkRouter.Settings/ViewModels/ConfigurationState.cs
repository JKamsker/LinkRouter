using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using LinkRouter;
using LinkRouter.Settings.Services;

namespace LinkRouter.Settings.ViewModels;

public sealed class ConfigurationState : ObservableObject
{
    private bool _isDirty;
    private ConfigDocument? _document;
    private ProfileEditorViewModel? _defaultProfile;
    private bool _isDefaultEnabled;
    private bool _isAutostartEnabled = true;

    public ObservableCollection<RuleEditorViewModel> Rules { get; } = new();
    public ObservableCollection<ProfileEditorViewModel> Profiles { get; } = new();

    public ConfigDocument? Document
    {
        get => _document;
        private set => SetProperty(ref _document, value);
    }

    public bool HasUnsavedChanges
    {
        get => _isDirty;
        private set => SetProperty(ref _isDirty, value);
    }

    public ProfileEditorViewModel? DefaultProfile
    {
        get => _defaultProfile;
        private set
        {
            if (!ReferenceEquals(_defaultProfile, value))
            {
                _defaultProfile = value;
                OnPropertyChanged(nameof(DefaultProfile));
            }
        }
    }

    public bool IsDefaultEnabled
    {
        get => _isDefaultEnabled;
        private set
        {
            if (_isDefaultEnabled != value)
            {
                _isDefaultEnabled = value;
                OnPropertyChanged(nameof(IsDefaultEnabled));
            }
        }
    }

    public bool IsAutostartEnabled => _isAutostartEnabled;

    public event EventHandler? StateChanged;

    public void Load(ConfigDocument document)
    {
        Unsubscribe(Rules);
        Unsubscribe(Profiles);

        Rules.Clear();
        Profiles.Clear();

        foreach (var rule in document.Config.rules)
        {
            var vm = new RuleEditorViewModel(rule);
            vm.PropertyChanged += OnChildPropertyChanged;
            Rules.Add(vm);
        }

        var profileStates = document.ProfileStates;

        if (document.Config.profiles is not null)
        {
            foreach (var kvp in document.Config.profiles)
            {
                var vm = new ProfileEditorViewModel(kvp.Key, kvp.Value);
                vm.InitializeAdvanced(ShouldStartAdvanced(kvp.Key, kvp.Value, profileStates));
                vm.SetDefaultFlag(false);
                vm.PropertyChanged += OnChildPropertyChanged;
                Profiles.Add(vm);
            }
        }

        if (document.Config.@default is { } defaultRule)
        {
            IsDefaultEnabled = defaultRule.Enabled;
            if (!string.IsNullOrWhiteSpace(defaultRule.useProfile))
            {
                var match = FindProfileByName(defaultRule.useProfile);
                if (match is null && HasDefaultRulePayload(defaultRule))
                {
                    match = CreateProfileFromDefault(defaultRule);
                }

                DefaultProfile = match;
            }
            else if (HasDefaultRulePayload(defaultRule))
            {
                var profile = CreateProfileFromDefault(defaultRule);
                DefaultProfile = profile;
            }
            else
            {
                DefaultProfile = null;
            }
        }
        else
        {
            DefaultProfile = null;
            IsDefaultEnabled = false;
        }

        if (DefaultProfile is null)
        {
            IsDefaultEnabled = false;
        }

        _isAutostartEnabled = document.ApplicationSettings.AutostartEnabled;
        OnPropertyChanged(nameof(IsAutostartEnabled));

        UpdateDefaultFlags();

        Document = document;
        HasUnsavedChanges = false;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public Config BuildConfig()
    {
        var rules = Rules.Select(rule => rule.ToRule()).ToArray();
        Rule? defaultRule = null;
        if (DefaultProfile is not null && !string.IsNullOrWhiteSpace(DefaultProfile.Name))
        {
            defaultRule = new Rule(
                match: "default",
                pattern: ".*",
                browser: null,
                argsTemplate: null,
                profile: null,
                userDataDir: null,
                workingDirectory: null,
                useProfile: DefaultProfile.Name,
                Enabled: IsDefaultEnabled);
        }
        Dictionary<string, Profile>? profiles = null;
        if (Profiles.Count > 0)
        {
            profiles = Profiles
                .Where(p => !string.IsNullOrWhiteSpace(p.Name))
                .ToDictionary(p => p.Name, p => p.ToProfile(), StringComparer.OrdinalIgnoreCase);
        }

        return new Config(rules, defaultRule, profiles);
    }

    public SettingsSnapshot BuildSettingsSnapshot()
    {
        var config = BuildConfig();
        var profileStates = Profiles
            .Where(p => !string.IsNullOrWhiteSpace(p.Name))
            .ToDictionary(
                p => p.Name,
                p => new ProfileUiState(p.IsAdvanced),
                StringComparer.OrdinalIgnoreCase);

        return new SettingsSnapshot(config, new ApplicationSettings(_isAutostartEnabled), profileStates);
    }

    public void MarkSaved()
    {
        HasUnsavedChanges = false;
    }

    public void MarkDirty()
    {
        HasUnsavedChanges = true;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetAutostartEnabled(bool value)
    {
        if (_isAutostartEnabled == value)
        {
            return;
        }

        _isAutostartEnabled = value;
        OnPropertyChanged(nameof(IsAutostartEnabled));
        MarkDirty();
    }

    public void AddRule(RuleEditorViewModel rule)
    {
        rule.PropertyChanged += OnChildPropertyChanged;
        Rules.Add(rule);
        MarkDirty();
    }

    public void RemoveRule(RuleEditorViewModel rule)
    {
        rule.PropertyChanged -= OnChildPropertyChanged;
        Rules.Remove(rule);
        MarkDirty();
    }

    public void MoveRule(int oldIndex, int newIndex)
    {
        if (oldIndex == newIndex || oldIndex < 0 || newIndex < 0 || oldIndex >= Rules.Count || newIndex >= Rules.Count)
        {
            return;
        }

        var item = Rules[oldIndex];
        Rules.RemoveAt(oldIndex);
        Rules.Insert(newIndex, item);
        MarkDirty();
    }

    public void AddProfile(ProfileEditorViewModel profile)
    {
        profile.PropertyChanged += OnChildPropertyChanged;
        Profiles.Add(profile);
        MarkDirty();
    }

    public void RemoveProfile(ProfileEditorViewModel profile)
    {
        profile.PropertyChanged -= OnChildPropertyChanged;
        Profiles.Remove(profile);
        if (ReferenceEquals(DefaultProfile, profile))
        {
            DefaultProfile = null;
            IsDefaultEnabled = false;
            UpdateDefaultFlags();
        }
        MarkDirty();
    }

    public void SetDefault(ProfileEditorViewModel? profile)
    {
        if (profile is null)
        {
            ClearDefault();
            return;
        }

        bool changed = false;

        if (!ReferenceEquals(DefaultProfile, profile))
        {
            DefaultProfile = profile;
            changed = true;
        }

        if (!IsDefaultEnabled)
        {
            IsDefaultEnabled = true;
            changed = true;
        }

        if (changed)
        {
            UpdateDefaultFlags();
            MarkDirty();
        }
    }

    public void ClearDefault()
    {
        if (DefaultProfile is null && !IsDefaultEnabled)
        {
            return;
        }

        DefaultProfile = null;
        IsDefaultEnabled = false;
        UpdateDefaultFlags();
        MarkDirty();
    }

    private void OnChildPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is ProfileEditorViewModel && e.PropertyName == nameof(ProfileEditorViewModel.IsDefault))
        {
            return;
        }

        HasUnsavedChanges = true;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Unsubscribe<T>(IEnumerable<T> items) where T : INotifyPropertyChanged
    {
        foreach (var item in items)
        {
            item.PropertyChanged -= OnChildPropertyChanged;
        }
    }

    private bool ShouldStartAdvanced(string profileName, Profile profile, IReadOnlyDictionary<string, ProfileUiState> profileStates)
    {
        if (profileStates.TryGetValue(profileName, out var state))
        {
            return state.IsAdvanced;
        }

        var template = profile.argsTemplate;
        if (string.IsNullOrWhiteSpace(template))
        {
            return true;
        }

        return !IsSimpleTemplate(template);
    }

    private static bool IsSimpleTemplate(string template)
    {
        var normalized = NormalizeTemplate(template);
        foreach (var candidate in SimpleArgsTemplates)
        {
            if (string.Equals(normalized, candidate, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string NormalizeTemplate(string template)
    {
        var parts = template.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', parts);
    }

    private static readonly string[] SimpleArgsTemplates =
    {
        "\"{url}\"",
        "-new-window \"{url}\"",
        "--new-window \"{url}\"",
        "-private-window \"{url}\"",
        "--incognito \"{url}\"",
        "--incognito --new-window \"{url}\"",
        "--new-window --incognito \"{url}\"",
        "--inprivate \"{url}\"",
        "--inprivate --new-window \"{url}\"",
        "--new-window --inprivate \"{url}\""
    };

    private ProfileEditorViewModel? FindProfileByName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return Profiles.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    private bool HasDefaultRulePayload(Rule rule)
    {
        return !string.IsNullOrWhiteSpace(rule.browser)
            || !string.IsNullOrWhiteSpace(rule.argsTemplate)
            || !string.IsNullOrWhiteSpace(rule.profile)
            || !string.IsNullOrWhiteSpace(rule.userDataDir)
            || !string.IsNullOrWhiteSpace(rule.workingDirectory)
            || rule.incognito.HasValue;
    }

    private ProfileEditorViewModel CreateProfileFromDefault(Rule rule)
    {
        var name = GenerateUniqueProfileName("Default");
        var profile = new Profile(rule.browser, rule.argsTemplate, rule.profile, rule.userDataDir, rule.workingDirectory, rule.incognito ?? false);
        var vm = new ProfileEditorViewModel(name, profile);
        vm.InitializeAdvanced(true);
        vm.SetDefaultFlag(false);
        vm.PropertyChanged += OnChildPropertyChanged;
        Profiles.Add(vm);
        return vm;
    }

    private string GenerateUniqueProfileName(string baseName)
    {
        var existing = new HashSet<string>(Profiles.Select(p => p.Name), StringComparer.OrdinalIgnoreCase);
        if (!existing.Contains(baseName))
        {
            return baseName;
        }

        for (int i = 2; i < 1000; i++)
        {
            var candidate = $"{baseName}{i}";
            if (!existing.Contains(candidate))
            {
                return candidate;
            }
        }

        return baseName + Guid.NewGuid().ToString("N");
    }

    private void UpdateDefaultFlags()
    {
        foreach (var profile in Profiles)
        {
            profile.SetDefaultFlag(IsDefaultEnabled && ReferenceEquals(profile, DefaultProfile));
        }
    }
}
