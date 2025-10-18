using System.Diagnostics;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LinkRouter.Settings.Core.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    public string AppName => "LinkRouter Settings";
    public string Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
    public string RepositoryUrl => "https://github.com/jonas/LinkRouter";
    public string VersionLabel => $"Version {Version}";

    [RelayCommand]
    private void OpenRepository()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = RepositoryUrl,
            UseShellExecute = true
        });
    }
}
