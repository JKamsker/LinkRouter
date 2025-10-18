using LinkRouter.Settings.Services.Interfaces;
using Windows.ApplicationModel.DataTransfer;

namespace LinkRouter.Settings.Platform;

internal sealed class WinUIClipboardService : IClipboardService
{
    public void SetText(string text)
    {
        var package = new DataPackage
        {
            RequestedOperation = DataPackageOperation.Copy
        };

        package.SetText(text);
        Clipboard.SetContent(package);
    }
}
