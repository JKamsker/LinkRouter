using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LinkRouter.Settings.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly ReadOnlyObservableCollection<NavigationItemViewModel> _pages;

    [ObservableProperty]
    private NavigationItemViewModel? _selectedPage;

    public MainWindowViewModel(IEnumerable<NavigationItemViewModel> pages)
    {
        var list = new ObservableCollection<NavigationItemViewModel>(pages);
        _pages = new ReadOnlyObservableCollection<NavigationItemViewModel>(list);
        SelectedPage = _pages.FirstOrDefault();
    }

    public ReadOnlyObservableCollection<NavigationItemViewModel> Pages => _pages;

    public object? CurrentContent => SelectedPage?.Content;

    public string? CurrentTitle => SelectedPage?.Title;

    partial void OnSelectedPageChanged(NavigationItemViewModel? value)
    {
        OnPropertyChanged(nameof(CurrentContent));
        OnPropertyChanged(nameof(CurrentTitle));
    }
}
