using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinkRouter.Settings.Services;

namespace LinkRouter.Settings.ViewModels;

public partial class RulesViewModel : ObservableObject
{
    private static readonly IReadOnlyList<string> s_matchTypes = new[] { "domain", "regex", "contains" };

    private readonly ConfigurationState _state;
    private readonly RuleTestService _tester;
    private readonly List<string> _profileOptions = new();
    private int _lastSelectedRuleIndex;

    [ObservableProperty]
    private RuleEditorViewModel? _selectedRule;

    [ObservableProperty]
    private string _testUrl = string.Empty;

    [ObservableProperty]
    private string? _testResult;

    [ObservableProperty]
    private string? _testError;

    [ObservableProperty]
    private RuleEditorDialogViewModel? _activeEditor;

    public RulesViewModel(
        ConfigurationState state,
        RuleTestService tester)
    {
        _state = state;
        _tester = tester;
        _state.StateChanged += OnStateChanged;
        _state.Rules.CollectionChanged += OnRulesCollectionChanged;
        RefreshProfileOptions();
        NotifyActionCommandStates();
    }

    public ObservableCollection<RuleEditorViewModel> Rules => _state.Rules;

    public IReadOnlyList<string> MatchTypes => s_matchTypes;

    public IReadOnlyList<string> ProfileOptions => _profileOptions;

    public bool HasSelectedRule => SelectedRule is not null;

    public bool IsEditorOpen => ActiveEditor is not null;

    [RelayCommand]
    private void AddRule()
    {
        var vm = new RuleEditorViewModel();
        vm.Match = "domain";
        vm.Pattern = "example.com";
        _state.AddRule(vm);
        SelectedRule = vm;
    }

    [RelayCommand(CanExecute = nameof(CanModifyRule))]
    private void DeleteRule(RuleEditorViewModel? rule)
    {
        if (rule is null)
        {
            return;
        }

        _state.RemoveRule(rule);
        if (ReferenceEquals(SelectedRule, rule))
        {
            SelectedRule = null;
        }
    }

    [RelayCommand(CanExecute = nameof(CanModifyRule))]
    private void DuplicateRule(RuleEditorViewModel? rule)
    {
        if (rule is null)
        {
            return;
        }

        var clone = rule.Clone();
        clone.Pattern = clone.Pattern + " copy";
        _state.AddRule(clone);
        SelectedRule = clone;
    }

    [RelayCommand(CanExecute = nameof(CanMoveUp))]
    private void MoveUp(RuleEditorViewModel? rule)
    {
        if (rule is null)
        {
            return;
        }

        var index = Rules.IndexOf(rule);
        if (index > 0)
        {
            _state.MoveRule(index, index - 1);
        }

        NotifyActionCommandStates();
    }

    [RelayCommand(CanExecute = nameof(CanMoveDown))]
    private void MoveDown(RuleEditorViewModel? rule)
    {
        if (rule is null)
        {
            return;
        }

        var index = Rules.IndexOf(rule);
        if (index >= 0 && index < Rules.Count - 1)
        {
            _state.MoveRule(index, index + 1);
        }

        NotifyActionCommandStates();
    }

    [RelayCommand(CanExecute = nameof(CanTestRule))]
    private void TestRule()
    {
        TestError = null;
        TestResult = null;

        if (SelectedRule is null)
        {
            TestError = "Select a rule to test.";
            return;
        }

        if (string.IsNullOrWhiteSpace(TestUrl))
        {
            TestError = "Enter a URL to test.";
            return;
        }

        try
        {
            var config = _state.BuildConfig();
            var result = _tester.Test(config, TestUrl);
            if (!result.Success)
            {
                TestError = result.Error;
                return;
            }

            TestResult = result.EffectiveRule is null
                ? "No rule resolved."
                : $"Browser: {result.EffectiveRule.browser}\nArgs: {result.LaunchArguments}";
        }
        catch (Exception ex)
        {
            TestError = ex.Message;
        }
    }

    [RelayCommand(CanExecute = nameof(CanClearUseProfile))]
    private void ClearUseProfile()
    {
        if (SelectedRule is null)
        {
            return;
        }

        SelectedRule.UseProfile = null;
    }

    private bool CanClearUseProfile() => SelectedRule?.UseProfile is not null;

