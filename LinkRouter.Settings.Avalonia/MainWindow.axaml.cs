using System.Collections.Generic;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using LinkRouter.Settings.Avalonia.Views;

namespace LinkRouter.Settings.Avalonia;

public partial class MainWindow : Window
{
    private readonly Dictionary<string, Control> _pages = new()
    {
        ["Overview"] = new OverviewPage(),
        ["Rules"] = new RulesPage(),
        ["Profiles"] = new ProfilesPage(),
        ["Default"] = new DefaultPage(),
        ["ImportExport"] = new ImportExportPage(),
        ["Advanced"] = new AdvancedPage(),
        ["About"] = new AboutPage()
    };

    public MainWindow()
    {
        InitializeComponent();
        if (NavigationView.MenuItems.Count > 0)
        {
            NavigationView.SelectedItem = NavigationView.MenuItems[0];
        }
    }

    private void OnSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (e.SelectedItem is NavigationViewItem item && item.Tag is string tag && _pages.TryGetValue(tag, out var page))
        {
            ContentHost.Content = page;
        }
    }
}
