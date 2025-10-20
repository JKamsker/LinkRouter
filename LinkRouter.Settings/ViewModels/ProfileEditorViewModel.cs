using CommunityToolkit.Mvvm.ComponentModel;
using LinkRouter;

namespace LinkRouter.Settings.ViewModels;

public sealed partial class ProfileEditorViewModel : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

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
    private bool isDefault;

    private bool _isAdvanced;

    public ProfileEditorViewModel()
    {
    }

    public ProfileEditorViewModel(string name, Profile profile)
    {
        Name = name;
        Browser = profile.browser;
        ArgsTemplate = profile.argsTemplate;
        Profile = profile.profile;
        UserDataDir = profile.userDataDir;
        WorkingDirectory = profile.workingDirectory;
    }

    public bool IsAdvanced
    {
        get => _isAdvanced;
        set
        {
            if (!value && _isAdvanced)
            {
                return;
            }

            SetProperty(ref _isAdvanced, value);
        }
    }

    public void InitializeAdvanced(bool isAdvanced)
    {
        _isAdvanced = isAdvanced;
        OnPropertyChanged(nameof(IsAdvanced));
    }

    public void SetDefaultFlag(bool isDefault)
    {
        IsDefault = isDefault;
    }

    public Profile ToProfile()
    {
        return new Profile(Browser, ArgsTemplate, Profile, UserDataDir, WorkingDirectory);
    }

    public ProfileEditorViewModel Clone()
    {
        var clone = new ProfileEditorViewModel
        {
            IsDefault = IsDefault
        };

        clone.InitializeAdvanced(_isAdvanced);
        clone.Name = Name;
        clone.Browser = Browser;
        clone.ArgsTemplate = ArgsTemplate;
        clone.Profile = Profile;
        clone.UserDataDir = UserDataDir;
        clone.WorkingDirectory = WorkingDirectory;

        return clone;
    }
}
