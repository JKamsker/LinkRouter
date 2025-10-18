using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using LinkRouter.Settings.ViewModels;

namespace LinkRouter.Settings.Avalonia.Views;

public partial class RuleEditorDialog : ContentDialog
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

    private void OnClearUseProfileClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is RuleEditorViewModel rule)
        {
            rule.UseProfile = null;
        }

        UseProfileCombo.SelectedItem = null;
    }
}
