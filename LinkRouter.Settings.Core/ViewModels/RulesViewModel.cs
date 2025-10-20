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

        if (!ReferenceEquals(ActiveEditor?.OriginalRule, SelectedRule))
        {
            ActiveEditor = new RuleEditorDialogViewModel(SelectedRule, MatchTypes, ProfileOptions);
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    private void CancelEditor()
    {
        ActiveEditor = null;
    }

    [RelayCommand]
    private void SaveEditor()
    {
        if (ActiveEditor is null)
        {
            return;
        }

        ActiveEditor.Commit();
        ActiveEditor = null;
    }

    private void RefreshProfileOptions()
    {
        _profileOptions.Clear();

        foreach (var profile in _state.Profiles)
        {
            if (!string.IsNullOrWhiteSpace(profile.Name))
            {
                _profileOptions.Add(profile.Name);
            }
        }

        _profileOptions.Sort(StringComparer.OrdinalIgnoreCase);
        OnPropertyChanged(nameof(ProfileOptions));
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        RefreshProfileOptions();
        OnPropertyChanged(nameof(HasSelectedRule));
        NotifyRuleCommandsCanExecute();
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
        }

        if (!ReferenceEquals(value, ActiveEditor?.OriginalRule))
        {
            ActiveEditor = null;
        }

        OnPropertyChanged(nameof(HasSelectedRule));
        ClearUseProfileCommand.NotifyCanExecuteChanged();
        NotifyRuleCommandsCanExecute();
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

    private void OnRulesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        NotifyRuleCommandsCanExecute();
    }

    private bool CanModifyRule(RuleEditorViewModel? rule) => rule is not null;

    private bool CanMoveUp(RuleEditorViewModel? rule)
    {
        if (rule is null)
        {
            return false;
        }

        return Rules.IndexOf(rule) > 0;
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

    private bool CanTestRule() => SelectedRule is not null && !string.IsNullOrWhiteSpace(TestUrl);

    partial void OnTestUrlChanged(string value)
    {
        TestRuleCommand.NotifyCanExecuteChanged();
    }

    private void NotifyRuleCommandsCanExecute()
    {
        DeleteRuleCommand.NotifyCanExecuteChanged();
        DuplicateRuleCommand.NotifyCanExecuteChanged();
        MoveUpCommand.NotifyCanExecuteChanged();
        MoveDownCommand.NotifyCanExecuteChanged();
        TestRuleCommand.NotifyCanExecuteChanged();
    }
}
