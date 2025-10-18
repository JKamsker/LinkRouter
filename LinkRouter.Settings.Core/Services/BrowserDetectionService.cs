using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace LinkRouter.Settings.Core.Services;

public sealed class BrowserDetectionService
{
    private static readonly (string Name, string Executable, string[] RegistryKeys)[] ChromiumBrowsers =
    {
        ("Microsoft Edge", "msedge.exe", new[]
        {
            "Software\\Microsoft\\Windows\\CurrentVersion\\App Paths\\msedge.exe"
        }),
        ("Google Chrome", "chrome.exe", new[]
        {
            "Software\\Microsoft\\Windows\\CurrentVersion\\App Paths\\chrome.exe"
        }),
        ("Brave", "brave.exe", new[]
        {
            "Software\\Microsoft\\Windows\\CurrentVersion\\App Paths\\brave.exe"
        })
    };

    private static readonly (string Name, string[] RegistryKeys)[] FirefoxKeys =
    {
        ("Mozilla Firefox", new[]
        {
            "Software\\Mozilla\\Mozilla Firefox"
        })
    };

    public IReadOnlyList<BrowserInfo> DetectInstalledBrowsers()
    {
        var results = new List<BrowserInfo>();
        foreach (var browser in ChromiumBrowsers)
        {
            var path = ReadExecutablePath(browser.RegistryKeys);
            if (path is not null && File.Exists(path))
            {
                results.Add(new BrowserInfo(browser.Name, path, BrowserFamily.Chromium));
            }
        }

        foreach (var entry in FirefoxKeys)
        {
            var path = ReadFirefoxPath(entry.RegistryKeys);
            if (path is not null && File.Exists(path))
            {
                results.Add(new BrowserInfo(entry.Name, path, BrowserFamily.Firefox));
            }
        }

        return results;
    }

    public IReadOnlyList<string> GetChromiumProfileDirectories()
    {
        var locations = new List<string>();
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var chromeRoot = Path.Combine(localAppData, "Google", "Chrome", "User Data");
        if (Directory.Exists(chromeRoot))
        {
            locations.Add(chromeRoot);
            locations.AddRange(Directory.GetDirectories(chromeRoot));
        }

        var edgeRoot = Path.Combine(localAppData, "Microsoft", "Edge", "User Data");
        if (Directory.Exists(edgeRoot))
        {
            locations.Add(edgeRoot);
            locations.AddRange(Directory.GetDirectories(edgeRoot));
        }

        return locations.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    public IReadOnlyList<FirefoxProfileInfo> GetFirefoxProfiles()
    {
        var profiles = new List<FirefoxProfileInfo>();
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var profileIni = Path.Combine(appData, "Mozilla", "Firefox", "profiles.ini");
        if (!File.Exists(profileIni))
        {
            return profiles;
        }

        FirefoxProfileInfo? current = null;
        foreach (var rawLine in File.ReadAllLines(profileIni))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
            {
                continue;
            }

            if (line.StartsWith("["))
            {
                if (current is not null)
                {
                    profiles.Add(current);
                }
                current = new FirefoxProfileInfo(string.Empty, string.Empty, false);
                continue;
            }

            var parts = line.Split('=', 2);
            if (parts.Length != 2 || current is null)
            {
                continue;
            }

            var key = parts[0].Trim();
            var value = parts[1].Trim();
            switch (key)
            {
                case "Name":
                    current = current with { Name = value };
                    break;
                case "Path":
                    current = current with { RelativePath = value };
                    break;
                case "IsRelative":
                    current = current with { IsRelative = value == "1" };
                    break;
            }
        }

        if (current is not null)
        {
            profiles.Add(current);
        }

        return profiles;
    }

    private static string? ReadExecutablePath(IEnumerable<string> registryKeys)
    {
        // Search HKCU then HKLM
        foreach (var root in new[] { Registry.CurrentUser, Registry.LocalMachine })
        {
            foreach (var keyPath in registryKeys)
            {
                using var key = root.OpenSubKey(keyPath);
                var value = key?.GetValue(null) as string;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
        }

        return null;
    }

    private static string? ReadFirefoxPath(IEnumerable<string> registryKeys)
    {
        foreach (var root in new[] { Registry.CurrentUser, Registry.LocalMachine })
        {
            foreach (var keyPath in registryKeys)
            {
                using var key = root.OpenSubKey(keyPath);
                if (key is null)
                {
                    continue;
                }

                var currentVersion = key.GetValue("CurrentVersion") as string;
                if (string.IsNullOrWhiteSpace(currentVersion))
                {
                    continue;
                }

                using var versionKey = key.OpenSubKey(currentVersion + "\\Main");
                var path = versionKey?.GetValue("PathToExe") as string;
                if (!string.IsNullOrWhiteSpace(path))
                {
                    return path;
                }
            }
        }

        return null;
    }
}

public enum BrowserFamily
{
    Unknown,
    Chromium,
    Firefox
}

public sealed record BrowserInfo
{
    public BrowserInfo()
    {
        Name = string.Empty;
        Path = string.Empty;
        Family = BrowserFamily.Unknown;
    }

    public BrowserInfo(string name, string path, BrowserFamily family)
    {
        Name = name;
        Path = path;
        Family = family;
    }

    public string Name { get; set; }
    public string Path { get; set; }
    public BrowserFamily Family { get; set; }
}

public sealed record FirefoxProfileInfo
{
    public FirefoxProfileInfo()
    {
        Name = string.Empty;
        RelativePath = string.Empty;
        IsRelative = false;
    }

    public FirefoxProfileInfo(string name, string relativePath, bool isRelative)
    {
        Name = name;
        RelativePath = relativePath;
        IsRelative = isRelative;
    }

    public string Name { get; set; }
    public string RelativePath { get; set; }
    public bool IsRelative { get; set; }

    public string GetAbsolutePath()
    {
        if (IsRelative)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "Mozilla", "Firefox", RelativePath.Replace('/', Path.DirectorySeparatorChar));
        }

        return RelativePath;
    }
}
