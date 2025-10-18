using System;
using System.Diagnostics;
using LinkRouter.Settings.Core.Infrastructure;

namespace LinkRouter.Settings.Platform;

internal sealed class WinUILauncherService : ILauncherService
{
    public void OpenFolder(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = path,
            UseShellExecute = true
        };
        Process.Start(startInfo);
    }

    public void OpenFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        };
        Process.Start(startInfo);
    }

    public void OpenUri(string uri)
    {
        if (string.IsNullOrWhiteSpace(uri))
        {
            return;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = uri,
            UseShellExecute = true
        };
        Process.Start(startInfo);
    }
}
