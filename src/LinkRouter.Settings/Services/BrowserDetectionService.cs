using System;
using System.Collections.Generic;
using System.Linq;
using LinkRouter.Settings.Services.Abstractions;
using LinkRouter.Settings.Services.Windows.BrowserDetection;

namespace LinkRouter.Settings.Services;

public sealed class BrowserDetectionService
{
    private readonly IReadOnlyList<IBrowserDetectionStrategy> detectionStrategies;
    private readonly IReadOnlyDictionary<BrowserFamily, IBrowserDetectionStrategy> strategyMap;

    public BrowserDetectionService()
        : this(new IBrowserDetectionStrategy[]
        {
            new ChromiumBrowserDetectionStrategy(),
            new FirefoxBrowserDetectionStrategy()
        })
    {
    }

    internal BrowserDetectionService(IEnumerable<IBrowserDetectionStrategy> strategies)
    {
        detectionStrategies = strategies?.ToArray() ?? Array.Empty<IBrowserDetectionStrategy>();
        strategyMap = detectionStrategies
            .GroupBy(strategy => strategy.Family)
            .ToDictionary(group => group.Key, group => group.First());
    }

    public IReadOnlyList<BrowserInfo> DetectInstalledBrowsers()
    {
        var results = new List<BrowserInfo>();
        foreach (var strategy in detectionStrategies)
        {
            try
            {
                results.AddRange(strategy.DetectInstalledBrowsers());
            }
            catch
            {
                // Ignore strategy failures and continue to the next one.
            }
        }

        return results;
    }

    public IReadOnlyList<BrowserProfileOption> GetBrowserProfileOptions(BrowserInfo browser)
    {
        try
        {
            if (strategyMap.TryGetValue(browser.Family, out var strategy))
            {
                return strategy.GetProfileOptions(browser);
            }

            return Array.Empty<BrowserProfileOption>();
        }
        catch
        {
            return Array.Empty<BrowserProfileOption>();
        }
    }

    public string? GetDefaultBrowserExecutablePath()
    {
        return DefaultBrowserResolver.GetDefaultBrowserExecutablePath();
    }

    public string? GetDefaultBrowserProgId(string scheme = "http")
    {
        return DefaultBrowserResolver.GetDefaultBrowserProgId(scheme);
    }

    public (bool isDefault, string? currentProgId, string? currentPath) CheckIfLinkRouterIsDefault(string? expectedLinkRouterPath = null)
    {
        return DefaultBrowserResolver.CheckIfLinkRouterIsDefault(expectedLinkRouterPath);
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

public sealed record BrowserProfileOption(string DisplayName, string? ProfileArgument, string? UserDataDir);
