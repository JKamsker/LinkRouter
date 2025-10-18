using System;
using System.Diagnostics;
using LinkRouter.Settings.Core.Infrastructure;

namespace LinkRouter.Settings.Avalonia.Platform;

internal sealed class AvaloniaLauncherService : ILauncherService
{
    public void OpenFolder(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        if (OperatingSystem.IsWindows())
        {
            Process.Start(new ProcessStartInfo("explorer.exe", path) { UseShellExecute = true });
        }
        else if (OperatingSystem.IsMacOS())
        {
            Process.Start(new ProcessStartInfo("open", path) { UseShellExecute = true });
        }
        else
        {
            Process.Start(new ProcessStartInfo("xdg-open", path) { UseShellExecute = false });
        }
    }

    public void OpenFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        if (OperatingSystem.IsWindows())
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        else if (OperatingSystem.IsMacOS())
        {
            Process.Start(new ProcessStartInfo("open", path) { UseShellExecute = true });
        }
        else
        {
            Process.Start(new ProcessStartInfo("xdg-open", path) { UseShellExecute = false });
        }
    }

    public void OpenUri(string uri)
    {
        if (string.IsNullOrWhiteSpace(uri))
        {
            return;
        }

        if (OperatingSystem.IsWindows())
        {
            Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
        }
        else if (OperatingSystem.IsMacOS())
        {
            Process.Start(new ProcessStartInfo("open", uri) { UseShellExecute = true });
        }
        else
        {
            Process.Start(new ProcessStartInfo("xdg-open", uri) { UseShellExecute = false });
        }
    }
}
