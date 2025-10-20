using System;
using System.Diagnostics;
using System.IO;
using LinkRouter.Settings.Services.Abstractions;

namespace LinkRouter.Settings.Services;

internal sealed class RouterPathResolver : IRouterPathResolver
{
    public bool TryGetRouterExecutable(out string? path)
    {
        var executableName = OperatingSystem.IsWindows() ? "LinkRouter.exe" : "LinkRouter";

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
    private static bool TryResolveDevelopmentBuild(string executableName, out string? path)
    {
        var baseDirectory = AppContext.BaseDirectory;
        var frameworkDirectory = new DirectoryInfo(baseDirectory);
        var configurationDirectory = frameworkDirectory.Parent;
        if (configurationDirectory?.Parent?.Parent == null)
        {
            path = null;
            return false;
        }

        var candidate = Path.GetFullPath(
            Path.Combine(
                baseDirectory,
                "..",
                "..",
                "..",
                "..",
                "LinkRouter",
                "bin",
                configurationDirectory.Name,
                frameworkDirectory.Name,
                executableName));

        if (File.Exists(candidate))
        {
            path = candidate;
            return true;
        }

        path = null;
        return false;
    }
#else
    private static bool TryResolveDevelopmentBuild(string executableName, out string? path)
    {
        path = null;
        return false;
    }
#endif
}
