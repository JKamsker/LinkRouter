using System;
using System.Collections.ObjectModel;
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
    private readonly ConfigurationState _state = AppServices.ConfigurationState;
    private readonly BrowserDetectionService _detector = AppServices.BrowserDetectionService;

    [ObservableProperty]
    private ProfileEditorViewModel? _selectedProfile;

    [ObservableProperty]
    private BrowserInfo? _selectedBrowser;

    [ObservableProperty]
    private BrowserProfileOption? _selectedDetectedProfile;

    [ObservableProperty]
    private bool _selectedProfileNewWindow;

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

    public ProfilesViewModel()
    {
        RefreshDetections();
        _state.PropertyChanged += OnStatePropertyChanged;
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
            SyncSelectionsWithProfile();
        }
        catch (Exception ex)
        {
            DetectionError = ex.Message;
        }
    }

    partial void OnDetectionErrorChanged(string? value) => OnPropertyChanged(nameof(HasDetectionError));

    partial void OnSelectedProfileChanged(ProfileEditorViewModel? value)
    {
        SyncSelectionsWithProfile();
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
            SelectedProfile.ArgsTemplate = BuildArgsTemplate(value, SelectedProfileNewWindow);
            SelectedProfile.WorkingDirectory = GetBrowserWorkingDirectory(value);
            SelectedProfile.Profile = null;
            SelectedProfile.UserDataDir = null;
        }

        UpdateDetectedProfiles(value);
        SelectedDetectedProfile = null;
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

        SelectedProfile.ArgsTemplate = BuildArgsTemplate(SelectedBrowser, value);
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
                SelectedDetectedProfile = null;
                SelectedProfileNewWindow = false;
            }
            else
            {
                SelectedBrowser = FindBrowserByPath(SelectedProfile.Browser);
                UpdateDetectedProfiles(SelectedBrowser);
                SelectedDetectedProfile = MatchProfileOption(SelectedProfile);
                SelectedProfileNewWindow = DetermineNewWindow(SelectedProfile);
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
        DetectedProfiles.Clear();
        if (browser is null)
        {
            return;
        }

        foreach (var option in _detector.GetBrowserProfileOptions(browser))
        {
            DetectedProfiles.Add(option);
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

    private static bool DetermineNewWindow(ProfileEditorViewModel profile)
    {
        if (string.IsNullOrWhiteSpace(profile.ArgsTemplate))
        {
            return false;
        }

        return profile.ArgsTemplate.Contains("--new-window", StringComparison.OrdinalIgnoreCase)
            || profile.ArgsTemplate.Contains("-new-window", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildArgsTemplate(BrowserInfo? browser, bool newWindow)
    {
        var family = browser?.Family ?? BrowserFamily.Unknown;
        return family switch
        {
            BrowserFamily.Firefox => newWindow ? "-new-window \"{url}\"" : "\"{url}\"",
            BrowserFamily.Chromium => newWindow ? "--new-window \"{url}\"" : "\"{url}\"",
            _ => newWindow ? "--new-window \"{url}\"" : "\"{url}\""
        };
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
}
