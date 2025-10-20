using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinkRouter;
using LinkRouter.Settings.Services;

namespace LinkRouter.Settings.ViewModels;

public partial class ProfilesViewModel : ObservableObject
{
    private readonly ConfigurationState _state;
    private readonly BrowserDetectionService _detector;
    private string? _systemDefaultBrowserPath;
    private string? _lastSelectedProfileName;
    private int _lastSelectedProfileIndex;

    [ObservableProperty]
    private ProfileEditorViewModel? _selectedProfile;

    [ObservableProperty]
    private BrowserInfo? _selectedBrowser;

    [ObservableProperty]
    private BrowserProfileOption? _selectedDetectedProfile;

    [ObservableProperty]
    private bool _selectedProfileNewWindow;

    [ObservableProperty]
    private bool _selectedProfileIncognito;

    [ObservableProperty]
    private string? _detectionError;

    public ObservableCollection<ProfileEditorViewModel> Profiles => _state.Profiles;
    public ObservableCollection<BrowserInfo> Browsers { get; } = new();
    public ObservableCollection<BrowserProfileOption> DetectedProfiles { get; } = new();

    public bool HasDetectionError => !string.IsNullOrWhiteSpace(DetectionError);

    public bool SelectedProfileIsDefault
    {
        get => SelectedProfile is not null
            && _state.IsDefaultEnabled
            && ReferenceEquals(SelectedProfile, _state.DefaultProfile);
        set
        {
            if (SelectedProfile is null)
            {
                return;
            }

            if (value)
            {
                _state.SetDefault(SelectedProfile);
            }
            else if (ReferenceEquals(_state.DefaultProfile, SelectedProfile))
            {
                _state.ClearDefault();
            }

            OnPropertyChanged(nameof(SelectedProfileIsDefault));
        }
    }

    public bool CanEnterAdvanced => SelectedProfile is not null && !SelectedProfile.IsAdvanced;

    private bool _suppressSelectionUpdates;
    private bool _ensuringDefaultProfileSelection;

    public ProfilesViewModel(ConfigurationState state, BrowserDetectionService detector)
    {
        _state = state;
        _detector = detector;
        RefreshDetections();
        _state.PropertyChanged += OnStatePropertyChanged;
        _state.StateChanged += OnStateChanged;
    }

    private void RefreshDetections()
    {
        try
        {
            Browsers.Clear();
            foreach (var browser in _detector.DetectInstalledBrowsers())
            {
                Browsers.Add(browser);
            }

            DetectionError = null;
            _systemDefaultBrowserPath = _detector.GetDefaultBrowserExecutablePath();
            SyncSelectionsWithProfile();
            EnsureDefaultSelections();
        }
        catch (Exception ex)
        {
            DetectionError = ex.Message;
        }
    }

    partial void OnDetectionErrorChanged(string? value) => OnPropertyChanged(nameof(HasDetectionError));

    partial void OnSelectedProfileChanging(ProfileEditorViewModel? value)
    {
        if (SelectedProfile is not null)
        {
            SelectedProfile.PropertyChanged -= OnSelectedProfilePropertyChanged;
        }
    }

    partial void OnSelectedProfileChanged(ProfileEditorViewModel? value)
    {
        if (value is not null)
        {
            value.PropertyChanged += OnSelectedProfilePropertyChanged;
            var index = Profiles.IndexOf(value);
            if (index >= 0)
            {
                _lastSelectedProfileIndex = index;
            }

            _lastSelectedProfileName = value.Name;
        }

        SyncSelectionsWithProfile();
        EnsureDefaultSelections();
    }

    partial void OnSelectedBrowserChanged(BrowserInfo? value)
    {
        if (_suppressSelectionUpdates || SelectedProfile is null)
        {
            return;
        }

        if (value is null)
        {
            SelectedProfile.Browser = null;
            SelectedProfile.WorkingDirectory = null;
            DetectedProfiles.Clear();
            SelectedDetectedProfile = null;
            return;
        }

        SelectedProfile.Browser = value.Path;
        if (!SelectedProfile.IsAdvanced)
        {
            SelectedProfile.ArgsTemplate = BuildArgsTemplate(value, SelectedProfileNewWindow, SelectedProfileIncognito);
            SelectedProfile.WorkingDirectory = GetBrowserWorkingDirectory(value);
            SelectedProfile.Profile = null;
            SelectedProfile.UserDataDir = null;
        }

        UpdateDetectedProfiles(value);
        if (!SelectedProfile.IsAdvanced)
        {
            EnsureDefaultProfileSelection();
        }
    }

