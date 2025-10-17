using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace LinkRouter.Settings.UITests;

/// <summary>
/// Starts a WinUI Application on a dedicated STA thread and provides helpers
/// to marshal test code onto the UI thread.
/// </summary>
public sealed class WinUiTestHost : IDisposable
{
    private readonly Thread _uiThread;
    private readonly ManualResetEventSlim _ready = new(initialState: false);
    private DispatcherQueue? _dispatcher;
    private Exception? _startupError;

    public bool IsReady { get; private set; }
    public string? StartupErrorMessage => _startupError?.Message;

    public WinUiTestHost()
    {
        _uiThread = new Thread(UIThreadStart)
        {
            IsBackground = true,
            Name = "WinUI Test UI Thread",
        };
        // intentionally not starting yet; tests will opt-in
    }

    public void EnsureStarted()
    {
        if (IsReady || _startupError is not null)
        {
            return;
        }
        _uiThread.SetApartmentState(ApartmentState.STA);
        _uiThread.Start();
        _ready.Wait();
        IsReady = _startupError is null;
    }

    private void UIThreadStart()
    {
        try
        {
            Application.Start((p) =>
            {
                // Create a minimal Application that does not open a window
                _ = new TestApp();

                // Capture DispatcherQueue for invoking test actions
                _dispatcher = DispatcherQueue.GetForCurrentThread();

                _ready.Set();
            });
        }
        catch (Exception ex)
        {
            _startupError = ex;
            _ready.Set();
        }
    }

    private sealed class TestApp : Application
    {
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // Intentionally do nothing; we only need XAML initialized.
        }
    }

    public Task RunOnUIThreadAsync(Action action)
    {
        if (_dispatcher is null)
        {
            throw new InvalidOperationException("UI dispatcher not ready");
        }

        var tcs = new TaskCompletionSource<object?>();
        _dispatcher.TryEnqueue(() =>
        {
            try
            {
                action();
                tcs.SetResult(null);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        return tcs.Task;
    }

    public Task<T> RunOnUIThreadAsync<T>(Func<T> func)
    {
        if (_dispatcher is null)
        {
            throw new InvalidOperationException("UI dispatcher not ready");
        }

        var tcs = new TaskCompletionSource<T>();
        _dispatcher.TryEnqueue(() =>
        {
            try
            {
                var result = func();
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        return tcs.Task;
    }

    public void Dispose()
    {
        _ready.Dispose();
    }
}

[CollectionDefinition("WinUI-UIThread")]
public class WinUiCollection : ICollectionFixture<WinUiTestHost> { }
