using System.Threading.Tasks;
using LinkRouter.Settings.Services;
using LinkRouter.Settings.Services.Abstractions;
using LinkRouter.Settings.ViewModels;
using Xunit;

namespace LinkRouter.Settings.Avalonia.Tests.Rules;

public class RulesWorkspacePageTests
{
    [Fact]
    public async Task EditRuleCommand_InvokesDialogService()
    {
        var state = new ConfigurationState();
        var tester = new RuleTestService();
        var dialog = new StubDialogService();
        var viewModel = new RulesViewModel(state, tester, dialog);

        var rule = new RuleEditorViewModel
        {
            Match = "domain",
            Pattern = "example.com"
        };

        state.AddRule(rule);
        viewModel.SelectedRule = rule;

        await viewModel.EditRuleCommand.ExecuteAsync(null);

        Assert.True(dialog.Invoked);
        Assert.NotNull(dialog.LastViewModel);
        Assert.Same(rule, dialog.LastViewModel!.Rule);
    }

    [Fact]
    public async Task EditRuleCommand_WithoutSelection_DoesNotInvokeDialog()
    {
        var state = new ConfigurationState();
        var tester = new RuleTestService();
        var dialog = new StubDialogService();
        var viewModel = new RulesViewModel(state, tester, dialog);

        await viewModel.EditRuleCommand.ExecuteAsync(null);

        Assert.False(dialog.Invoked);
        Assert.Null(dialog.LastViewModel);
    }

    private sealed class StubDialogService : IRuleEditorDialogService
    {
        public bool Invoked { get; private set; }

        public RuleEditorDialogViewModel? LastViewModel { get; private set; }

        public Task<bool> EditRuleAsync(RuleEditorDialogViewModel viewModel)
        {
            Invoked = true;
            LastViewModel = viewModel;
            return Task.FromResult(true);
        }
    }
}
