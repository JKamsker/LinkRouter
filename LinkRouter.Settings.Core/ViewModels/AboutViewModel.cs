using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinkRouter.Settings.Services;
using LinkRouter.Settings.Services.Abstractions;

namespace LinkRouter.Settings.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    private readonly IShellService _shellService;
    public string AppName => "LinkRouter Settings";
    public string Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
    public string RepositoryUrl => "https://github.com/jonas/LinkRouter";
    public string VersionLabel => $"Version {Version}";

    public AboutViewModel(IShellService shellService)
    {
        _shellService = shellService;
    }

    [RelayCommand]
    private void OpenRepository() => _shellService.OpenUri(RepositoryUrl);
}
