using System;
using System.Diagnostics;
using System.Runtime.Versioning;
using Microsoft.Win32;
using LinkRouter.Settings.Services.Abstractions;

namespace LinkRouter.Settings.Services;

[
    SupportedOSPlatform("windows")
]
internal sealed class WindowsAutostartService : IAutostartService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "LinkRouter";

    private readonly string command;

    public WindowsAutostartService()
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Windows autostart is only available on Windows platforms.");
        }

        var executablePath = ResolveExecutablePath();
        command = $"\"{executablePath}\" --minimized";
    }

    public bool IsSupported => true;

    public bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
        if (key is null)
        {
            return false;
        }

        var value = key.GetValue(ValueName) as string;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return string.Equals(Normalize(value), Normalize(command), StringComparison.OrdinalIgnoreCase);
    }

    public void SetEnabled(bool enabled)
    {
        if (!enabled)
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
            key?.DeleteValue(ValueName, false);
            return;
        }

        using var writableKey = Registry.CurrentUser.CreateSubKey(RunKeyPath);
        writableKey.SetValue(ValueName, command, RegistryValueKind.String);
    }

    private static string Normalize(string value) => value.Trim();

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

internal sealed class NoOpAutostartService : IAutostartService
{
    public bool IsSupported => false;

    public bool IsEnabled() => false;

    public void SetEnabled(bool enabled)
    {
        // Autostart is not supported on this platform.
    }
}
