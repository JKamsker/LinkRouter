using System.Threading.Tasks;
using Avalonia;
using Avalonia.Headless.XUnit;
using FluentAvalonia.Styling;
using LinkRouter.Settings.Avalonia.Tests;
using Xunit;

namespace LinkRouter.Settings.Avalonia.Tests.Startup;

public class ApplicationStartupTests
{
    [AvaloniaFact(Timeout = 30_000)]
    public Task App_RegistersFluentTheme()
    {
        var app = Assert.IsType<App>(Application.Current);

        Assert.Contains(app.Styles, style => style is FluentAvaloniaTheme);

        return Task.CompletedTask;
    }
}
