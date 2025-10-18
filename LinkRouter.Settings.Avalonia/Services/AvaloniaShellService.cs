using System;
using System.Diagnostics;
using LinkRouter.Settings.Services.Abstractions;

namespace LinkRouter.Settings.Avalonia.Services;

internal sealed class AvaloniaShellService : IShellService
{
    public void OpenFolder(string path) => Launch(path);

    public void OpenFile(string path) => Launch(path);

    public void OpenUri(string uri) => Launch(uri);

    private static void Launch(string target)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = target,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to launch '{target}': {ex.Message}");
        }
    }
}
