using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Headless.XUnit;
using LinkRouter.Settings.Avalonia;
using LinkRouter.Settings.Avalonia.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using SettingsApp = LinkRouter.Settings.Avalonia.App;

namespace LinkRouter.Settings.Avalonia.Tests.Startup;

public class ServiceConfigurationTests
{
    [AvaloniaFact(Timeout = 30_000)]
    public Task RegistersDesktopLifetimeAsSingleton()
    {
        var lifetime = new ClassicDesktopStyleApplicationLifetime();

        var services = new ServiceCollection();
        SettingsApp.ConfigureServices(services, lifetime);

        var descriptor = Assert.Single(
            services.Where(d => d.ServiceType == typeof(IClassicDesktopStyleApplicationLifetime)));

        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.Same(lifetime, descriptor.ImplementationInstance);

        return Task.CompletedTask;
    }

    [AvaloniaFact(Timeout = 30_000)]
    public Task SettingsTrayIconService_IsResolvable()
    {
        var app = Application.Current;
        Assert.NotNull(app);

        var lifetime = new ClassicDesktopStyleApplicationLifetime();

        var services = new ServiceCollection();
        SettingsApp.ConfigureServices(services, lifetime);

        using var provider = services.BuildServiceProvider();
        using var trayService = provider.GetRequiredService<SettingsTrayIconService>();

        Assert.NotNull(trayService);

        return Task.CompletedTask;
    }
}
