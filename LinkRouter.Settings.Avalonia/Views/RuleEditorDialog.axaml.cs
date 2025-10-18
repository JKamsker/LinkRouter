using System;
using System.Collections.Generic;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using LinkRouter.Settings.ViewModels;

namespace LinkRouter.Settings.Avalonia.Views;

public partial class RuleEditorDialog : ContentDialog
{
    public IReadOnlyList<string> MatchTypes { get; set; } = Array.Empty<string>();

    public IReadOnlyList<string> ProfileOptions { get; set; } = Array.Empty<string>();

    public RuleEditorDialog()
    {
        InitializeComponent();
    }

    private void OnClearUseProfileClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is RuleEditorViewModel rule)
        {
            rule.UseProfile = null;
        }
    }
}
