using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LinkRouter.Settings.ViewModels;

public sealed partial class SettingsShellViewModel : ObservableObject
{
    public SettingsShellViewModel(
        GeneralViewModel general,
        RulesViewModel rules,
        ProfilesViewModel profiles,
        ImportExportViewModel importExport,
        AdvancedViewModel advanced,
        AboutViewModel about)
    {
        Pages = new[]
        {
            new SettingsPage("Overview", general),
            new SettingsPage("Rules", rules),
            new SettingsPage("Browsers & Profiles", profiles),
            new SettingsPage("Import / Export", importExport),
            new SettingsPage("Advanced", advanced),
            new SettingsPage("About", about)
        };

        SelectedPage = Pages.FirstOrDefault();
    }

    public IReadOnlyList<SettingsPage> Pages { get; }

    [ObservableProperty]
    private SettingsPage? _selectedPage;

    public object? ActiveContent => SelectedPage?.Content;

    partial void OnSelectedPageChanged(SettingsPage? value)
    {
        OnPropertyChanged(nameof(ActiveContent));
    }

    public sealed record SettingsPage(string Title, object Content);
}
