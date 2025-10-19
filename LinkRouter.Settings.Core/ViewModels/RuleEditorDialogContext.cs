using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LinkRouter.Settings.ViewModels;

public sealed partial class RuleEditorDialogContext : ObservableObject
{
    public RuleEditorDialogContext(
        RuleEditorViewModel rule,
        IReadOnlyList<string> matchTypes,
        IReadOnlyList<string> profileOptions)
    {
        Rule = rule ?? throw new ArgumentNullException(nameof(rule));
        MatchTypes = matchTypes ?? Array.Empty<string>();
        ProfileOptions = profileOptions ?? Array.Empty<string>();
    }

    public RuleEditorViewModel Rule { get; }

    public IReadOnlyList<string> MatchTypes { get; }

    public IReadOnlyList<string> ProfileOptions { get; }

    [RelayCommand]
    private void ClearUseProfile() => Rule.UseProfile = null;
}
