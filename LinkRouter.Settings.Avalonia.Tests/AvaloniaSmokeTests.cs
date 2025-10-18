using System;
using System.Threading;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Headless;
using LinkRouter.Settings.Avalonia;

namespace LinkRouter.Settings.Avalonia.Tests;

public class AvaloniaSmokeTests
{
    [Fact]
    public void App_Should_Create_MainWindow()
    {
        using var session = HeadlessUnitTestSession.StartNew(typeof(TestAppBuilder));

        session.Dispatch(() =>
        {
            using var lifetime = new ClassicDesktopStyleApplicationLifetime
            {
                Args = Array.Empty<string>()
            };

            var builder = TestAppBuilder.BuildAvaloniaApp();
            builder.SetupWithLifetime(lifetime);

            Assert.NotNull(lifetime.MainWindow);
            Assert.IsType<MainWindow>(lifetime.MainWindow);

            lifetime.Shutdown();
        }, CancellationToken.None);
    }
}
