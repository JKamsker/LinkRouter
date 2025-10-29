using System;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using LinkRouter.Settings.Services;
using LinkRouter.Settings.Services.Linux.BrowserDetection;
using Xunit;

namespace LinkRouter.Settings.Tests.Linux;

[SupportedOSPlatform("linux")]
public class LinuxBrowserDetectionStrategyTests
{
    [SkippableFact]
    public void DetectInstalledBrowsers_FindsChrome_WhenDesktopFileExists()
    {
        Skip.IfNot(OperatingSystem.IsLinux(), "Test requires Linux");

        var testDataHome = Path.Combine(Path.GetTempPath(), $"test-apps-{Guid.NewGuid()}");
        Environment.SetEnvironmentVariable("XDG_DATA_HOME", testDataHome);

        try
        {
            var applicationsDir = Path.Combine(testDataHome, "applications");
            Directory.CreateDirectory(applicationsDir);

            // Create a fake Chrome desktop file
            var chromeDesktopFile = Path.Combine(applicationsDir, "google-chrome.desktop");
            var chromeDesktopContent = @"[Desktop Entry]
Version=1.0
Name=Google Chrome
Exec=/usr/bin/google-chrome %U
Icon=google-chrome
Type=Application
Categories=Network;WebBrowser;
";
            File.WriteAllText(chromeDesktopFile, chromeDesktopContent);

            var strategy = new LinuxBrowserDetectionStrategy();
            var browsers = strategy.DetectInstalledBrowsers().ToList();

            // Should find Chrome
            var chrome = browsers.FirstOrDefault(b => b.Name.Contains("Chrome"));
            Assert.NotNull(chrome);
            Assert.Equal(BrowserFamily.Chromium, chrome!.Family);
            Assert.Contains("google-chrome", chrome.Path);
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
    public void DetectInstalledBrowsers_FindsFirefox_WhenDesktopFileExists()
    {
        Skip.IfNot(OperatingSystem.IsLinux(), "Test requires Linux");

        var testDataHome = Path.Combine(Path.GetTempPath(), $"test-apps-{Guid.NewGuid()}");
        Environment.SetEnvironmentVariable("XDG_DATA_HOME", testDataHome);

        try
        {
            var applicationsDir = Path.Combine(testDataHome, "applications");
            Directory.CreateDirectory(applicationsDir);

            // Create a fake Firefox desktop file
            var firefoxDesktopFile = Path.Combine(applicationsDir, "firefox.desktop");
            var firefoxDesktopContent = @"[Desktop Entry]
Version=1.0
Name=Mozilla Firefox
Exec=/usr/bin/firefox %u
Icon=firefox
Type=Application
Categories=Network;WebBrowser;
";
            File.WriteAllText(firefoxDesktopFile, firefoxDesktopContent);

            var strategy = new LinuxBrowserDetectionStrategy();
            var browsers = strategy.DetectInstalledBrowsers().ToList();

            // Should find Firefox
            var firefox = browsers.FirstOrDefault(b => b.Name.Contains("Firefox"));
            Assert.NotNull(firefox);
            Assert.Equal(BrowserFamily.Firefox, firefox!.Family);
            Assert.Contains("firefox", firefox.Path);
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
    public void DetectInstalledBrowsers_FindsMultipleBrowsers()
    {
        Skip.IfNot(OperatingSystem.IsLinux(), "Test requires Linux");

        var testDataHome = Path.Combine(Path.GetTempPath(), $"test-apps-{Guid.NewGuid()}");
        Environment.SetEnvironmentVariable("XDG_DATA_HOME", testDataHome);

        try
        {
            var applicationsDir = Path.Combine(testDataHome, "applications");
            Directory.CreateDirectory(applicationsDir);

            // Create multiple browser desktop files (using different executables to avoid deduplication)
            File.WriteAllText(Path.Combine(applicationsDir, "google-chrome.desktop"), @"[Desktop Entry]
Name=Google Chrome Test
Exec=/bin/sh %U
Type=Application
");

            File.WriteAllText(Path.Combine(applicationsDir, "firefox.desktop"), @"[Desktop Entry]
Name=Mozilla Firefox Test
Exec=/bin/ls %u
Type=Application
");

            File.WriteAllText(Path.Combine(applicationsDir, "brave-browser.desktop"), @"[Desktop Entry]
Name=Brave Web Browser Test
Exec=/bin/cat %U
Type=Application
");

            var strategy = new LinuxBrowserDetectionStrategy();
            var browsers = strategy.DetectInstalledBrowsers().ToList();

            // Should find all three test browsers (may also find system browsers)
            Assert.True(browsers.Count >= 3, $"Expected at least 3 browsers, found {browsers.Count}");
            Assert.Contains(browsers, b => b.Name.Contains("Chrome Test"));
            Assert.Contains(browsers, b => b.Name.Contains("Firefox Test"));
            Assert.Contains(browsers, b => b.Name.Contains("Brave") && b.Name.Contains("Test"));
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
    public void DetectInstalledBrowsers_HandlesQuotedExecPaths()
    {
        Skip.IfNot(OperatingSystem.IsLinux(), "Test requires Linux");

        var testDataHome = Path.Combine(Path.GetTempPath(), $"test-apps-{Guid.NewGuid()}");
        Environment.SetEnvironmentVariable("XDG_DATA_HOME", testDataHome);

        try
        {
            var applicationsDir = Path.Combine(testDataHome, "applications");
            Directory.CreateDirectory(applicationsDir);

            // Create desktop file with quoted Exec path
            var desktopFile = Path.Combine(applicationsDir, "chrome.desktop");
            var desktopContent = @"[Desktop Entry]
Name=Google Chrome
Exec=""/usr/bin/google-chrome"" %U
Type=Application
";
            File.WriteAllText(desktopFile, desktopContent);

            var strategy = new LinuxBrowserDetectionStrategy();
            var browsers = strategy.DetectInstalledBrowsers().ToList();

            var chrome = browsers.FirstOrDefault(b => b.Name.Contains("Chrome"));
            Assert.NotNull(chrome);
            // Path should be extracted without quotes
            Assert.DoesNotContain("\"", chrome!.Path);
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
    public void DetectInstalledBrowsers_IgnoresInvalidDesktopFiles()
    {
        Skip.IfNot(OperatingSystem.IsLinux(), "Test requires Linux");

        var testDataHome = Path.Combine(Path.GetTempPath(), $"test-apps-{Guid.NewGuid()}");
        Environment.SetEnvironmentVariable("XDG_DATA_HOME", testDataHome);

        try
        {
            var applicationsDir = Path.Combine(testDataHome, "applications");
            Directory.CreateDirectory(applicationsDir);

            // Create an invalid desktop file (missing Exec)
            var invalidDesktopFile = Path.Combine(applicationsDir, "invalid.desktop");
            var invalidContent = @"[Desktop Entry]
Name=Invalid App
Type=Application
";
            File.WriteAllText(invalidDesktopFile, invalidContent);

            // Create a valid one (using /bin/sh as test executable)
            var validDesktopFile = Path.Combine(applicationsDir, "firefox.desktop");
            var validContent = @"[Desktop Entry]
Name=Firefox Test Valid
Exec=/bin/sh %u
Type=Application
";
            File.WriteAllText(validDesktopFile, validContent);

            var strategy = new LinuxBrowserDetectionStrategy();
            var browsers = strategy.DetectInstalledBrowsers().ToList();

            // Should find the valid browser (may also find system browsers, but not the invalid one)
            Assert.Contains(browsers, b => b.Name.Contains("Firefox Test Valid"));
            Assert.DoesNotContain(browsers, b => b.Name.Contains("Invalid App"));
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
    public void GetProfileOptions_ReturnsEmpty_ForNow()
    {
        Skip.IfNot(OperatingSystem.IsLinux(), "Test requires Linux");

        var strategy = new LinuxBrowserDetectionStrategy();
        var browserInfo = new BrowserInfo("Test Browser", "/usr/bin/test", BrowserFamily.Chromium);

        var profiles = strategy.GetProfileOptions(browserInfo);

        // Profile support is not yet implemented for Linux
        Assert.Empty(profiles);
    }
}
