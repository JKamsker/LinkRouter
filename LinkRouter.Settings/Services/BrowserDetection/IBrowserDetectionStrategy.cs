using System.Collections.Generic;

namespace LinkRouter.Settings.Services.BrowserDetection;

internal interface IBrowserDetectionStrategy
{
    BrowserFamily Family { get; }

    IEnumerable<BrowserInfo> DetectInstalledBrowsers();

    IReadOnlyList<BrowserProfileOption> GetProfileOptions(BrowserInfo browser);
}
