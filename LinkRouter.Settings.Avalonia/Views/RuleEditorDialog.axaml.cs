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

    public new Task<ContentDialogResult> ShowAsync(Window? owner)
    {
        return owner is null
            ? base.ShowAsync()
            : base.ShowAsync(owner);
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
