using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinkRouter.Settings.Services;
using LinkRouter.Settings.Services.Abstractions;

namespace LinkRouter.Settings.ViewModels;

public partial class RulesViewModel : ObservableObject
{
    private static readonly IReadOnlyList<string> s_matchTypes = new[] { "domain", "regex", "contains" };

    private readonly ConfigurationState _state;
    private readonly RuleTestService _tester;
    private readonly IDialogService _dialogService;
    private readonly List<string> _profileOptions = new();

    [ObservableProperty]
    private RuleEditorViewModel? _selectedRule;

    [ObservableProperty]
    private string _testUrl = string.Empty;

    [ObservableProperty]
    private string? _testResult;

    [ObservableProperty]
    private string? _testError;

    public RulesViewModel(
        ConfigurationState state,
        RuleTestService tester,
        IDialogService dialogService)
    {
        _state = state;
        _tester = tester;
        _dialogService = dialogService;
        _state.StateChanged += OnStateChanged;
        RefreshProfileOptions();
    }

    public ObservableCollection<RuleEditorViewModel> Rules => _state.Rules;

    public IReadOnlyList<string> MatchTypes => s_matchTypes;

    public IReadOnlyList<string> ProfileOptions => _profileOptions;

    public bool HasSelectedRule => SelectedRule is not null;

    [RelayCommand]
    private void AddRule()
    {
        var vm = new RuleEditorViewModel
        {
            Match = "domain",
            Pattern = "example.com"
        };

        _state.AddRule(vm);
        SelectedRule = vm;
    }

    [RelayCommand]
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

    [RelayCommand]
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

    [RelayCommand]
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

    [RelayCommand]
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

    [RelayCommand]
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
                : $"Browser: {result.EffectiveRule.browser}\\nArgs: {result.LaunchArguments}";
        }
        catch (Exception ex)
        {
            TestError = ex.Message;
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditRule))]
    private async Task EditRuleAsync()
    {
        if (SelectedRule is null)
        {
            return;
        }

        var workingCopy = SelectedRule.Clone();
        var accepted = await _dialogService
            .ShowRuleEditorAsync(workingCopy, MatchTypes, ProfileOptions)
            .ConfigureAwait(false);

        if (accepted)
        {
            SelectedRule.UpdateFrom(workingCopy);
        }
    }

    private bool CanEditRule() => SelectedRule is not null;

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
        EditRuleCommand.NotifyCanExecuteChanged();
        ClearUseProfileCommand.NotifyCanExecuteChanged();
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

        OnPropertyChanged(nameof(HasSelectedRule));
        EditRuleCommand.NotifyCanExecuteChanged();
        ClearUseProfileCommand.NotifyCanExecuteChanged();
    }

    private void OnSelectedRulePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RuleEditorViewModel.UseProfile))
        {
            ClearUseProfileCommand.NotifyCanExecuteChanged();
        }
    }
}
