using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace LinkRouter;

public static class DefaultAppRegistrar
{
    private const string AppName = "LinkRouter";
    private const string ProgId = "LinkRouterURL"; // Our per-user ProgID

    public static bool IsRegistered()
    {
        using var progIdKey = Registry.CurrentUser.OpenSubKey($"Software\\Classes\\{ProgId}");
        if (progIdKey is null)
        {
            return false;
        }

        using var appRoot = Registry.CurrentUser.OpenSubKey($"Software\\Clients\\StartMenuInternet\\{AppName}");
        if (appRoot is null)
        {
            return false;
        }

        using var regApps = Registry.CurrentUser.OpenSubKey("Software\\RegisteredApplications");
        var registrationPath = regApps?.GetValue(AppName) as string;
        return !string.IsNullOrEmpty(registrationPath);
    }

    public static void RegisterPerUser()
    {
        string exePath = GetExecutablePath();
        string iconRef = exePath + ",0";
        string command = $"\"{exePath}\" \"%1\"";

        // 1) Create ProgID under HKCU\Software\Classes
        using (var progidKey = Registry.CurrentUser.CreateSubKey($"Software\\Classes\\{ProgId}"))
        {
            if (progidKey == null) throw new InvalidOperationException("Failed to create ProgID registry key.");
            progidKey.SetValue(null, "LinkRouter URL");
            progidKey.SetValue("URL Protocol", ""); // presence marks as URL protocol

            using (var defaultIcon = progidKey.CreateSubKey("DefaultIcon"))
            {
                defaultIcon?.SetValue(null, iconRef);
            }
            using (var commandKey = progidKey.CreateSubKey("shell\\open\\command"))
            {
                commandKey?.SetValue(null, command);
            }
        }

        // 2) Register application capabilities like a browser (per-user) so it shows in Default Apps UI
        using (var appRoot = Registry.CurrentUser.CreateSubKey($"Software\\Clients\\StartMenuInternet\\{AppName}"))
        {
            if (appRoot == null) throw new InvalidOperationException("Failed to create application registry key.");
            appRoot.SetValue(null, AppName);

            using (var defaultIcon = appRoot.CreateSubKey("DefaultIcon"))
            {
                defaultIcon?.SetValue(null, iconRef);
            }
            using (var openCmd = appRoot.CreateSubKey("shell\\open\\command"))
            {
                openCmd?.SetValue(null, exePath);
            }
            using (var caps = appRoot.CreateSubKey("Capabilities"))
            {
                caps?.SetValue("ApplicationName", AppName);
                caps?.SetValue("ApplicationDescription", "Routes links to configured browsers/profiles.");
                caps?.SetValue("ApplicationIcon", iconRef);
                using (var urlAssoc = caps?.CreateSubKey("UrlAssociations"))
                {
                    urlAssoc?.SetValue("http", ProgId);
                    urlAssoc?.SetValue("https", ProgId);
                }
            }
        }

        // 3) Register under RegisteredApplications so Windows sees our Capabilities
        using (var regApps = Registry.CurrentUser.CreateSubKey("Software\\RegisteredApplications"))
        {
            regApps?.SetValue(AppName, $"Software\\Clients\\StartMenuInternet\\{AppName}\\Capabilities");
        }

        // 4) Prompt user to set defaults in Settings (we cannot set it silently due to Windows protections)
        TryOpenDefaultAppsSettings();

        Console.WriteLine("Registration complete. Opened Windows Settings where you can set LinkRouter as the default handler for HTTP and HTTPS.");
    }

    public static void UnregisterPerUser()
    {
        try
        {
            Registry.CurrentUser.DeleteSubKeyTree($"Software\\Clients\\StartMenuInternet\\{AppName}", throwOnMissingSubKey: false);
            using (var regApps = Registry.CurrentUser.CreateSubKey("Software\\RegisteredApplications"))
            {
                regApps?.DeleteValue(AppName, throwOnMissingValue: false);
            }
            Registry.CurrentUser.DeleteSubKeyTree($"Software\\Classes\\{ProgId}", throwOnMissingSubKey: false);
            Console.WriteLine("LinkRouter per-user registration removed. You may wish to reassign defaults in Windows Settings.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to unregister: {ex.Message}");
        }
    }

    private static void TryOpenDefaultAppsSettings()
    {
        try
        {
            // Windows 11: can open app-specific page
            Process.Start(new ProcessStartInfo
            {
                FileName = $"ms-settings:defaultapps?name={Uri.EscapeDataString(AppName)}",
                UseShellExecute = true
            });
        }
        catch
        {
            try
            {
                // Fallback: general default apps page (Windows 10/11)
                Process.Start(new ProcessStartInfo
                {
                    FileName = "ms-settings:defaultapps",
                    UseShellExecute = true
                });
            }
            catch
            {
                // As last resort, open classic Default Programs (older systems)
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "control.exe"),
                        Arguments = "/name Microsoft.DefaultPrograms",
                        UseShellExecute = true
                    });
                }
                catch
                {
                    // swallow – nothing more to do
                }
            }
        }
    }

    private static string GetExecutablePath()
    {
        // .NET 6+ provides Environment.ProcessPath
        var path = Environment.ProcessPath;
        if (!string.IsNullOrEmpty(path)) return path!;
        return Process.GetCurrentProcess().MainModule?.FileName
               ?? Path.Combine(AppContext.BaseDirectory, AppDomain.CurrentDomain.FriendlyName);
    }
}
