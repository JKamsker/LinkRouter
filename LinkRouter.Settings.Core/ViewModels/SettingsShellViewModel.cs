using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LinkRouter.Settings.ViewModels;

public sealed partial class SettingsShellViewModel : ObservableObject
{
    private NavigationItemViewModel? _selectedItem;

    public SettingsShellViewModel(
        GeneralViewModel generalViewModel,
        RulesViewModel rulesViewModel,
        ProfilesViewModel profilesViewModel,
        ImportExportViewModel importExportViewModel,
        AdvancedViewModel advancedViewModel,
        AboutViewModel aboutViewModel)
    {
        Items = new ObservableCollection<NavigationItemViewModel>
        {
            new("Overview", "overview", generalViewModel, NavigationGlyphs.Home),
            new("Rules", "rules", rulesViewModel, NavigationGlyphs.Rule),
            new("Browsers & Profiles", "profiles", profilesViewModel, NavigationGlyphs.Browser),
            new("Import / Export", "import", importExportViewModel, NavigationGlyphs.Transfer),
            new("Advanced", "advanced", advancedViewModel, NavigationGlyphs.Settings),
            new("About", "about", aboutViewModel, NavigationGlyphs.Info)
        };

        _selectedItem = Items.FirstOrDefault();
    }

    public ObservableCollection<NavigationItemViewModel> Items { get; }

    public NavigationItemViewModel? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
            {
                OnPropertyChanged(nameof(ActiveContent));
            }
        }
    }

    public object? ActiveContent => SelectedItem?.Content;
}

public sealed class NavigationItemViewModel
{
    public NavigationItemViewModel(string title, string key, object content, string? glyph = null)
    {
        Title = title;
        Key = key;
        Content = content;
        Glyph = glyph;
    }

    public string Title { get; }

    public string Key { get; }

    public object Content { get; }

    public string? Glyph { get; }
}

public static class NavigationGlyphs
{
    public const string Home = "\uE80F";
    public const string Rule = "\uE16F";
    public const string Browser = "\uE774";
    public const string Transfer = "\uE895";
    public const string Settings = "\uE713";
    public const string Info = "\uE946";
}
