using System;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace LinkRouter.Settings.Services.Windows.BrowserDetection;

internal static class DefaultBrowserResolver
{
    public static string? GetDefaultBrowserExecutablePath()
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
                var command = ReadCommandFromProgId(progId);
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

    public static string? GetDefaultBrowserProgId(string scheme = "http")
    {
        if (!OperatingSystem.IsWindows())
        {
            return null;
        }

        try
        {
            return ReadUserChoiceProgId(scheme);
        }
        catch
        {
            return null;
        }
    }

    public static (bool isDefault, string? currentProgId, string? currentPath) CheckIfLinkRouterIsDefault(string? expectedLinkRouterPath = null)
    {
        if (!OperatingSystem.IsWindows())
        {
            return (false, null, null);
        }

        try
        {
            var httpProgId = ReadUserChoiceProgId("http");
            var httpsProgId = ReadUserChoiceProgId("https");

            // Both http and https should be set to LinkRouterURL
            if (httpProgId != "LinkRouterURL" || httpsProgId != "LinkRouterURL")
            {
                return (false, httpProgId ?? httpsProgId, null);
            }

            // If we have an expected path, verify it matches the registered path
            if (!string.IsNullOrWhiteSpace(expectedLinkRouterPath))
            {
                var command = ReadCommandFromProgId("LinkRouterURL");
                var registeredPath = ExtractExecutablePath(command);

                if (!string.IsNullOrWhiteSpace(registeredPath) &&
                    !string.Equals(registeredPath, expectedLinkRouterPath, StringComparison.OrdinalIgnoreCase))
                {
                    return (false, "LinkRouterURL", registeredPath);
                }
            }

            return (true, "LinkRouterURL", null);
        }
        catch
        {
            return (false, null, null);
        }
    }

    [SupportedOSPlatform("windows")]
    private static string? ReadUserChoiceProgId(string scheme)
    {
        using var key = Registry.CurrentUser.OpenSubKey($"Software\\Microsoft\\Windows\\Shell\\Associations\\UrlAssociations\\{scheme}\\UserChoice");
        var value = key?.GetValue("ProgId") as string;
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    [SupportedOSPlatform("windows")]
    private static string? ReadCommandFromProgId(string? progId)
    {
        if (string.IsNullOrWhiteSpace(progId))
        {
            return null;
        }

        using var key = Registry.ClassesRoot.OpenSubKey($"{progId}\\shell\\open\\command");
        return key?.GetValue(null) as string;
    }

    [SupportedOSPlatform("windows")]
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
