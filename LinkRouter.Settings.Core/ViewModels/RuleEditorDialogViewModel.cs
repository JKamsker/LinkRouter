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
        OriginalRule = rule;
        Rule = rule.Clone();
        MatchTypes = new List<string>(matchTypes);
        ProfileOptions = new List<string>(profileOptions);
    }

    public RuleEditorViewModel OriginalRule { get; }

    public RuleEditorViewModel Rule { get; }

    public IReadOnlyList<string> MatchTypes { get; }

    public IReadOnlyList<string> ProfileOptions { get; }

    [RelayCommand]
    private void ClearUseProfile()
    {
        Rule.UseProfile = null;
    }

    public void CommitEdits()
    {
        OriginalRule.Enabled = Rule.Enabled;
        OriginalRule.Match = Rule.Match;
        OriginalRule.Pattern = Rule.Pattern;
        OriginalRule.Browser = Rule.Browser;
        OriginalRule.ArgsTemplate = Rule.ArgsTemplate;
        OriginalRule.Profile = Rule.Profile;
        OriginalRule.UserDataDir = Rule.UserDataDir;
        OriginalRule.WorkingDirectory = Rule.WorkingDirectory;
        OriginalRule.UseProfile = Rule.UseProfile;
    }
}