    partial void OnSelectedDetectedProfileChanged(BrowserProfileOption? value)
    {
        if (_suppressSelectionUpdates || SelectedProfile is null || SelectedProfile.IsAdvanced)
        {
            return;
        }

        if (value is null)
        {
            SelectedProfile.Profile = null;
            SelectedProfile.UserDataDir = null;
        }
        else
        {
            SelectedProfile.Profile = value.ProfileArgument;
            SelectedProfile.UserDataDir = value.UserDataDir;
        }
    }

    partial void OnSelectedProfileNewWindowChanged(bool value)
    {
        if (_suppressSelectionUpdates || SelectedProfile is null || SelectedProfile.IsAdvanced)
        {
            return;
        }

        SelectedProfile.ArgsTemplate = BuildArgsTemplate(SelectedBrowser, value, SelectedProfileIncognito);
    }

    partial void OnSelectedProfileIncognitoChanged(bool value)
    {
        if (_suppressSelectionUpdates || SelectedProfile is null)
        {
            return;
        }

        if (SelectedProfile.Incognito != value)
        {
            SelectedProfile.Incognito = value;
        }

        if (SelectedProfile.IsAdvanced)
        {
            return;
        }

        SelectedProfile.ArgsTemplate = BuildArgsTemplate(SelectedBrowser, SelectedProfileNewWindow, value);
    }

    [RelayCommand]
    private void AddProfile()
    {
        var vm = new ProfileEditorViewModel
        {
            Name = $"Profile{Profiles.Count + 1}"
        };
        _state.AddProfile(vm);
        SelectedProfile = vm;
    }

    [RelayCommand]
    private void RemoveProfile(ProfileEditorViewModel? profile)
    {
        if (profile is null)
        {
            return;
        }

        _state.RemoveProfile(profile);
        if (ReferenceEquals(SelectedProfile, profile))
        {
            SelectedProfile = null;
        }
    }

    [RelayCommand]
    private void EnterAdvancedMode()
    {
        if (SelectedProfile is null)
        {
            return;
        }

        if (!SelectedProfile.IsAdvanced)
        {
            SelectedProfile.IsAdvanced = true;
            OnPropertyChanged(nameof(CanEnterAdvanced));
        }
    }

    [RelayCommand]
    private void TestLaunch(ProfileEditorViewModel? profile)
    {
        if (profile is null)
        {
            return;
        }

        try
        {
            DetectionError = null;
            var config = _state.BuildConfig();
            var rule = new Rule(
                match: "default",
                pattern: ".*",
                browser: profile.Browser,
                argsTemplate: profile.ArgsTemplate,
                profile: profile.Profile,
                userDataDir: profile.UserDataDir,
                workingDirectory: profile.WorkingDirectory,
                useProfile: profile.Name,
                incognito: profile.Incognito,
                Enabled: true
            );

            var effective = ProfileResolver.ResolveEffectiveRule(config, rule);
            var uri = new Uri("https://example.com/");
            var args = BrowserLauncher.GetLaunchArguments(effective, uri);
            Process.Start(new ProcessStartInfo
            {
                FileName = effective.browser!,
                Arguments = args,
                UseShellExecute = false
            });
        }
        catch (Exception ex)
        {
            DetectionError = ex.Message;
        }
    }

    private void SyncSelectionsWithProfile()
    {
        _suppressSelectionUpdates = true;
        try
        {
            if (SelectedProfile is null)
            {
                SelectedBrowser = null;
                DetectedProfiles.Clear();
                SetSelectedDetectedProfileSilently(null);
                SelectedProfileNewWindow = false;
                SelectedProfileIncognito = false;
            }
            else
            {
                SelectedBrowser = FindBrowserByPath(SelectedProfile.Browser);
                UpdateDetectedProfiles(SelectedBrowser);
                SetSelectedDetectedProfileSilently(MatchProfileOption(SelectedProfile));
                SelectedProfileNewWindow = DetermineNewWindow(SelectedProfile);
                SelectedProfileIncognito = DetermineIncognito(SelectedProfile);
            }
        }
        finally
        {
            _suppressSelectionUpdates = false;
        }

        OnPropertyChanged(nameof(SelectedProfileIsDefault));
        OnPropertyChanged(nameof(CanEnterAdvanced));
    }

