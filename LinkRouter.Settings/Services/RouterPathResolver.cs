using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using LinkRouter.Settings.Services.Abstractions;

namespace LinkRouter.Settings.Services;

internal sealed class RouterPathResolver : IRouterPathResolver
{
    public bool TryGetRouterExecutable([NotNullWhen(true)] out string? path)
    {
        var executableName = OperatingSystem.IsWindows() ? "LinkRouter.Launcher.exe" : "LinkRouter.Launcher";

#if DEBUG
        if (Debugger.IsAttached && TryResolveDevelopmentBuild(executableName, out path))
        {
            return true;
        }
#endif

        var installedPath = GetInstalledPath(executableName);
        if (File.Exists(installedPath))
        {
            path = installedPath;
            return true;
        }

#if DEBUG
        if (!Debugger.IsAttached && TryResolveDevelopmentBuild(executableName, out path))
        {
            return true;
        }
#endif

        path = null;
        return false;
    }

    private static string GetInstalledPath(string executableName)
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "LinkRouter", executableName);
    }

#if DEBUG
    private static bool TryResolveDevelopmentBuild(string executableName, [NotNullWhen(true)] out string? path)
    {
        var baseDirectory = AppContext.BaseDirectory;
        var frameworkDirectory = new DirectoryInfo(baseDirectory);
        var configurationDirectory = frameworkDirectory.Parent;
        if (configurationDirectory?.Parent?.Parent == null)
        {
            path = null;
            return false;
        }

        var launcherBinRoot = Path.GetFullPath(
            Path.Combine(
                baseDirectory,
                "..",
                "..",
                "..",
                "..",
                "LinkRouter.Launcher",
                "bin"));

        if (!Directory.Exists(launcherBinRoot))
        {
            path = null;
            return false;
        }

        // Prefer the same configuration as the settings build (Debug/Release/etc.)
        if (TryResolveFromConfiguration(launcherBinRoot, configurationDirectory.Name, executableName, out path))
        {
            return true;
        }

        // Fall back to any other configuration that was built.
        foreach (var configurationPath in Directory.EnumerateDirectories(launcherBinRoot))
        {
            var configurationName = Path.GetFileName(configurationPath);
            if (string.Equals(configurationName, configurationDirectory.Name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (TryResolveFromConfiguration(launcherBinRoot, configurationName, executableName, out path))
            {
                return true;
            }
        }

        path = null;
        return false;
    }

    private static bool TryResolveFromConfiguration(string launcherBinRoot, string configurationName, string executableName, [NotNullWhen(true)] out string? path)
    {
        if (string.IsNullOrWhiteSpace(configurationName))
        {
            path = null;
            return false;
        }

        var configurationRoot = Path.Combine(launcherBinRoot, configurationName);
        if (!Directory.Exists(configurationRoot))
        {
            path = null;
            return false;
        }

        // Check the configuration root directly (single-TFM builds).
        var directCandidate = Path.Combine(configurationRoot, executableName);
        if (File.Exists(directCandidate))
        {
            path = directCandidate;
            return true;
        }

        // Otherwise, look in each target framework folder.
        foreach (var tfmDirectory in Directory.EnumerateDirectories(configurationRoot))
        {
            var candidate = Path.Combine(tfmDirectory, executableName);
            if (File.Exists(candidate))
            {
                path = candidate;
                return true;
            }
        }

        path = null;
        return false;
    }
#else
    private static bool TryResolveDevelopmentBuild(string executableName, [NotNullWhen(true)] out string? path)
    {
        path = null;
        return false;
    }
#endif
}
