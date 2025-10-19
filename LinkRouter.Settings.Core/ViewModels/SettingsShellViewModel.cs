using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LinkRouter.Settings.ViewModels;

public sealed partial class SettingsShellViewModel : ObservableObject
{
    public SettingsShellViewModel(
        GeneralViewModel overview,
        RulesViewModel rules,
        ProfilesViewModel profiles,
        ImportExportViewModel importExport,
        AdvancedViewModel advanced,
        AboutViewModel about)
        : this(new[]
        {
            new SettingsNavigationItem("Overview", overview),
            new SettingsNavigationItem("Rules", rules),
            new SettingsNavigationItem("Browsers & Profiles", profiles),
            new SettingsNavigationItem("Import / Export", importExport),
            new SettingsNavigationItem("Advanced", advanced),
            new SettingsNavigationItem("About", about)
        })
    {
    }

    public SettingsShellViewModel(IEnumerable<SettingsNavigationItem> items)
    {
        NavigationItems = new ObservableCollection<SettingsNavigationItem>(items);
        if (NavigationItems.Count > 0)
        {
            SelectedItem = NavigationItems[0];
        }
    }

    public ObservableCollection<SettingsNavigationItem> NavigationItems { get; }

    [ObservableProperty]
    private SettingsNavigationItem? _selectedItem;

    public object? CurrentContent => SelectedItem?.Content;

    partial void OnSelectedItemChanged(SettingsNavigationItem? value)
    {
        OnPropertyChanged(nameof(CurrentContent));
    }
}

public sealed class SettingsNavigationItem
{
    public SettingsNavigationItem(string title, object content)
    {
        Title = title;
        Content = content;
    }

    public string Title { get; }

    public object Content { get; }
}