    private BrowserInfo? FindBrowserByPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return Browsers.FirstOrDefault(b => string.Equals(b.Path, path, StringComparison.OrdinalIgnoreCase));
    }

    private void UpdateDetectedProfiles(BrowserInfo? browser)
    {
        if (browser is null)
        {
            if (DetectedProfiles.Count > 0)
            {
                DetectedProfiles.Clear();
            }

            return;
        }

        var options = _detector.GetBrowserProfileOptions(browser);

        var needsUpdate = options.Count != DetectedProfiles.Count;
        if (!needsUpdate)
        {
            for (var i = 0; i < options.Count; i++)
            {
                if (!Equals(DetectedProfiles[i], options[i]))
                {
                    needsUpdate = true;
                    break;
                }
            }
        }

        if (!needsUpdate)
        {
            return;
        }

        DetectedProfiles.Clear();
        foreach (var option in options)
        {
            DetectedProfiles.Add(option);
        }

        if (_suppressSelectionUpdates && SelectedProfile is not null)
        {
            SetSelectedDetectedProfileSilently(MatchProfileOption(SelectedProfile));
        }
    }

    private BrowserProfileOption? MatchProfileOption(ProfileEditorViewModel profile)
    {
        if (DetectedProfiles.Count == 0)
        {
            return null;
        }

        foreach (var option in DetectedProfiles)
        {
            if (string.Equals(option.ProfileArgument, profile.Profile, StringComparison.OrdinalIgnoreCase)
                && string.Equals(option.UserDataDir, profile.UserDataDir, StringComparison.OrdinalIgnoreCase))
            {
                return option;
            }
        }

        return null;
    }

    private void SetSelectedDetectedProfileSilently(BrowserProfileOption? option)
    {
        if (ReferenceEquals(SelectedDetectedProfile, option))
        {
            return;
        }

        var wasSuppressed = _suppressSelectionUpdates;
        _suppressSelectionUpdates = true;

        try
        {
            if (Equals(SelectedDetectedProfile, option) && option is not null)
            {
                SelectedDetectedProfile = null;
            }

            SelectedDetectedProfile = option;
        }
        finally
        {
            _suppressSelectionUpdates = wasSuppressed;
        }
    }

    private static bool DetermineIncognito(ProfileEditorViewModel profile)
    {
        if (profile.Incognito)
        {
            return true;
        }

        return ContainsIncognitoArgument(profile.ArgsTemplate);
    }

    private static bool DetermineNewWindow(ProfileEditorViewModel profile)
    {
        if (string.IsNullOrWhiteSpace(profile.ArgsTemplate))
        {
            return false;
        }

        return profile.ArgsTemplate.Contains("--new-window", StringComparison.OrdinalIgnoreCase)
            || profile.ArgsTemplate.Contains("-new-window", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsIncognitoArgument(string? argsTemplate)
    {
        if (string.IsNullOrWhiteSpace(argsTemplate))
        {
            return false;
        }

        return argsTemplate.Contains("--incognito", StringComparison.OrdinalIgnoreCase)
               || argsTemplate.Contains("--inprivate", StringComparison.OrdinalIgnoreCase)
               || argsTemplate.Contains("-private-window", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildArgsTemplate(BrowserInfo? browser, bool newWindow, bool incognito)
    {
        var family = browser?.Family ?? BrowserFamily.Unknown;
        if (family == BrowserFamily.Firefox)
        {
            if (incognito)
            {
                return "-private-window \"{url}\"";
            }

            return newWindow ? "-new-window \"{url}\"" : "\"{url}\"";
        }

        var browserPath = browser?.Path ?? string.Empty;
        var lowerPath = browserPath.ToLowerInvariant();
        var prefix = string.Empty;

        if (incognito)
        {
            prefix = lowerPath.Contains("msedge") ? "--inprivate " : "--incognito ";
        }

        if (newWindow)
        {
            prefix += "--new-window ";
        }

        return $"{prefix}\"{{url}}\"".TrimStart();
    }

    private static string? GetBrowserWorkingDirectory(BrowserInfo browser)
    {
        try
        {
            var directory = Path.GetDirectoryName(browser.Path);
            return string.IsNullOrWhiteSpace(directory) ? null : directory;
        }
        catch
        {
            return null;
        }
    }

    private void OnStatePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ConfigurationState.DefaultProfile)
            || e.PropertyName == nameof(ConfigurationState.IsDefaultEnabled))
        {
            OnPropertyChanged(nameof(SelectedProfileIsDefault));
        }
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        RestoreSelectedProfile();
    }

    private void EnsureDefaultSelections()
    {
        EnsureDefaultBrowserSelection();
        EnsureDefaultProfileSelection();
    }

    private void EnsureDefaultBrowserSelection()
    {
        if (SelectedProfile is null || SelectedProfile.IsAdvanced)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(SelectedProfile.Browser) || SelectedBrowser is not null)
        {
            return;
        }

        var browser = FindDefaultBrowser() ?? Browsers.FirstOrDefault();
        if (browser is not null)
        {
            SelectedBrowser = browser;
        }
    }

    private BrowserInfo? FindDefaultBrowser()
    {
        if (Browsers.Count == 0)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(_systemDefaultBrowserPath))
        {
            var match = Browsers.FirstOrDefault(b =>
                string.Equals(b.Path, _systemDefaultBrowserPath, StringComparison.OrdinalIgnoreCase));
            if (match is not null)
            {
                return match;
            }

            var defaultFileName = Path.GetFileName(_systemDefaultBrowserPath);
            if (!string.IsNullOrWhiteSpace(defaultFileName))
            {
                match = Browsers.FirstOrDefault(b =>
                    string.Equals(Path.GetFileName(b.Path), defaultFileName, StringComparison.OrdinalIgnoreCase));
                if (match is not null)
                {
                    return match;
                }
            }
        }

        return null;
    }

    private void EnsureDefaultProfileSelection()
    {
        if (_ensuringDefaultProfileSelection)
        {
            return;
        }

        _ensuringDefaultProfileSelection = true;
        try
        {
            if (SelectedProfile is null || SelectedProfile.IsAdvanced)
            {
                return;
            }

            if (DetectedProfiles.Count == 0)
            {
                if (SelectedDetectedProfile is not null)
                {
                    SelectedDetectedProfile = null;
                }

                if (SelectedBrowser?.Family == BrowserFamily.Chromium && string.IsNullOrWhiteSpace(SelectedProfile.Profile))
                {
                    SelectedProfile.Profile = "Default";
                }

                return;
            }

            var matchedOption = MatchProfileOption(SelectedProfile);
            if (matchedOption is not null)
            {
                if (!Equals(SelectedDetectedProfile, matchedOption))
                {
                    SelectedDetectedProfile = matchedOption;
                }

                return;
            }

            if (SelectedDetectedProfile is not null && DetectedProfiles.Contains(SelectedDetectedProfile))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(SelectedProfile.Profile) || !string.IsNullOrWhiteSpace(SelectedProfile.UserDataDir))
            {
                SelectedProfile.Profile = null;
                SelectedProfile.UserDataDir = null;
            }

            var option = FindDefaultProfileOption() ?? DetectedProfiles.FirstOrDefault();
            if (option is not null)
            {
                SelectedDetectedProfile = option;
            }
            else if (SelectedBrowser?.Family == BrowserFamily.Chromium && string.IsNullOrWhiteSpace(SelectedProfile.Profile))
            {
                SelectedProfile.Profile = "Default";
            }
        }
        finally
        {
            _ensuringDefaultProfileSelection = false;
        }
    }

    private void RestoreSelectedProfile()
    {
        if (Profiles.Count == 0)
        {
            if (SelectedProfile is not null)
            {
                SelectedProfile = null;
            }
            return;
        }

        ProfileEditorViewModel? target = null;

        if (!string.IsNullOrWhiteSpace(_lastSelectedProfileName))
        {
            target = Profiles.FirstOrDefault(p =>
                string.Equals(p.Name, _lastSelectedProfileName, StringComparison.OrdinalIgnoreCase));
        }

        if (target is null && _lastSelectedProfileIndex >= 0 && _lastSelectedProfileIndex < Profiles.Count)
        {
            target = Profiles[_lastSelectedProfileIndex];
        }

        target ??= Profiles[0];

        if (!ReferenceEquals(target, SelectedProfile))
        {
            SelectedProfile = target;
        }
        else
        {
            SyncSelectionsWithProfile();
            EnsureDefaultSelections();
        }
    }

    private void OnSelectedProfilePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is ProfileEditorViewModel profile
            && e.PropertyName == nameof(ProfileEditorViewModel.Name))
        {
            _lastSelectedProfileName = profile.Name;
        }
    }

    private BrowserProfileOption? FindDefaultProfileOption()
    {
        return DetectedProfiles.FirstOrDefault(option =>
                   string.Equals(option.ProfileArgument, "default-release", StringComparison.OrdinalIgnoreCase))
               ?? DetectedProfiles.FirstOrDefault(option =>
                   string.Equals(option.DisplayName, "default-release", StringComparison.OrdinalIgnoreCase))
               ?? DetectedProfiles.FirstOrDefault(option =>
                   string.Equals(option.ProfileArgument, "Default", StringComparison.OrdinalIgnoreCase))
               ?? DetectedProfiles.FirstOrDefault(option =>
                   string.Equals(option.DisplayName, "Default", StringComparison.OrdinalIgnoreCase));
    }
}
