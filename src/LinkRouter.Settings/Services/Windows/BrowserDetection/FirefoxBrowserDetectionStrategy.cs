using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using LinkRouter.Settings.Services.Abstractions;
using Microsoft.Win32;

namespace LinkRouter.Settings.Services.Windows.BrowserDetection;

internal sealed class FirefoxBrowserDetectionStrategy : IBrowserDetectionStrategy
{
    private static readonly (string Name, string[] RegistryKeys)[] FirefoxKeys =
    {
        ("Mozilla Firefox", new[]
        {
            "Software\\Mozilla\\Mozilla Firefox"
        })
    };

    public BrowserFamily Family => BrowserFamily.Firefox;

    [SupportedOSPlatform("windows")]
    public IEnumerable<BrowserInfo> DetectInstalledBrowsers()
    {
        foreach (var entry in FirefoxKeys)
        {
            var path = ReadFirefoxPath(entry.RegistryKeys);
            if (path is not null && File.Exists(path))
            {
                yield return new BrowserInfo(entry.Name, path, BrowserFamily.Firefox);
            }
        }
    }

    public IReadOnlyList<BrowserProfileOption> GetProfileOptions(BrowserInfo browser)
    {
        if (browser.Family != BrowserFamily.Firefox)
        {
            return Array.Empty<BrowserProfileOption>();
        }

        var profiles = new List<BrowserProfileOption>();
        foreach (var profile in EnumerateFirefoxProfiles())
        {
            if (string.IsNullOrWhiteSpace(profile.Name) && string.IsNullOrWhiteSpace(profile.RelativePath))
            {
                continue;
            }

            profiles.Add(new BrowserProfileOption(
                string.IsNullOrWhiteSpace(profile.Name) ? profile.RelativePath : profile.Name,
                profile.Name,
                null));
        }

        profiles.Sort((a, b) =>
        {
            if (string.Equals(a.ProfileArgument, "default-release", StringComparison.OrdinalIgnoreCase))
            {
                return -1;
            }

            if (string.Equals(b.ProfileArgument, "default-release", StringComparison.OrdinalIgnoreCase))
            {
                return 1;
            }

            return string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase);
        });

        return profiles;
    }

    [SupportedOSPlatform("windows")]
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

    private static IReadOnlyList<FirefoxProfileInfo> EnumerateFirefoxProfiles()
    {
        var profiles = new List<FirefoxProfileInfo>();
        try
        {
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
                if (string.IsNullOrEmpty(line) || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                if (line.StartsWith("[", StringComparison.Ordinal))
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
        }
        catch
        {
            // Ignore parsing errors.
        }

        return profiles;
    }

    private sealed record FirefoxProfileInfo(string Name, string RelativePath, bool IsRelative);
}