    [RelayCommand]
    private Task EditRuleAsync()
    {
        if (SelectedRule is null)
        {
            return Task.CompletedTask;
        }

        if (!ReferenceEquals(ActiveEditor?.TargetRule, SelectedRule))
        {
            ActiveEditor = new RuleEditorDialogViewModel(SelectedRule, MatchTypes, ProfileOptions);
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    private void SaveEditor()
    {
        if (ActiveEditor is null)
        {
            return;
        }

        ActiveEditor.CommitChanges();
        ActiveEditor = null;
    }

    [RelayCommand]
    private void CancelEditor()
    {
        if (ActiveEditor is null)
        {
            return;
        }

        ActiveEditor.ResetChanges();
        ActiveEditor = null;
    }

    private void RefreshProfileOptions()
    {
        var options = new List<string>();

        foreach (var profile in _state.Profiles)
        {
            if (!string.IsNullOrWhiteSpace(profile.Name))
            {
                options.Add(profile.Name);
            }
        }

        options.Sort(StringComparer.OrdinalIgnoreCase);

        if (options.Count == _profileOptions.Count)
        {
            var identical = true;
            for (var i = 0; i < options.Count; i++)
            {
                if (!string.Equals(options[i], _profileOptions[i], StringComparison.Ordinal))
                {
                    identical = false;
                    break;
                }
            }

            if (identical)
            {
                return;
            }
        }

        _profileOptions.Clear();
        _profileOptions.AddRange(options);
        OnPropertyChanged(nameof(ProfileOptions));
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        RefreshProfileOptions();
        OnPropertyChanged(nameof(HasSelectedRule));
        NotifyActionCommandStates();
        RestoreSelectedRule();
    }

    partial void OnSelectedRuleChanging(RuleEditorViewModel? value)
    {
        if (SelectedRule is not null)
        {
            SelectedRule.PropertyChanged -= OnSelectedRulePropertyChanged;
        }
    }

    partial void OnSelectedRuleChanged(RuleEditorViewModel? value)
    {
        if (value is not null)
        {
            value.PropertyChanged += OnSelectedRulePropertyChanged;
            var index = Rules.IndexOf(value);
            if (index >= 0)
            {
                _lastSelectedRuleIndex = index;
            }
        }

        if (!ReferenceEquals(value, ActiveEditor?.TargetRule))
        {
            ActiveEditor = null;
        }

        OnPropertyChanged(nameof(HasSelectedRule));
        ClearUseProfileCommand.NotifyCanExecuteChanged();
        NotifyActionCommandStates();
    }

    private void OnSelectedRulePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RuleEditorViewModel.UseProfile))
        {
            ClearUseProfileCommand.NotifyCanExecuteChanged();
        }
    }

    partial void OnActiveEditorChanged(RuleEditorDialogViewModel? value)
    {
        OnPropertyChanged(nameof(IsEditorOpen));
    }

    private void RestoreSelectedRule()
    {
        if (Rules.Count == 0)
        {
            if (SelectedRule is not null)
            {
                SelectedRule = null;
            }
            return;
        }

        if (_lastSelectedRuleIndex < 0)
        {
            _lastSelectedRuleIndex = 0;
        }

        if (_lastSelectedRuleIndex >= Rules.Count)
        {
            _lastSelectedRuleIndex = Rules.Count - 1;
        }

        var target = Rules[_lastSelectedRuleIndex];
        if (!ReferenceEquals(target, SelectedRule))
        {
            SelectedRule = target;
        }
    }

    private bool CanModifyRule(RuleEditorViewModel? rule) => rule is not null;

    private bool CanMoveUp(RuleEditorViewModel? rule)
    {
        if (rule is null)
        {
            return false;
        }

        var index = Rules.IndexOf(rule);
        return index > 0;
    }

    private bool CanMoveDown(RuleEditorViewModel? rule)
    {
        if (rule is null)
        {
            return false;
        }

        var index = Rules.IndexOf(rule);
        return index >= 0 && index < Rules.Count - 1;
    }

    private bool CanTestRule() => SelectedRule is not null;

    private void NotifyActionCommandStates()
    {
        DeleteRuleCommand.NotifyCanExecuteChanged();
        DuplicateRuleCommand.NotifyCanExecuteChanged();
        MoveUpCommand.NotifyCanExecuteChanged();
        MoveDownCommand.NotifyCanExecuteChanged();
        TestRuleCommand.NotifyCanExecuteChanged();
    }

    private void OnRulesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        NotifyActionCommandStates();
    }
}
