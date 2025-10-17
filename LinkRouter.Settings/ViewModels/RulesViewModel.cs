using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinkRouter.Settings.Services;

namespace LinkRouter.Settings.ViewModels;

public partial class RulesViewModel : ObservableObject
{
    private static readonly IReadOnlyList<string> s_matchTypes = new[] { "domain", "regex", "contains" };
    private static readonly IReadOnlyList<string> s_sortOptions = new[] { "Custom order", "Match type", "Pattern (A-Z)" };

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

    [ObservableProperty]
    private string _filterText = string.Empty;

    [ObservableProperty]
    private string _selectedSortOption = s_sortOptions[0];

    public ObservableCollection<RuleEditorViewModel> Rules => _state.Rules;

    public IReadOnlyList<string> MatchTypes => s_matchTypes;

    public IReadOnlyList<string> SortOptions => s_sortOptions;

    public IEnumerable<RuleEditorViewModel> FilteredRules => BuildFilteredRules();

    public bool HasFilter => !string.IsNullOrWhiteSpace(FilterText);

    public bool HasFilteredResults => FilteredRules.Any();

    public IEnumerable<string> Profiles => _state.Profiles
        .Select(profile => profile.Name)
        .Where(name => !string.IsNullOrWhiteSpace(name))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(name => name, StringComparer.OrdinalIgnoreCase);

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

    partial void OnFilterTextChanged(string value)
    {
        OnPropertyChanged(nameof(FilteredRules));
        OnPropertyChanged(nameof(HasFilter));
        OnPropertyChanged(nameof(HasFilteredResults));
    }

    partial void OnSelectedSortOptionChanged(string value)
    {
        OnPropertyChanged(nameof(FilteredRules));
        OnPropertyChanged(nameof(HasFilteredResults));
    }

    public RulesViewModel()
    {
        _state.StateChanged += (_, _) => OnStateChanged();
    }

    private void OnStateChanged()
    {
        OnPropertyChanged(nameof(FilteredRules));
        OnPropertyChanged(nameof(HasFilter));
        OnPropertyChanged(nameof(HasFilteredResults));
        OnPropertyChanged(nameof(Profiles));
    }

    private IEnumerable<RuleEditorViewModel> BuildFilteredRules()
    {
        IEnumerable<RuleEditorViewModel> query = Rules;

        if (!string.IsNullOrWhiteSpace(FilterText))
        {
            var filter = FilterText.Trim();
            query = query.Where(rule =>
                rule.DisplayName.Contains(filter, StringComparison.OrdinalIgnoreCase)
                || (!string.IsNullOrWhiteSpace(rule.Browser) && rule.Browser.Contains(filter, StringComparison.OrdinalIgnoreCase))
                || (!string.IsNullOrWhiteSpace(rule.Profile) && rule.Profile.Contains(filter, StringComparison.OrdinalIgnoreCase)));
        }

        return SelectedSortOption switch
        {
            "Match type" => query.OrderBy(rule => rule.Match).ThenBy(rule => rule.Pattern, StringComparer.OrdinalIgnoreCase),
            "Pattern (A-Z)" => query.OrderBy(rule => rule.Pattern, StringComparer.OrdinalIgnoreCase),
            _ => query
        };
    }
}
