using System;
using CommunityToolkit.Mvvm.ComponentModel;
using LinkRouter;

namespace LinkRouter.Settings.ViewModels;

public sealed class ProfileEditorViewModel : ObservableObject
{
    private string _name;
    private string? _browser;
    private string? _argsTemplate;
    private string? _profile;
    private string? _userDataDir;
    private string? _workingDirectory;

    public ProfileEditorViewModel()
    {
        _name = string.Empty;
    }

    public ProfileEditorViewModel(string name, Profile profile)
    {
        _name = name;
        _browser = profile.browser;
        _argsTemplate = profile.argsTemplate;
        _profile = profile.profile;
        _userDataDir = profile.userDataDir;
        _workingDirectory = profile.workingDirectory;
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
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

    public Profile ToProfile()
    {
        return new Profile(Browser, ArgsTemplate, Profile, UserDataDir, WorkingDirectory);
    }

    public ProfileEditorViewModel Clone()
    {
        return new ProfileEditorViewModel
        {
            _name = _name,
            _browser = _browser,
            _argsTemplate = _argsTemplate,
            _profile = _profile,
            _userDataDir = _userDataDir,
            _workingDirectory = _workingDirectory
        };
    }
}
