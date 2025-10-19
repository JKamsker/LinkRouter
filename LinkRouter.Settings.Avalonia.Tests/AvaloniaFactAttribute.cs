using Xunit;

namespace LinkRouter.Settings.Avalonia.Tests;

internal sealed class AvaloniaFactAttribute : FactAttribute
{
    public AvaloniaFactAttribute()
    {
        Timeout = 30_000;
    }
}
