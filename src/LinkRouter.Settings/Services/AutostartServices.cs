using LinkRouter.Settings.Services.Abstractions;

namespace LinkRouter.Settings.Services;

internal sealed class NoOpAutostartService : IAutostartService
{
    public bool IsSupported => false;

    public bool IsEnabled() => false;

    public void SetEnabled(bool enabled)
    {
        // Autostart is not supported on this platform.
    }
}
