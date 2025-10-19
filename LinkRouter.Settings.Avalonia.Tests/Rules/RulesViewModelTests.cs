using System.Collections.Generic;
using System.Threading.Tasks;
using LinkRouter.Settings.Services;
using LinkRouter.Settings.Services.Abstractions;
using LinkRouter.Settings.ViewModels;
using Xunit;

namespace LinkRouter.Settings.Avalonia.Tests.Rules;

public class RulesViewModelTests
{
    [Fact]
    public void EditRuleCommand_WhenNoSelection_CannotExecute()
    {
        var state = new ConfigurationState();
        var viewModel = CreateViewModel(state, new RecordingRuleEditorDialogService());

        Assert.False(viewModel.EditRuleCommand.CanExecute(null));
    }

    [Fact]
    public async Task EditRuleCommand_WhenRuleSelected_InvokesDialogService()
    {
        var state = new ConfigurationState();
        var dialog = new RecordingRuleEditorDialogService();
        var viewModel = CreateViewModel(state, dialog);

        var rule = new RuleEditorViewModel
        {
            Match = "domain",
            Pattern = "example.com"
        };

        state.AddRule(rule);
        viewModel.SelectedRule = rule;

        Assert.True(viewModel.EditRuleCommand.CanExecute(null));

        await viewModel.EditRuleCommand.ExecuteAsync(null);

        Assert.True(dialog.Invoked);
        Assert.Same(rule, dialog.Rule);
        Assert.NotNull(dialog.MatchTypes);
        Assert.NotNull(dialog.ProfileOptions);
    }

    private static RulesViewModel CreateViewModel(ConfigurationState state, IRuleEditorDialogService dialogService)
    {
        var tester = new RuleTestService();
        return new RulesViewModel(state, tester, dialogService);
    }

    private sealed class RecordingRuleEditorDialogService : IRuleEditorDialogService
    {
        public bool Invoked { get; private set; }
        public RuleEditorViewModel? Rule { get; private set; }
        public IReadOnlyList<string>? MatchTypes { get; private set; }
        public IReadOnlyList<string>? ProfileOptions { get; private set; }

        public Task<bool> EditRuleAsync(
            RuleEditorViewModel rule,
            IReadOnlyList<string> matchTypes,
            IReadOnlyList<string> profileOptions)
        {
            Invoked = true;
            Rule = rule;
            MatchTypes = matchTypes;
            ProfileOptions = profileOptions;
            return Task.FromResult(true);
        }
    }
}
