using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using LinkRouter.Settings.ViewModels;

namespace LinkRouter.Settings.Avalonia.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel(
        GeneralViewModel general,
        RulesViewModel rules,
        ProfilesViewModel profiles,
        ImportExportViewModel importExport,
        AdvancedViewModel advanced,
        AboutViewModel about)
    {
        Items = new ObservableCollection<NavigationItem>
        {
            new("overview", "Overview", general),
            new("rules", "Rules", rules),
            new("profiles", "Browsers & Profiles", profiles),
            new("import", "Import / Export", importExport),
            new("advanced", "Advanced", advanced),
            new("about", "About", about)
        };

        SelectedItem = Items.FirstOrDefault();
    }

    public ObservableCollection<NavigationItem> Items { get; }

    [ObservableProperty]
    private NavigationItem? _selectedItem;

    public object? CurrentViewModel => SelectedItem?.Content;

    partial void OnSelectedItemChanged(NavigationItem? value)
    {
        OnPropertyChanged(nameof(CurrentViewModel));
    }
}

public sealed record NavigationItem(string Id, string Title, object Content);
