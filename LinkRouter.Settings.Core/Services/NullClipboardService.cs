using LinkRouter.Settings.Services.Interfaces;

namespace LinkRouter.Settings.Services;

internal sealed class NullClipboardService : IClipboardService
{
    public static IClipboardService Instance { get; } = new NullClipboardService();

    private NullClipboardService()
    {
    }

    public void SetText(string text)
    {
        // intentionally left blank - used when no clipboard implementation is available
    }
}
