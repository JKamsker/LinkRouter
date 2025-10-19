using Avalonia;
using System;
using System.Reflection;
using System.Threading;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Headless;

namespace LinkRouter.Settings.Avalonia.Tests;

internal static class TestAppHost
{
    public static ClassicDesktopStyleApplicationLifetime EnsureLifetime()
    {
        var session = HeadlessUnitTestSession.GetOrStartForAssembly(typeof(TestAppBuilder).Assembly);

        return session.Dispatch(() =>
        {
            if (Application.Current?.ApplicationLifetime is ClassicDesktopStyleApplicationLifetime lifetime)
            {
                return lifetime;
            }

            throw new InvalidOperationException("The Avalonia application lifetime could not be initialized.");
        }, CancellationToken.None).GetAwaiter().GetResult();
    }
}
