using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using LinkRouter.Settings.Avalonia.Views;
using LinkRouter.Settings.ViewModels;

namespace LinkRouter.Settings.Avalonia;

public partial class MainWindow : Window
{
    private readonly Dictionary<string, Control> _pages;

    public MainWindow()
    {
        InitializeComponent();

        _pages = new Dictionary<string, Control>
        {
            ["overview"] = CreatePage(new OverviewPage(), new GeneralViewModel()),
            ["rules"] = CreatePage(new RulesWorkspacePage(), new RulesViewModel()),
            ["profiles"] = CreatePage(new ProfilesPage(), new ProfilesViewModel()),
            ["default"] = CreatePage(new DefaultRulePage(), new DefaultViewModel()),
            ["import"] = CreatePage(new ImportExportPage(), new ImportExportViewModel()),
            ["advanced"] = CreatePage(new AdvancedPage(), new AdvancedViewModel()),
            ["about"] = CreatePage(new AboutPage(), new AboutViewModel())
        };

        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        if (NavView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault() is NavigationViewItem item)
        {
            NavView.SelectedItem = item;
        }
    }

    private static Control CreatePage(Control view, object viewModel)
    {
        view.DataContext = viewModel;
        return view;
    }

    private void OnSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (e.SelectedItem is NavigationViewItem item && item.Tag is string tag && _pages.TryGetValue(tag, out var page))
        {
            ContentHost.Content = page;
        }
    }
}