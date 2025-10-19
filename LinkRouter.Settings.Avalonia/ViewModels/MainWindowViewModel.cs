using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using LinkRouter.Settings.ViewModels;

namespace LinkRouter.Settings.Avalonia.ViewModels;

public sealed partial class NavigationItemViewModel : ObservableObject
{
    public NavigationItemViewModel(string key, string title, object viewModel)
    {
        Key = key;
        Title = title;
        ViewModel = viewModel;
    }

    public string Key { get; }

    public string Title { get; }

    public object ViewModel { get; }
}

public sealed partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private NavigationItemViewModel? _selectedItem;

    public MainWindowViewModel(
        GeneralViewModel general,
        RulesViewModel rules,
        ProfilesViewModel profiles,
        ImportExportViewModel importExport,
        AdvancedViewModel advanced,
        AboutViewModel about)
    {
        NavigationItems = new ObservableCollection<NavigationItemViewModel>
        {
            new("overview", "Overview", general),
            new("rules", "Rules", rules),
            new("profiles", "Browsers & Profiles", profiles),
            new("import", "Import / Export", importExport),
            new("advanced", "Advanced", advanced),
            new("about", "About", about)
        };

        SelectedItem = NavigationItems.FirstOrDefault();
    }

    internal MainWindowViewModel(IEnumerable<NavigationItemViewModel> items)
    {
        NavigationItems = new ObservableCollection<NavigationItemViewModel>(items);
        SelectedItem = NavigationItems.FirstOrDefault();
    }

    public ObservableCollection<NavigationItemViewModel> NavigationItems { get; }

    public object? CurrentContent => SelectedItem?.ViewModel;

    public string Title => "LinkRouter Settings";

    partial void OnSelectedItemChanged(NavigationItemViewModel? value)
    {
        OnPropertyChanged(nameof(CurrentContent));
    }
}
