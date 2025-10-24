using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using LinkRouter.Settings.Services.Abstractions;

namespace LinkRouter.Settings.Services.Linux.BrowserDetection;

[SupportedOSPlatform("linux")]
internal sealed class LinuxBrowserDetectionStrategy : IBrowserDetectionStrategy
{
    private static readonly (string Name, string DesktopFilePattern, BrowserFamily Family)[] KnownBrowsers =
    {
        ("Google Chrome", "google-chrome*.desktop", BrowserFamily.Chromium),
        ("Chromium", "chromium*.desktop", BrowserFamily.Chromium),
        ("Microsoft Edge", "microsoft-edge*.desktop", BrowserFamily.Chromium),
        ("Brave", "brave-browser*.desktop", BrowserFamily.Chromium),
        ("Mozilla Firefox", "firefox*.desktop", BrowserFamily.Firefox),
    };

    public BrowserFamily Family => BrowserFamily.Unknown; // This strategy detects all families

    [SupportedOSPlatform("linux")]
    public IEnumerable<BrowserInfo> DetectInstalledBrowsers()
    {
        var applicationsDirs = GetApplicationsDirectories();
        var foundBrowsers = new Dictionary<string, BrowserInfo>(StringComparer.OrdinalIgnoreCase);

        foreach (var dir in applicationsDirs)
        {
            if (!Directory.Exists(dir))
            {
                continue;
            }

            foreach (var (name, pattern, family) in KnownBrowsers)
            {
                foreach (var desktopFile in Directory.GetFiles(dir, pattern))
                {
                    var browserInfo = ParseDesktopFile(desktopFile, name, family);
                    if (browserInfo != null && !foundBrowsers.ContainsKey(browserInfo.Path))
                    {
                        foundBrowsers[browserInfo.Path] = browserInfo;
                    }
                }
            }
        }

        return foundBrowsers.Values;
    }

    public IReadOnlyList<BrowserProfileOption> GetProfileOptions(BrowserInfo browser)
    {
        // Profile detection for Linux browsers
        // For now, return empty - full profile support can be added later
        // Similar to Windows implementation, we'd need to check:
        // - Firefox: ~/.mozilla/firefox/profiles.ini
        // - Chrome: ~/.config/google-chrome/ (Local State file)
        // - Chromium: ~/.config/chromium/
        // - Brave: ~/.config/BraveSoftware/Brave-Browser/
        // - Edge: ~/.config/microsoft-edge/

        return Array.Empty<BrowserProfileOption>();
    }

    private static IEnumerable<string> GetApplicationsDirectories()
    {
        yield return "/usr/share/applications";
        yield return "/usr/local/share/applications";

        var dataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME")
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share");

        yield return Path.Combine(dataHome, "applications");

        // XDG_DATA_DIRS
        var dataDirs = Environment.GetEnvironmentVariable("XDG_DATA_DIRS");
        if (!string.IsNullOrWhiteSpace(dataDirs))
        {
            foreach (var dir in dataDirs.Split(':', StringSplitOptions.RemoveEmptyEntries))
            {
                yield return Path.Combine(dir, "applications");
            }
        }
    }

    private static BrowserInfo? ParseDesktopFile(string desktopFilePath, string defaultName, BrowserFamily family)
    {
        try
        {
            var lines = File.ReadAllLines(desktopFilePath);
            string? exec = null;
            string? name = null;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                if (trimmed.StartsWith("Exec=", StringComparison.OrdinalIgnoreCase))
                {
                    exec = trimmed.Substring(5).Trim();
                }
                else if (trimmed.StartsWith("Name=", StringComparison.OrdinalIgnoreCase))
                {
                    name = trimmed.Substring(5).Trim();
                }

                if (exec != null && name != null)
                {
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(exec))
            {
                return null;
            }

            // Extract the executable path from the Exec line
            // Desktop files can have arguments like "google-chrome %U"
            var executablePath = ExtractExecutablePath(exec);
            if (string.IsNullOrWhiteSpace(executablePath))
            {
                return null;
            }

            return new BrowserInfo(name ?? defaultName, executablePath, family);
        }
        catch
        {
            return null;
        }
    }

    private static string? ExtractExecutablePath(string execLine)
    {
        // Remove field codes like %U, %F, etc.
        execLine = Regex.Replace(execLine, @"%[uUfFdDnNickvm]", string.Empty).Trim();

        // Handle quoted paths
        if (execLine.StartsWith('"'))
        {
            var endQuote = execLine.IndexOf('"', 1);
            if (endQuote > 0)
            {
                return execLine.Substring(1, endQuote - 1);
            }
        }

        // Take the first token (executable)
        var tokens = execLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0)
        {
            return null;
        }

        var executable = tokens[0];

        // If it's not an absolute path, try to find it in PATH
        if (!Path.IsPathRooted(executable))
        {
            return FindExecutableInPath(executable);
        }

        return File.Exists(executable) ? executable : null;
    }

    private static string? FindExecutableInPath(string executable)
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathEnv))
        {
            return null;
        }

        foreach (var dir in pathEnv.Split(':', StringSplitOptions.RemoveEmptyEntries))
        {
            var fullPath = Path.Combine(dir, executable);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        return null;
    }
}
