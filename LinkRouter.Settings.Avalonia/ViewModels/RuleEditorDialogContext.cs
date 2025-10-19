using System.Collections.Generic;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinkRouter.Settings.ViewModels;

namespace LinkRouter.Settings.Avalonia.ViewModels;

public sealed partial class RuleEditorDialogContext : ObservableObject
{
    public RuleEditorDialogContext(
        RuleEditorViewModel rule,
        IReadOnlyList<string> matchTypes,
        IReadOnlyList<string> profileOptions)
    {
        Rule = rule;
        MatchTypes = matchTypes;
        ProfileOptions = profileOptions;
        ClearUseProfileCommand = new RelayCommand(ClearUseProfile, CanClearUseProfile);
        Rule.PropertyChanged += OnRulePropertyChanged;
    }

    public RuleEditorViewModel Rule { get; }

    public IReadOnlyList<string> MatchTypes { get; }

    public IReadOnlyList<string> ProfileOptions { get; }

    public IRelayCommand ClearUseProfileCommand { get; }

    private void ClearUseProfile() => Rule.UseProfile = null;

    private bool CanClearUseProfile() => !string.IsNullOrWhiteSpace(Rule.UseProfile);

    private void OnRulePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RuleEditorViewModel.UseProfile))
        {
            ClearUseProfileCommand.NotifyCanExecuteChanged();
        }
    }
}
