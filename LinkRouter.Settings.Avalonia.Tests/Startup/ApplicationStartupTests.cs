using Avalonia;
using Avalonia.Headless.XUnit;
using LinkRouter.Settings.Avalonia;
using Xunit;

namespace LinkRouter.Settings.Avalonia.Tests.Startup;

public class ApplicationStartupTests
{
    [AvaloniaFact(Timeout = 30_000)]
    public void Application_CanInitializeMainWindow()
    {
        Assert.IsType<LinkRouter.Settings.Avalonia.Tests.App>(Application.Current);

        var window = new MainWindow();
        try
        {
            window.Show();
            Assert.NotNull(window.ContentHost.Content);
        }
        finally
        {
            window.Close();
        }
    }
}
