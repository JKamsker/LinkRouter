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
        Rule = rule;
        MatchTypes = new List<string>(matchTypes);
        ProfileOptions = new List<string>(profileOptions);
    }

    public RuleEditorViewModel Rule { get; }

    public IReadOnlyList<string> MatchTypes { get; }

    public IReadOnlyList<string> ProfileOptions { get; }

    [RelayCommand]
    private void ClearUseProfile()
    {
        Rule.UseProfile = null;
    }
}
