using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using LinkRouter.Settings.ViewModels;

namespace LinkRouter.Settings.Avalonia.Views;

public partial class RuleEditorDialog : Window, IRuleEditorDialog
{
    private TaskCompletionSource? _completionSource;

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

    public Task ShowAsync(Window? owner)
    {
        if (owner is not null)
        {
            return ShowDialog(owner);
        }

        _completionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        void OnClosed(object? sender, EventArgs e)
        {
            Closed -= OnClosed;
            _completionSource?.TrySetResult();
        }

        Closed += OnClosed;
        Show();
        return _completionSource.Task;
    }

    private void OnClearUseProfileClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is RuleEditorViewModel rule)
        {
            rule.UseProfile = null;
        }

        UseProfileCombo.SelectedItem = null;
    }

    private void OnDoneClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
