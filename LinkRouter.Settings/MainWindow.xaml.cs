using System;
using System.Collections.Generic;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinRT;

namespace LinkRouter.Settings;

public sealed partial class MainWindow : Window
{
    private readonly Dictionary<string, Type> _pageMap = new()
    {
        ["General"] = typeof(Views.GeneralPage),
        ["Rules"] = typeof(Views.RulesPage),
        ["Profiles"] = typeof(Views.ProfilesPage),
        ["Default"] = typeof(Views.DefaultPage),
        ["ImportExport"] = typeof(Views.ImportExportPage),
        ["Advanced"] = typeof(Views.AdvancedPage),
        ["About"] = typeof(Views.AboutPage)
    };

    private readonly WindowsSystemDispatcherQueueHelper _wsdqHelper = new();
    private MicaController? _micaController;
    private SystemBackdropConfiguration? _backdropConfiguration;

    public MainWindow()
    {
        InitializeComponent();
        Title = "LinkRouter Settings";
        ExtendsContentIntoTitleBar = true;

        TryInitializeBackdrop();

        if (RootNavigationView.MenuItems.Count > 0)
        {
            RootNavigationView.SelectedItem = RootNavigationView.MenuItems[0];
            if (RootNavigationView.MenuItems[0] is NavigationViewItem item && item.Tag is string tag)
            {
                Navigate(tag);
            }
        }
    }

    private void OnNavigationSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer is NavigationViewItem item && item.Tag is string tag)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation to: {tag}");
            Navigate(tag);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"Navigation failed - SelectedItem: {args.SelectedItem}, SelectedItemContainer: {args.SelectedItemContainer}");
        }
    }

    private void Navigate(string tag)
    {
        System.Diagnostics.Debug.WriteLine($"Navigate called with tag: {tag}");
        if (_pageMap.TryGetValue(tag, out var pageType))
        {
            System.Diagnostics.Debug.WriteLine($"Found pageType: {pageType.Name}");
            var result = ContentFrame.Navigate(pageType);
            System.Diagnostics.Debug.WriteLine($"Navigate result: {result}, CurrentSourcePageType: {ContentFrame.CurrentSourcePageType?.Name}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"Tag '{tag}' not found in _pageMap");
        }
    }

    private void TryInitializeBackdrop()
    {
        if (!MicaController.IsSupported())
        {
            return;
        }

        _wsdqHelper.EnsureWindowsSystemDispatcherQueueController();

        _backdropConfiguration = new SystemBackdropConfiguration
        {
            Theme = SystemBackdropTheme.Default,
            IsInputActive = true
        };

        _micaController = new MicaController
        {
            Kind = MicaKind.Base
        };

        _micaController.AddSystemBackdropTarget(this.As<ICompositionSupportsSystemBackdrop>());
        _micaController.SetSystemBackdropConfiguration(_backdropConfiguration);

        Activated += OnWindowActivated;
        Closed += OnWindowClosed;
        ((FrameworkElement)Content).ActualThemeChanged += OnActualThemeChanged;
    }

    private void OnWindowActivated(object sender, WindowActivatedEventArgs args)
    {
        if (_backdropConfiguration is not null)
        {
            _backdropConfiguration.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
        }
    }

    private void OnWindowClosed(object sender, WindowEventArgs args)
    {
        _micaController?.Dispose();
        _micaController = null;
        Activated -= OnWindowActivated;
        Closed -= OnWindowClosed;
    }

    private void OnActualThemeChanged(FrameworkElement sender, object args)
    {
        if (_backdropConfiguration is null)
        {
            return;
        }

        _backdropConfiguration.Theme = sender.ActualTheme switch
        {
            ElementTheme.Dark => SystemBackdropTheme.Dark,
            ElementTheme.Light => SystemBackdropTheme.Light,
            _ => SystemBackdropTheme.Default
        };
    }
}

internal sealed class WindowsSystemDispatcherQueueHelper
{
    [System.Runtime.InteropServices.DllImport("CoreMessaging.dll")]
    private static extern int CreateDispatcherQueueController(DispatcherQueueOptions options, out nint dispatcherQueueController);

    private nint _controller;

    public void EnsureWindowsSystemDispatcherQueueController()
    {
        if (DispatcherQueue.GetForCurrentThread() != null)
        {
            return;
        }

        if (_controller != nint.Zero)
        {
            return;
        }

        DispatcherQueueOptions options = new()
        {
            size = (uint)System.Runtime.InteropServices.Marshal.SizeOf<DispatcherQueueOptions>(),
            threadType = 2,
            apartmentType = 2
        };

        CreateDispatcherQueueController(options, out _controller);
    }

    private struct DispatcherQueueOptions
    {
        public uint size;
        public int threadType;
        public int apartmentType;
    }
}
