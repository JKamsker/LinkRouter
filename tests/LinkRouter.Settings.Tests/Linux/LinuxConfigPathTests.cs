using System;
using System.IO;
using System.Runtime.Versioning;
using LinkRouter.Settings.Services;
using Xunit;

namespace LinkRouter.Settings.Tests.Linux;

[SupportedOSPlatform("linux")]
public class LinuxConfigPathTests
{
    [SkippableFact]
    public void ConfigService_UsesXdgConfigHome_OnLinux()
    {
        Skip.IfNot(OperatingSystem.IsLinux(), "Test requires Linux");

        var testConfigHome = Path.Combine(Path.GetTempPath(), $"test-config-{Guid.NewGuid()}");
        Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", testConfigHome);

        try
        {
            var configService = new ConfigService();

            // Verify config path uses XDG_CONFIG_HOME
            var expectedConfigRoot = Path.Combine(testConfigHome, "LinkRouter");
            Assert.Contains(expectedConfigRoot, configService.ConfigPath);
            Assert.Contains(expectedConfigRoot, configService.ManifestPath);
            Assert.Contains(expectedConfigRoot, configService.BackupFolder);
        }
        finally
        {
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", null);
            if (Directory.Exists(testConfigHome))
            {
                try { Directory.Delete(testConfigHome, true); } catch { }
            }
        }
    }

    [SkippableFact]
    public void ConfigService_FallsBackToDefaultXdgPath_WhenXdgConfigHomeNotSet()
    {
        Skip.IfNot(OperatingSystem.IsLinux(), "Test requires Linux");

        // Ensure XDG_CONFIG_HOME is not set
        var originalValue = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", null);

        try
        {
            var configService = new ConfigService();
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var expectedConfigRoot = Path.Combine(homeDir, ".config", "LinkRouter");

            // Should use ~/.config/LinkRouter
            Assert.Contains(expectedConfigRoot, configService.ConfigPath);
        }
        finally
        {
            Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", originalValue);
        }
    }

    [SkippableFact]
    public void ConfigService_ConfigPath_EndsWithSettingsJson()
    {
        Skip.IfNot(OperatingSystem.IsLinux(), "Test requires Linux");

        var configService = new ConfigService();

        Assert.EndsWith("settings.json", configService.ConfigPath);
    }

    [SkippableFact]
    public void ConfigService_ManifestPath_EndsWithMappingsJson()
    {
        Skip.IfNot(OperatingSystem.IsLinux(), "Test requires Linux");

        var configService = new ConfigService();

        Assert.EndsWith("mappings.json", configService.ManifestPath);
    }

    [SkippableFact]
    public void ConfigService_BackupFolder_EndsWithBackups()
    {
        Skip.IfNot(OperatingSystem.IsLinux(), "Test requires Linux");

        var configService = new ConfigService();

        Assert.EndsWith("backups", configService.BackupFolder);
    }

    [SkippableFact]
    public void ConfigService_PathsAreUnderSameRoot()
    {
        Skip.IfNot(OperatingSystem.IsLinux(), "Test requires Linux");

        var configService = new ConfigService();

        // All paths should be under the same LinkRouter directory
        var configDir = Path.GetDirectoryName(configService.ConfigPath);
        var manifestDir = Path.GetDirectoryName(configService.ManifestPath);
        var backupParentDir = Path.GetDirectoryName(configService.BackupFolder);

        Assert.Equal(configDir, manifestDir);
        Assert.Equal(configDir, backupParentDir);
    }
}
