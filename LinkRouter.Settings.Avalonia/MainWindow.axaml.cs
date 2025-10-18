using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using LinkRouter.Settings.Avalonia.Views;

namespace LinkRouter.Settings.Avalonia;

public partial class MainWindow : Window
{
    private readonly Dictionary<string, Control> _pageCache;
    private readonly NavigationView _navigationView;
    private readonly ContentControl _contentHost;

    public MainWindow()
    {
        InitializeComponent();

        _navigationView = this.FindControl<NavigationView>("NavigationView")
                            ?? throw new InvalidOperationException("NavigationView not found in XAML.");
        _contentHost = this.FindControl<ContentControl>("ContentHost")
                        ?? throw new InvalidOperationException("ContentHost not found in XAML.");

        _pageCache = new Dictionary<string, Control>
        {
            ["Overview"] = new OverviewPage(),
            ["Rules"] = new RulesWorkspacePage(),
            ["Browsers"] = new BrowsersProfilesPage(),
            ["Default"] = new DefaultRulePage(),
            ["ImportExport"] = new ImportExportPage(),
            ["Advanced"] = new AdvancedUtilitiesPage()
        };

        _navigationView.SelectionChanged += OnSelectionChanged;

        if (_navigationView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault() is { } firstItem)
        {
            _navigationView.SelectedItem = firstItem;
            if (firstItem.Tag is string tag && _pageCache.TryGetValue(tag, out var initialView))
            {
                _contentHost.Content = initialView;
            }
        }
    }

    private void OnSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs args)
    {
        var tag = (args.SelectedItem as NavigationViewItem)?.Tag?.ToString()
                  ?? args.SelectedItemContainer?.Tag?.ToString();
        if (tag is not null && _pageCache.TryGetValue(tag, out var view))
        {
            _contentHost.Content = view;
        }
    }
}

