using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using System;
using System.Threading;

namespace LinkRouter.Settings.Avalonia.Tests;

internal static class TestAppHost
{
    private static readonly object s_sync = new();
    private static ClassicDesktopStyleApplicationLifetime? s_lifetime;
    private static Thread? s_dispatchThread;
    private static CancellationTokenSource? s_dispatchLoop;
    private static Exception? s_startupException;

    public static ClassicDesktopStyleApplicationLifetime EnsureLifetime()
    {
        lock (s_sync)
        {
            if (s_lifetime is { } existingLifetime)
            {
                return existingLifetime;
            }

            var ready = new ManualResetEventSlim();
            s_dispatchLoop = new CancellationTokenSource();
            s_dispatchThread = new Thread(() =>
            {
                try
                {
                    var lifetime = new ClassicDesktopStyleApplicationLifetime();
                    TestAppBuilder.BuildAvaloniaApp()
                        .SetupWithLifetime(lifetime);

                    s_lifetime = lifetime;
                    ready.Set();

                    Dispatcher.UIThread.MainLoop(s_dispatchLoop.Token);
                }
                catch (Exception ex)
                {
                    s_startupException = ex;
                    ready.Set();
                }
            })
            {
                IsBackground = true,
                Name = "Avalonia UI Thread"
            };

            s_dispatchThread.Start();

            if (!ready.Wait(TimeSpan.FromSeconds(5)))
            {
                throw new InvalidOperationException("Timed out while starting the Avalonia test lifetime.");
            }

            if (s_startupException is { } startupException)
            {
                throw new InvalidOperationException("Failed to start the Avalonia test lifetime.", startupException);
            }

            return s_lifetime ?? throw new InvalidOperationException("Avalonia test lifetime was not initialized.");
        }
    }
}
