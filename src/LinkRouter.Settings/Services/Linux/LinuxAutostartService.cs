using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using LinkRouter.Settings.Services.Abstractions;

namespace LinkRouter.Settings.Services.Linux;

[SupportedOSPlatform("linux")]
internal sealed class LinuxAutostartService : IAutostartService
{
    private const string DesktopFileName = "linkrouter-settings.desktop";
    private readonly string autostartDirectory;
    private readonly string desktopFilePath;
    private readonly string executablePath;

    public LinuxAutostartService()
    {
        if (!OperatingSystem.IsLinux())
        {
            throw new PlatformNotSupportedException("Linux autostart is only available on Linux platforms.");
        }

        var configHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME")
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");

        autostartDirectory = Path.Combine(configHome, "autostart");
        desktopFilePath = Path.Combine(autostartDirectory, DesktopFileName);
        executablePath = ResolveExecutablePath();
    }

    public bool IsSupported => true;

    public bool IsEnabled()
    {
        if (!File.Exists(desktopFilePath))
        {
            return false;
        }

        try
        {
            var content = File.ReadAllText(desktopFilePath);
            // Check if the file contains our expected Exec line
            var expectedExec = $"Exec={executablePath} --minimized";
            return content.Contains(expectedExec, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    public void SetEnabled(bool enabled)
    {
        if (!enabled)
        {
            if (File.Exists(desktopFilePath))
            {
                File.Delete(desktopFilePath);
            }
            return;
        }

        // Ensure autostart directory exists
        if (!Directory.Exists(autostartDirectory))
        {
            Directory.CreateDirectory(autostartDirectory);
        }

        // Create .desktop file following freedesktop.org Desktop Entry Specification
        var desktopEntry = $@"[Desktop Entry]
Type=Application
Version=1.0
Name=LinkRouter Settings
Comment=Link routing configuration utility
Exec={executablePath} --minimized
Icon=linkrouter
Terminal=false
Categories=Utility;Settings;
X-GNOME-Autostart-enabled=true
";

        File.WriteAllText(desktopFilePath, desktopEntry);
    }

    private static string ResolveExecutablePath()
    {
        var processPath = Environment.ProcessPath;
        if (!string.IsNullOrWhiteSpace(processPath))
        {
            return processPath;
        }

        using var process = Process.GetCurrentProcess();
        var module = process.MainModule?.FileName;
        if (!string.IsNullOrWhiteSpace(module))
        {
            return module!;
        }

        throw new InvalidOperationException("Unable to determine executable path for autostart registration.");
    }
}
