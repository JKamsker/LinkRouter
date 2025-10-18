using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using LinkRouter.Settings.Avalonia.Views;

namespace LinkRouter.Settings.Avalonia;

public partial class MainWindow : Window
{
    private readonly Dictionary<string, Control> _pageCache = new();
    private NavigationView? _navigationView;
    private ContentControl? _contentHost;

    public MainWindow()
    {
        InitializeComponent();
        _navigationView = this.FindControl<NavigationView>("Shell");
        _contentHost = this.FindControl<ContentControl>("ContentHost");

        if (_navigationView?.MenuItems.FirstOrDefault() is NavigationViewItem firstItem)
        {
            _navigationView.SelectedItem = firstItem;
            Navigate(firstItem.Tag?.ToString());
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnNavigationSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
    {
        var tag = e.SelectedItemContainer?.Tag?.ToString();
        Navigate(tag);
    }

    private void Navigate(string? tag)
    {
        if (string.IsNullOrWhiteSpace(tag) || _contentHost is null)
        {
            return;
        }

        if (!_pageCache.TryGetValue(tag, out var control))
        {
            control = tag switch
            {
                "overview" => new OverviewPage(),
                "rules" => new RulesPage(),
                "profiles" => new ProfilesPage(),
                "default" => new DefaultRulePage(),
                "import" => new ImportExportPage(),
                "advanced" => new AdvancedPage(),
                _ => new OverviewPage()
            };

            _pageCache[tag] = control;
        }

        _contentHost.Content = control;
    }
}