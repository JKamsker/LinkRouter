using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinkRouter.Settings.Core.Services;
using LinkRouter.Settings.Services;

namespace LinkRouter.Settings.ViewModels;

public partial class RulesViewModel : ObservableObject
{
    private static readonly IReadOnlyList<string> s_matchTypes = new[] { "domain", "regex", "contains" };

    private readonly ConfigurationState _state = AppServices.ConfigurationState;
    private readonly RuleTestService _tester = AppServices.RuleTestService;

    [ObservableProperty]
    private RuleEditorViewModel? _selectedRule;

    [ObservableProperty]
    private string _testUrl = string.Empty;

    [ObservableProperty]
    private string? _testResult;

    [ObservableProperty]
    private string? _testError;

    public ObservableCollection<RuleEditorViewModel> Rules => _state.Rules;

    public IReadOnlyList<string> MatchTypes => s_matchTypes;

    [RelayCommand]
    private void AddRule()
    {
        var vm = new RuleEditorViewModel();
        vm.Match = "domain";
        vm.Pattern = "example.com";
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
                : $"Browser: {result.EffectiveRule.browser}\nArgs: {result.LaunchArguments}";
        }
        catch (Exception ex)
        {
            TestError = ex.Message;
        }
    }
}
