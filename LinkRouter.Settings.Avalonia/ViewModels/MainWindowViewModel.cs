using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using LinkRouter.Settings.ViewModels;

namespace LinkRouter.Settings.Avalonia.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private NavigationItemViewModel? _selectedItem;
    private object? _currentPage;

    public GeneralViewModel General { get; }

    public MainWindowViewModel(
        GeneralViewModel general,
        RulesViewModel rules,
        ProfilesViewModel profiles,
        ImportExportViewModel importExport,
        AdvancedViewModel advanced,
        AboutViewModel about)
    {
        General = general;
        NavigationItems = new ObservableCollection<NavigationItemViewModel>
        {
            new("overview", "Overview", general, Symbol.Home),
            new("rules", "Rules", rules, Symbol.Code),
            new("profiles", "Browsers & Profiles", profiles, Symbol.Globe),
            new("import", "Import / Export", importExport, Symbol.Sync),
            new("advanced", "Advanced", advanced, Symbol.Settings)
        };
        FooterItems = new ObservableCollection<NavigationItemViewModel>
        {
            new("about", "About", about, Symbol.Help)
        };

        SelectedItem = NavigationItems.FirstOrDefault();
    }

    public ObservableCollection<NavigationItemViewModel> NavigationItems { get; }
    public ObservableCollection<NavigationItemViewModel> FooterItems { get; }

    public NavigationItemViewModel? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
            {
                CurrentPage = value?.Content;
            }
        }
    }

    public object? CurrentPage
    {
        get => _currentPage;
        private set => SetProperty(ref _currentPage, value);
    }
}
