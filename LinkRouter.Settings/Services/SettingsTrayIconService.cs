using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using LinkRouter.Settings;

namespace LinkRouter.Settings.Services;

internal sealed class SettingsTrayIconService : IDisposable
{
    private readonly Application application;
    private readonly IClassicDesktopStyleApplicationLifetime desktopLifetime;
    private readonly MainWindow mainWindow;
    private readonly List<(NativeMenuItem Item, EventHandler Handler)> menuSubscriptions = new();
    private readonly TrayIcon trayIcon;
    private readonly TrayIcons trayIcons;
    private readonly NativeMenu trayMenu;
    private readonly NativeMenu windowMenu;

    private bool isShutdownRequested;
    private bool isDisposed;

    public SettingsTrayIconService(
        IClassicDesktopStyleApplicationLifetime desktopLifetime,
        MainWindow mainWindow)
    {
        this.desktopLifetime = desktopLifetime ?? throw new ArgumentNullException(nameof(desktopLifetime));
        this.mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));

        application = Application.Current ?? throw new InvalidOperationException("Application must be initialized before creating the tray icon.");

        desktopLifetime.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        trayIcon = CreateTrayIcon();
        trayMenu = BuildMenu();
        trayIcon.Menu = trayMenu;
        windowMenu = BuildMenu();
        NativeMenu.SetMenu(mainWindow, windowMenu);

        trayIcon.Clicked += OnTrayIconClicked;

        trayIcons = TrayIcon.GetIcons(application) ?? new TrayIcons();
        if (!trayIcons.Contains(trayIcon))
        {
            trayIcons.Add(trayIcon);
        }

        TrayIcon.SetIcons(application, trayIcons);

        mainWindow.Closing += OnMainWindowClosing;
        desktopLifetime.Exit += OnDesktopExit;
    }

    private TrayIcon CreateTrayIcon()
    {
        return new TrayIcon
        {
            Icon = LoadIcon(),
            ToolTipText = "LinkRouter Settings",
            IsVisible = true
        };
    }

    private NativeMenu BuildMenu()
    {
        var menu = new NativeMenu();
        menu.Items.Add(CreateMenuItem("Open Settings", OnOpenMenuItemClick));
        menu.Items.Add(new NativeMenuItemSeparator());
        menu.Items.Add(CreateMenuItem("Quit application", OnQuitMenuItemClick));
        return menu;
    }

    private NativeMenuItem CreateMenuItem(string header, EventHandler handler)
    {
        var item = new NativeMenuItem(header);
        item.Click += handler;
        menuSubscriptions.Add((item, handler));
        return item;
    }

    private static WindowIcon LoadIcon()
    {
        var assetUri = new Uri("avares://LinkRouter.Settings/LinkRouter.ico");
        using var stream = AssetLoader.Open(assetUri);
        return new WindowIcon(stream);
    }

    private void OnTrayIconClicked(object? sender, EventArgs e)
    {
        RestoreFromTray();
    }

    private void OnOpenMenuItemClick(object? sender, EventArgs e)
    {
        RestoreFromTray();
    }

    private void OnQuitMenuItemClick(object? sender, EventArgs e)
    {
        isShutdownRequested = true;
        desktopLifetime.Shutdown();
    }

    private void RestoreFromTray()
    {
        if (!mainWindow.IsVisible)
        {
            mainWindow.Show();
        }

        mainWindow.ShowInTaskbar = true;

        if (mainWindow.WindowState == WindowState.Minimized)
        {
            mainWindow.WindowState = WindowState.Normal;
        }

        mainWindow.Activate();
    }

    private void HideToTray()
    {
        trayIcon.IsVisible = true;
        mainWindow.ShowInTaskbar = false;

        if (mainWindow.IsVisible)
        {
            mainWindow.Hide();
        }
    }

    private void OnMainWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        if (isShutdownRequested)
        {
            return;
        }

        e.Cancel = true;
        HideToTray();
    }

    private void OnDesktopExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        Dispose();
    }

    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        isDisposed = true;

        mainWindow.Closing -= OnMainWindowClosing;
        desktopLifetime.Exit -= OnDesktopExit;

        trayIcon.Clicked -= OnTrayIconClicked;

        foreach (var (item, handler) in menuSubscriptions)
        {
            item.Click -= handler;
        }
        menuSubscriptions.Clear();

        trayIcons.Remove(trayIcon);
        TrayIcon.SetIcons(application, trayIcons);

        NativeMenu.SetMenu(mainWindow, null);

        trayIcon.Dispose();
    }
}
