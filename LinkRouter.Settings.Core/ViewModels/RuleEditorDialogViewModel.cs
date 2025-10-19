using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LinkRouter.Settings.ViewModels;

public sealed partial class RuleEditorDialogViewModel : ObservableObject
{
    public RuleEditorDialogViewModel(
        RuleEditorViewModel rule,
        IReadOnlyList<string> matchTypes,
        IReadOnlyList<string> profileOptions)
    {
        Rule = rule;
        MatchTypes = matchTypes;
        ProfileOptions = profileOptions;
    }

    public RuleEditorViewModel Rule { get; }

    public IReadOnlyList<string> MatchTypes { get; }

    public IReadOnlyList<string> ProfileOptions { get; }

    [RelayCommand]
    private void ClearProfile()
    {
        Rule.UseProfile = null;
    }
}
