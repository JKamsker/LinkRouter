using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    private string? _detectionError;

    public ObservableCollection<ProfileEditorViewModel> Profiles => _state.Profiles;
    public ObservableCollection<BrowserInfo> Browsers { get; } = new();
    public ObservableCollection<string> ChromiumProfileDirectories { get; } = new();
    public ObservableCollection<FirefoxProfileInfo> FirefoxProfiles { get; } = new();

    public bool HasDetectionError => !string.IsNullOrWhiteSpace(DetectionError);

    public ProfilesViewModel()
    {
        RefreshDetections();
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

            ChromiumProfileDirectories.Clear();
            foreach (var path in _detector.GetChromiumProfileDirectories())
            {
                ChromiumProfileDirectories.Add(path);
            }

            FirefoxProfiles.Clear();
            foreach (var profile in _detector.GetFirefoxProfiles())
            {
                FirefoxProfiles.Add(profile);
            }

            DetectionError = null;
        }
        catch (Exception ex)
        {
            DetectionError = ex.Message;
        }
    }

    partial void OnDetectionErrorChanged(string? value) => OnPropertyChanged(nameof(HasDetectionError));

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
    private void UseDetectedBrowser(BrowserInfo? browser)
    {
        if (browser is null || SelectedProfile is null)
        {
            return;
        }

        SelectedProfile.Browser = browser.Path;
    }

    [RelayCommand]
    private void UseChromiumProfile(string? path)
    {
        if (SelectedProfile is null || string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        SelectedProfile.UserDataDir = path;
    }

    [RelayCommand]
    private void UseFirefoxProfile(FirefoxProfileInfo? profile)
    {
        if (SelectedProfile is null || profile is null)
        {
            return;
        }

        SelectedProfile.Profile = profile.Name;
        SelectedProfile.UserDataDir = null;
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
}
