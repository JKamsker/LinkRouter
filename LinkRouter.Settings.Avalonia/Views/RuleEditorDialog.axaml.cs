using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using LinkRouter.Settings.ViewModels;

namespace LinkRouter.Settings.Avalonia.Views;

public partial class RuleEditorDialog : ContentDialog, IRuleEditorDialog
{
    public RuleEditorDialog()
    {
        InitializeComponent();
    }

    public void Configure(RuleEditorViewModel rule, IEnumerable<string> matchTypes, IEnumerable<string> profileOptions)
    {
        DataContext = rule;
        MatchTypeCombo.ItemsSource = matchTypes;
        UseProfileCombo.ItemsSource = profileOptions;
    }

    public new async Task ShowAsync(Window? owner)
    {
        if (owner is not null)
        {
            await base.ShowAsync(owner);
            return;
        }

        await base.ShowAsync();
    }

    private void OnClearUseProfileClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is RuleEditorViewModel rule)
        {
            rule.UseProfile = null;
        }

        UseProfileCombo.SelectedItem = null;
    }
}
