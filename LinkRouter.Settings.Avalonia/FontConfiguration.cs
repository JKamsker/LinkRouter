using System.Collections.Generic;
using Avalonia.Media;

namespace LinkRouter.Settings.Avalonia;

/// <summary>
/// Provides reusable font configuration so FluentAvalonia symbol fonts resolve in headless and desktop hosts.
/// </summary>
public static class FontConfiguration
{
    public static FontManagerOptions Create()
    {
        var iconFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets, Segoe UI Symbol");

        return new FontManagerOptions
        {
            FontFamilyMappings = new Dictionary<string, FontFamily>
            {
                ["Symbols"] = iconFamily,
                ["FluentSystemIcons"] = iconFamily
            },
            FontFallbacks = new[]
            {
                new FontFallback
                {
                    FontFamily = iconFamily
                }
            }
        };
    }
}

