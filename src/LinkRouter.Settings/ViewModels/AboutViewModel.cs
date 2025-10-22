using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinkRouter.Settings.Services.Abstractions;

namespace LinkRouter.Settings.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    private readonly IShellService _shellService;
    public string AppName => "LinkRouter Settings";
    private static readonly string _buildVersion = ResolveBuildVersion();
    public string Version => _buildVersion;
    public string RepositoryUrl => "https://github.com/jonas/LinkRouter";
    public string VersionLabel => $"Version {Version}";

    public AboutViewModel(IShellService shellService)
    {
        _shellService = shellService;
    }

    private static string ResolveBuildVersion()
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(informationalVersion))
        {
            return informationalVersion;
        }

        var fileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
        if (!string.IsNullOrWhiteSpace(fileVersion))
        {
            return fileVersion;
        }

        var assemblyVersion = assembly.GetName().Version;
        if (assemblyVersion is not null)
        {
            return assemblyVersion.ToString();
        }

        return "1.0.0";
    }

    [RelayCommand]
    private void OpenRepository()
    {
        _shellService.OpenUri(RepositoryUrl);
    }
}
