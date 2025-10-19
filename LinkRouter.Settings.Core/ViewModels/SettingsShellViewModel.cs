using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LinkRouter.Settings.ViewModels;

public sealed partial class SettingsShellViewModel : ObservableObject
{
    private readonly IReadOnlyList<SettingsNavigationItem> _items;

    public SettingsShellViewModel(
        GeneralViewModel overview,
        RulesViewModel rules,
        ProfilesViewModel profiles,
        ImportExportViewModel importExport,
        AdvancedViewModel advanced,
        AboutViewModel about)
    {
        _items = new[]
        {
            new SettingsNavigationItem("Overview", overview),
            new SettingsNavigationItem("Rules", rules),
            new SettingsNavigationItem("Browsers & Profiles", profiles),
            new SettingsNavigationItem("Import / Export", importExport),
            new SettingsNavigationItem("Advanced", advanced),
            new SettingsNavigationItem("About", about)
        };

        SelectedItem = _items[0];
    }

    public IReadOnlyList<SettingsNavigationItem> Items => _items;

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
