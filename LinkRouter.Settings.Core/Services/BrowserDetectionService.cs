using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Win32;

namespace LinkRouter.Settings.Services;

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

    public IReadOnlyList<BrowserProfileOption> GetBrowserProfileOptions(BrowserInfo browser)
    {
        try
        {
            return browser.Family switch
            {
                BrowserFamily.Chromium => GetChromiumProfileOptions(browser),
                BrowserFamily.Firefox => GetFirefoxProfileOptions(),
                _ => Array.Empty<BrowserProfileOption>()
            };
        }
        catch
        {
            return Array.Empty<BrowserProfileOption>();
        }
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

    private IReadOnlyList<BrowserProfileOption> GetChromiumProfileOptions(BrowserInfo browser)
    {
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

    private static IEnumerable<string> EnumerateChromiumRoots(BrowserInfo browser)
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localAppData))
        {
            yield break;
        }

        var executable = Path.GetFileName(browser.Path) ?? string.Empty;
        executable = executable.ToLowerInvariant();

        if (browser.Name.Contains("Chrome", StringComparison.OrdinalIgnoreCase) || executable.Contains("chrome"))
        {
            yield return Path.Combine(localAppData, "Google", "Chrome", "User Data");
        }

        if (browser.Name.Contains("Edge", StringComparison.OrdinalIgnoreCase) || executable.Contains("msedge"))
        {
            yield return Path.Combine(localAppData, "Microsoft", "Edge", "User Data");
        }

        if (browser.Name.Contains("Brave", StringComparison.OrdinalIgnoreCase) || executable.Contains("brave"))
        {
            yield return Path.Combine(localAppData, "BraveSoftware", "Brave-Browser", "User Data");
        }
    }

    private IReadOnlyList<BrowserProfileOption> GetFirefoxProfileOptions()
    {
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

        return profiles;
    }

    private IReadOnlyList<FirefoxProfileInfo> EnumerateFirefoxProfiles()
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

    public string? GetDefaultBrowserExecutablePath()
    {
        if (!OperatingSystem.IsWindows())
        {
            return null;
        }

        try
        {
            var progId = ReadUserChoiceProgId("http") ?? ReadUserChoiceProgId("https");
            if (!string.IsNullOrWhiteSpace(progId))
            {
                var command = ReadCommandFromProgId(progId!);
                var path = ExtractExecutablePath(command);
                if (!string.IsNullOrWhiteSpace(path))
                {
                    return path;
                }
            }

            var fallbackCommand = ReadStartMenuInternetCommand();
            var fallbackPath = ExtractExecutablePath(fallbackCommand);
            return string.IsNullOrWhiteSpace(fallbackPath) ? null : fallbackPath;
        }
        catch
        {
            return null;
        }
    }

    private sealed record ChromiumProfileEntry(string DirectoryName, string DisplayName);

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

        var profileTypeAllowed = true;
        if (profileElement.TryGetProperty("profile_type", out var typeElement) && typeElement.ValueKind == JsonValueKind.String)
        {
            var typeValue = typeElement.GetString();
            if (!string.IsNullOrWhiteSpace(typeValue)
                && typeValue.Equals("System", StringComparison.OrdinalIgnoreCase))
            {
                profileTypeAllowed = false;
            }
        }

        return profileTypeAllowed;
    }

    private static string? ReadUserChoiceProgId(string scheme)
    {
        using var key = Registry.CurrentUser.OpenSubKey($"Software\\Microsoft\\Windows\\Shell\\Associations\\UrlAssociations\\{scheme}\\UserChoice");
        var value = key?.GetValue("ProgId") as string;
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string? ReadCommandFromProgId(string progId)
    {
        using var key = Registry.ClassesRoot.OpenSubKey($"{progId}\\shell\\open\\command");
        return key?.GetValue(null) as string;
    }

    private static string? ReadStartMenuInternetCommand()
    {
        using var userKey = Registry.CurrentUser.OpenSubKey("Software\\Clients\\StartMenuInternet");
        var userValue = userKey?.GetValue(null) as string;
        if (!string.IsNullOrWhiteSpace(userValue))
        {
            var command = ReadCommandFromProgId(userValue);
            if (!string.IsNullOrWhiteSpace(command))
            {
                return command;
            }
        }

        using var machineKey = Registry.LocalMachine.OpenSubKey("Software\\Clients\\StartMenuInternet");
        var machineValue = machineKey?.GetValue(null) as string;
        if (!string.IsNullOrWhiteSpace(machineValue))
        {
            return ReadCommandFromProgId(machineValue);
        }

        return null;
    }

    private static string? ExtractExecutablePath(string? command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return null;
        }

        var trimmed = command.Trim();
        if (trimmed.StartsWith("\"", StringComparison.Ordinal))
        {
            var endQuote = trimmed.IndexOf('"', 1);
            if (endQuote > 1)
            {
                var candidate = trimmed.Substring(1, endQuote - 1);
                candidate = Environment.ExpandEnvironmentVariables(candidate);
                return candidate;
            }
        }

        var parts = trimmed.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 0)
        {
            var candidate = Environment.ExpandEnvironmentVariables(parts[0]);
            if (candidate.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                return candidate;
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

public sealed record BrowserProfileOption(string DisplayName, string? ProfileArgument, string? UserDataDir);
