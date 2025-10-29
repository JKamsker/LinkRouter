using System;
using System.IO;
using System.Runtime.Versioning;
using LinkRouter.Settings.Services.Linux;
using Xunit;

namespace LinkRouter.Settings.Tests.Linux;

[SupportedOSPlatform("linux")]
public class LinuxDefaultAppRegistrarTests
{
    [SkippableFact]
    public void RegisterPerUser_CreatesDesktopFile()
    {
        Skip.IfNot(OperatingSystem.IsLinux(), "Test requires Linux");

        var testDataHome = Path.Combine(Path.GetTempPath(), $"test-data-{Guid.NewGuid()}");
        Environment.SetEnvironmentVariable("XDG_DATA_HOME", testDataHome);

        try
        {
            // Create a fake launcher executable
            var testLauncherPath = Path.Combine(testDataHome, "LinkRouter.Launcher");
            Directory.CreateDirectory(Path.GetDirectoryName(testLauncherPath)!);
            File.WriteAllText(testLauncherPath, "#!/bin/bash\n");

            var registrar = new LinuxDefaultAppRegistrar();

            // Register with explicit path
            registrar.RegisterPerUser(testLauncherPath);

            // Verify desktop file was created
            var desktopFilePath = Path.Combine(testDataHome, "applications", "linkrouter.desktop");
            Assert.True(File.Exists(desktopFilePath), $"Desktop file should exist at {desktopFilePath}");

            // Verify content
            var content = File.ReadAllText(desktopFilePath);
            Assert.Contains("[Desktop Entry]", content);
            Assert.Contains("Type=Application", content);
            Assert.Contains("Name=LinkRouter", content);
            Assert.Contains($"Exec=\"{testLauncherPath}\" %u", content);
            Assert.Contains("MimeType=x-scheme-handler/http;x-scheme-handler/https;", content);
            Assert.Contains("Categories=Network;WebBrowser;", content);
            Assert.Contains("Terminal=false", content);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("XDG_DATA_HOME", null);
            if (Directory.Exists(testDataHome))
            {
                try { Directory.Delete(testDataHome, true); } catch { }
            }
        }
    }

    [SkippableFact]
    public void UnregisterPerUser_RemovesDesktopFile()
    {
        Skip.IfNot(OperatingSystem.IsLinux(), "Test requires Linux");

        var testDataHome = Path.Combine(Path.GetTempPath(), $"test-data-{Guid.NewGuid()}");
        Environment.SetEnvironmentVariable("XDG_DATA_HOME", testDataHome);

        try
        {
            // Create a fake launcher executable
            var testLauncherPath = Path.Combine(testDataHome, "LinkRouter.Launcher");
            Directory.CreateDirectory(Path.GetDirectoryName(testLauncherPath)!);
            File.WriteAllText(testLauncherPath, "#!/bin/bash\n");

            var registrar = new LinuxDefaultAppRegistrar();

            // Register first
            registrar.RegisterPerUser(testLauncherPath);

            var desktopFilePath = Path.Combine(testDataHome, "applications", "linkrouter.desktop");
            Assert.True(File.Exists(desktopFilePath));

            // Now unregister
            registrar.UnregisterPerUser();

            // Verify desktop file was removed
            Assert.False(File.Exists(desktopFilePath));
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("XDG_DATA_HOME", null);
            if (Directory.Exists(testDataHome))
            {
                try { Directory.Delete(testDataHome, true); } catch { }
            }
        }
    }

    [SkippableFact]
    public void RegisterPerUser_CreatesApplicationsDirectory_IfMissing()
    {
        Skip.IfNot(OperatingSystem.IsLinux(), "Test requires Linux");

        var testDataHome = Path.Combine(Path.GetTempPath(), $"test-data-{Guid.NewGuid()}");
        Environment.SetEnvironmentVariable("XDG_DATA_HOME", testDataHome);

        try
        {
            // Ensure applications directory doesn't exist
            var applicationsDir = Path.Combine(testDataHome, "applications");
            if (Directory.Exists(applicationsDir))
            {
                Directory.Delete(applicationsDir, true);
            }

            // Create a fake launcher executable
            var testLauncherPath = Path.Combine(testDataHome, "LinkRouter.Launcher");
            Directory.CreateDirectory(Path.GetDirectoryName(testLauncherPath)!);
            File.WriteAllText(testLauncherPath, "#!/bin/bash\n");

            var registrar = new LinuxDefaultAppRegistrar();

            // Register - should create applications directory
            registrar.RegisterPerUser(testLauncherPath);

            // Verify applications directory was created
            Assert.True(Directory.Exists(applicationsDir));

            // Verify desktop file exists
            var desktopFilePath = Path.Combine(applicationsDir, "linkrouter.desktop");
            Assert.True(File.Exists(desktopFilePath));
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("XDG_DATA_HOME", null);
            if (Directory.Exists(testDataHome))
            {
                try { Directory.Delete(testDataHome, true); } catch { }
            }
        }
    }

    [SkippableFact]
    public void UnregisterPerUser_DoesNotThrow_WhenDesktopFileDoesNotExist()
    {
        Skip.IfNot(OperatingSystem.IsLinux(), "Test requires Linux");

        var testDataHome = Path.Combine(Path.GetTempPath(), $"test-data-{Guid.NewGuid()}");
        Environment.SetEnvironmentVariable("XDG_DATA_HOME", testDataHome);

        try
        {
            var registrar = new LinuxDefaultAppRegistrar();

            // Should not throw even if file doesn't exist
            var exception = Record.Exception(() => registrar.UnregisterPerUser());
            Assert.Null(exception);
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("XDG_DATA_HOME", null);
            if (Directory.Exists(testDataHome))
            {
                try { Directory.Delete(testDataHome, true); } catch { }
            }
        }
    }
}
