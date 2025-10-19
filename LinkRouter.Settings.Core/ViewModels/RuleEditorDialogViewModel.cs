using System;
using System.Collections.Generic;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LinkRouter.Settings.ViewModels;

public sealed partial class RuleEditorDialogViewModel : ObservableObject, IDisposable
{
    public RuleEditorDialogViewModel(
        RuleEditorViewModel rule,
        IReadOnlyList<string> matchTypes,
        IReadOnlyList<string> profileOptions)
    {
        Rule = rule;
        MatchTypes = matchTypes;
        ProfileOptions = profileOptions;
        Rule.PropertyChanged += OnRulePropertyChanged;
    }

    public RuleEditorViewModel Rule { get; }

    public IReadOnlyList<string> MatchTypes { get; }

    public IReadOnlyList<string> ProfileOptions { get; }

    [RelayCommand(CanExecute = nameof(CanClearProfile))]
    private void ClearProfile()
    {
        Rule.UseProfile = null;
    }

    private bool CanClearProfile() => !string.IsNullOrWhiteSpace(Rule.UseProfile);

    private void OnRulePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RuleEditorViewModel.UseProfile))
        {
            ClearProfileCommand.NotifyCanExecuteChanged();
        }
    }

    public void Dispose()
    {
        Rule.PropertyChanged -= OnRulePropertyChanged;
    }
}
