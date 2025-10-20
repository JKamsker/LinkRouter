using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Win32;

namespace LinkRouter.Settings.Services.BrowserDetection;

internal sealed class ChromiumBrowserDetectionStrategy : IBrowserDetectionStrategy
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

    public BrowserFamily Family => BrowserFamily.Chromium;

    public IEnumerable<BrowserInfo> DetectInstalledBrowsers()
    {
        foreach (var browser in ChromiumBrowsers)
        {
            var path = ReadExecutablePath(browser.RegistryKeys);
            if (path is not null && File.Exists(path))
            {
                yield return new BrowserInfo(browser.Name, path, BrowserFamily.Chromium);
            }
        }
    }

    public IReadOnlyList<BrowserProfileOption> GetProfileOptions(BrowserInfo browser)
    {
        if (browser.Family != BrowserFamily.Chromium)
        {
            return Array.Empty<BrowserProfileOption>();
        }

        var options = new List<BrowserProfileOption>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var root in EnumerateChromiumRoots(browser))
        {
            if (!Directory.Exists(root))
            {
                continue;
            }

            var profiles = ReadChromiumProfiles(root);
            if (profiles.Count == 0)
            {
                profiles = EnumerateChromiumProfilesFallback(root);
            }

            foreach (var profile in profiles)
            {
                var key = $"{profile.DirectoryName}|{root}";
                if (seen.Add(key))
                {
                    options.Add(new BrowserProfileOption(profile.DisplayName, profile.DirectoryName, root));
                }
            }
        }

        return options;
    }

    private static string? ReadExecutablePath(IEnumerable<string> registryKeys)
    {
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

    private static IEnumerable<string> EnumerateChromiumRoots(BrowserInfo browser)
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localAppData))
        {
            yield break;
        }

        var executable = Path.GetFileName(browser.Path) ?? string.Empty;

        if (browser.Name.Contains("Chrome", StringComparison.OrdinalIgnoreCase) || executable.Contains("chrome", StringComparison.OrdinalIgnoreCase))
        {
            yield return Path.Combine(localAppData, "Google", "Chrome", "User Data");
        }

        if (browser.Name.Contains("Edge", StringComparison.OrdinalIgnoreCase) || executable.Contains("msedge", StringComparison.OrdinalIgnoreCase))
        {
            yield return Path.Combine(localAppData, "Microsoft", "Edge", "User Data");
        }

        if (browser.Name.Contains("Brave", StringComparison.OrdinalIgnoreCase) || executable.Contains("brave", StringComparison.OrdinalIgnoreCase))
        {
            yield return Path.Combine(localAppData, "BraveSoftware", "Brave-Browser", "User Data");
        }
    }

    private static IReadOnlyList<ChromiumProfileEntry> ReadChromiumProfiles(string root)
    {
        var results = new List<ChromiumProfileEntry>();
        try
        {
            var localStatePath = Path.Combine(root, "Local State");
            if (!File.Exists(localStatePath))
            {
                return results;
            }

            using var stream = File.OpenRead(localStatePath);
            using var document = JsonDocument.Parse(stream);
            if (!document.RootElement.TryGetProperty("profile", out var profileElement))
            {
                return results;
            }

            if (!profileElement.TryGetProperty("info_cache", out var infoCache) || infoCache.ValueKind != JsonValueKind.Object)
            {
                return results;
            }

            foreach (var property in infoCache.EnumerateObject())
            {
                if (!ShouldIncludeChromiumProfile(property.Value))
                {
                    continue;
                }

                var directoryName = property.Name;
                if (string.IsNullOrWhiteSpace(directoryName))
                {
                    continue;
                }

                var directoryPath = Path.Combine(root, directoryName);
                if (!Directory.Exists(directoryPath))
                {
                    continue;
                }

                var displayName = property.Value.TryGetProperty("name", out var nameElement)
                    && nameElement.ValueKind == JsonValueKind.String
                    ? nameElement.GetString()
                    : null;

                results.Add(new ChromiumProfileEntry(
                    directoryName,
                    string.IsNullOrWhiteSpace(displayName) ? directoryName : displayName!));
            }
        }
        catch
        {
            // Ignore parse errors; we'll fall back to directory scanning.
        }

        return results;
    }

    private static IReadOnlyList<ChromiumProfileEntry> EnumerateChromiumProfilesFallback(string root)
    {
        var results = new List<ChromiumProfileEntry>();
        try
        {
            foreach (var directory in Directory.GetDirectories(root))
            {
                var name = Path.GetFileName(directory);
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                if (string.Equals(name, "System Profile", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!File.Exists(Path.Combine(directory, "Preferences")))
                {
                    continue;
                }

                results.Add(new ChromiumProfileEntry(name, name));
            }
        }
        catch
        {
            // Ignore directory enumeration errors.
        }

        return results;
    }

    private static bool ShouldIncludeChromiumProfile(JsonElement profileElement)
    {
        if (profileElement.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (profileElement.TryGetProperty("is_deleted", out var deletedElement)
            && deletedElement.ValueKind == JsonValueKind.True)
        {
            return false;
        }

        if (profileElement.TryGetProperty("is_omitted_from_profile_list", out var omittedElement)
            && omittedElement.ValueKind == JsonValueKind.True)
        {
            return false;
        }

        if (profileElement.TryGetProperty("profile_type", out var typeElement) && typeElement.ValueKind == JsonValueKind.String)
        {
            var typeValue = typeElement.GetString();
            if (!string.IsNullOrWhiteSpace(typeValue)
                && typeValue.Equals("System", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private sealed record ChromiumProfileEntry(string DirectoryName, string DisplayName);
}
