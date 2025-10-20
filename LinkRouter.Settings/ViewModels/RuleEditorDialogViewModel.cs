using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LinkRouter.Settings.ViewModels;

public partial class RuleEditorDialogViewModel : ObservableObject
{
    public RuleEditorDialogViewModel(
        RuleEditorViewModel rule,
        IEnumerable<string> matchTypes,
        IEnumerable<string> profileOptions)
    {
        TargetRule = rule;
        Rule = rule.Clone();
        MatchTypes = new List<string>(matchTypes);
        ProfileOptions = new List<string>(profileOptions);
    }

    public RuleEditorViewModel TargetRule { get; }
    public RuleEditorViewModel Rule { get; }

    public IReadOnlyList<string> MatchTypes { get; }

    public IReadOnlyList<string> ProfileOptions { get; }

    [RelayCommand]
    private void ClearUseProfile()
    {
        Rule.UseProfile = null;
    }

    public void CommitChanges()
    {
        TargetRule.CopyFrom(Rule);
    }

    public void ResetChanges()
    {
        Rule.CopyFrom(TargetRule);
    }
}
