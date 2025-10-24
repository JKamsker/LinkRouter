using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using LinkRouter.Settings.Services.Abstractions;

namespace LinkRouter.Settings.Services.Linux;

[SupportedOSPlatform("linux")]
internal sealed class LinuxDefaultAppRegistrar : IDefaultAppRegistrar
{
    private const string DesktopFileName = "linkrouter.desktop";
    private const string AppName = "LinkRouter";

    public void RegisterPerUser(string? executablePath = null, string? appUserModelId = null)
    {
        var launcherPath = ResolveExecutablePath(executablePath);

        // Get XDG data directory for desktop files
        var dataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME")
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share");

        var applicationsDir = Path.Combine(dataHome, "applications");
        var desktopFilePath = Path.Combine(applicationsDir, DesktopFileName);

        // Ensure applications directory exists
        if (!Directory.Exists(applicationsDir))
        {
            Directory.CreateDirectory(applicationsDir);
        }

        // Create .desktop file
        var desktopEntry = $@"[Desktop Entry]
Version=1.0
Type=Application
Name={AppName}
Comment=Routes links to configured browsers and profiles
Exec=""{launcherPath}"" %u
Icon=linkrouter
Terminal=false
Categories=Network;WebBrowser;
MimeType=x-scheme-handler/http;x-scheme-handler/https;
StartupNotify=false
";

        File.WriteAllText(desktopFilePath, desktopEntry);
        Console.WriteLine($"Created desktop file: {desktopFilePath}");

        // Update desktop database
        TryUpdateDesktopDatabase(applicationsDir);

        // Try to register as default handler using xdg-settings
        var registeredHttp = TryRegisterSchemeHandler("http", DesktopFileName);
        var registeredHttps = TryRegisterSchemeHandler("https", DesktopFileName);

        if (registeredHttp && registeredHttps)
        {
            Console.WriteLine("LinkRouter registered as default browser via xdg-settings.");
        }
        else
        {
            Console.WriteLine("Automatic registration failed. Opening system settings for manual configuration.");
            TryOpenSystemSettings();
        }
    }

    public void UnregisterPerUser()
    {
        try
        {
            var dataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME")
                ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share");

            var desktopFilePath = Path.Combine(dataHome, "applications", DesktopFileName);

            if (File.Exists(desktopFilePath))
            {
                File.Delete(desktopFilePath);
                Console.WriteLine($"Removed desktop file: {desktopFilePath}");

                // Update desktop database after removal
                TryUpdateDesktopDatabase(Path.Combine(dataHome, "applications"));
            }

            Console.WriteLine("LinkRouter unregistered. You may need to select a new default browser in system settings.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to unregister: {ex.Message}");
        }
    }

    private static void TryUpdateDesktopDatabase(string applicationsDir)
    {
        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "update-desktop-database",
                Arguments = $"\"{applicationsDir}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            process?.WaitForExit(5000);
        }
        catch
        {
            // update-desktop-database is optional, continue without it
        }
    }

    private static bool TryRegisterSchemeHandler(string scheme, string desktopFileName)
    {
        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "xdg-settings",
                Arguments = $"set default-url-scheme-handler {scheme} {desktopFileName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            if (process == null)
            {
                return false;
            }

            process.WaitForExit(10000);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private static void TryOpenSystemSettings()
    {
        // Try various desktop environment settings commands
        var settingsCommands = new[]
        {
            "gnome-control-center default-apps",           // GNOME
            "systemsettings5 kcm_filetypes",               // KDE Plasma
            "xfce4-settings-manager",                       // XFCE
            "unity-control-center default-apps",            // Unity
        };

        foreach (var command in settingsCommands)
        {
            var parts = command.Split(' ', 2);
            var executable = parts[0];
            var arguments = parts.Length > 1 ? parts[1] : string.Empty;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = executable,
                    Arguments = arguments,
                    UseShellExecute = true
                });
                return; // Successfully opened settings
            }
            catch
            {
                // Try next command
            }
        }

        Console.WriteLine("Could not automatically open system settings. Please manually set LinkRouter as your default browser in your system settings.");
    }

    private static string ResolveExecutablePath(string? executablePath)
    {
        if (!string.IsNullOrWhiteSpace(executablePath))
        {
            var fullPath = Path.GetFullPath(executablePath);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("LinkRouter.Launcher executable not found.", fullPath);
            }

            return fullPath;
        }

        // Try to find the launcher executable
        var searchPaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "bin", "LinkRouter.Launcher"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "LinkRouter", "bin", "LinkRouter.Launcher"),
            "/usr/local/bin/LinkRouter.Launcher",
            "/opt/LinkRouter/LinkRouter.Launcher",
        };

        foreach (var path in searchPaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        throw new FileNotFoundException("LinkRouter.Launcher executable not found in standard locations.");
    }
}
