using System;
using System.IO;
using System.Runtime.Versioning;
using LinkRouter.Settings.Services.Linux;
using Xunit;

namespace LinkRouter.Settings.Tests.Linux;

[SupportedOSPlatform("linux")]
public class LinuxAutostartServiceTests
{
    [SkippableFact]
    public void IsSupported_ReturnsTrue_OnLinux()
    {
        Skip.IfNot(OperatingSystem.IsLinux(), "Test requires Linux");

        var service = new LinuxAutostartService();
        Assert.True(service.IsSupported);
    }

    [SkippableFact]
    public void SetEnabled_CreatesDesktopFile_WhenEnabled()
    {
        Skip.IfNot(OperatingSystem.IsLinux(), "Test requires Linux");

        var testConfigHome = Path.Combine(Path.GetTempPath(), $"test-config-{Guid.NewGuid()}");
        Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", testConfigHome);

        try
        {
            var service = new LinuxAutostartService();
            var autostartDir = Path.Combine(testConfigHome, "autostart");
            var desktopFile = Path.Combine(autostartDir, "linkrouter-settings.desktop");

            // Enable autostart
            service.SetEnabled(true);

            // Verify desktop file was created
            Assert.True(File.Exists(desktopFile), $"Desktop file should exist at {desktopFile}");

            // Verify content contains expected entries
            var content = File.ReadAllText(desktopFile);
            Assert.Contains("[Desktop Entry]", content);
            Assert.Contains("Type=Application", content);
            Assert.Contains("Name=LinkRouter Settings", content);
            Assert.Contains("Exec=", content);
            Assert.Contains("--minimized", content);
            Assert.Contains("Terminal=false", content);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", null);
            if (Directory.Exists(testConfigHome))
            {
                try { Directory.Delete(testConfigHome, true); } catch { }
            }
        }
    }

    [SkippableFact]
    public void SetEnabled_RemovesDesktopFile_WhenDisabled()
    {
        Skip.IfNot(OperatingSystem.IsLinux(), "Test requires Linux");

        var testConfigHome = Path.Combine(Path.GetTempPath(), $"test-config-{Guid.NewGuid()}");
        Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", testConfigHome);

        try
        {
            var service = new LinuxAutostartService();
            var autostartDir = Path.Combine(testConfigHome, "autostart");
            var desktopFile = Path.Combine(autostartDir, "linkrouter-settings.desktop");

            // Enable first
            service.SetEnabled(true);
            Assert.True(File.Exists(desktopFile));

            // Now disable
            service.SetEnabled(false);

            // Verify desktop file was removed
            Assert.False(File.Exists(desktopFile));
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", null);
            if (Directory.Exists(testConfigHome))
            {
                try { Directory.Delete(testConfigHome, true); } catch { }
            }
        }
    }

    [SkippableFact]
    public void IsEnabled_ReturnsFalse_WhenNoDesktopFile()
    {
        Skip.IfNot(OperatingSystem.IsLinux(), "Test requires Linux");

        var testConfigHome = Path.Combine(Path.GetTempPath(), $"test-config-{Guid.NewGuid()}");
        Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", testConfigHome);

        try
        {
            var service = new LinuxAutostartService();
            Assert.False(service.IsEnabled());
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", null);
            if (Directory.Exists(testConfigHome))
            {
                try { Directory.Delete(testConfigHome, true); } catch { }
            }
        }
    }

    [SkippableFact]
    public void IsEnabled_ReturnsTrue_WhenDesktopFileExists()
    {
        Skip.IfNot(OperatingSystem.IsLinux(), "Test requires Linux");

        var testConfigHome = Path.Combine(Path.GetTempPath(), $"test-config-{Guid.NewGuid()}");
        Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", testConfigHome);

        try
        {
            var service = new LinuxAutostartService();

            // Enable autostart
            service.SetEnabled(true);

            // Check if it's enabled
            Assert.True(service.IsEnabled());
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", null);
            if (Directory.Exists(testConfigHome))
            {
                try { Directory.Delete(testConfigHome, true); } catch { }
            }
        }
    }

    [SkippableFact]
    public void AutostartDirectory_UsesXdgConfigHome_WhenSet()
    {
        Skip.IfNot(OperatingSystem.IsLinux(), "Test requires Linux");

        var customConfigHome = Path.Combine(Path.GetTempPath(), $"custom-config-{Guid.NewGuid()}");
        Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", customConfigHome);

        try
        {
            var service = new LinuxAutostartService();
            service.SetEnabled(true);

            var expectedPath = Path.Combine(customConfigHome, "autostart", "linkrouter-settings.desktop");
            Assert.True(File.Exists(expectedPath), $"Should create desktop file at custom XDG_CONFIG_HOME: {expectedPath}");
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", null);
            if (Directory.Exists(customConfigHome))
            {
                try { Directory.Delete(customConfigHome, true); } catch { }
            }
        }
    }
}
