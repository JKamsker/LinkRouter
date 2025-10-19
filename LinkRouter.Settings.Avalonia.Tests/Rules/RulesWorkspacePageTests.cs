using System.Threading.Tasks;
using LinkRouter.Settings.Services;
using LinkRouter.Settings.ViewModels;
using Xunit;

namespace LinkRouter.Settings.Avalonia.Tests.Rules;

public class RulesWorkspacePageTests
{
    [Fact]
    public async Task EditRuleCommand_WithSelection_OpensInlineEditor()
    {
        var state = new ConfigurationState();
        var tester = new RuleTestService();
        var viewModel = new RulesViewModel(state, tester);

        var rule = new RuleEditorViewModel
        {
            Match = "domain",
            Pattern = "example.com"
        };

        state.AddRule(rule);
        viewModel.SelectedRule = rule;

        await viewModel.EditRuleCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsEditorOpen);
        Assert.NotNull(viewModel.ActiveEditor);
        Assert.Same(rule, viewModel.ActiveEditor!.Rule);
    }

    [Fact]
    public async Task EditRuleCommand_WithoutSelection_DoesNotOpenEditor()
    {
        var state = new ConfigurationState();
        var tester = new RuleTestService();
        var viewModel = new RulesViewModel(state, tester);

        await viewModel.EditRuleCommand.ExecuteAsync(null);

        Assert.False(viewModel.IsEditorOpen);
        Assert.Null(viewModel.ActiveEditor);
    }

    [Fact]
    public async Task CloseEditorCommand_ClosesEditor()
    {
        var state = new ConfigurationState();
        var tester = new RuleTestService();
        var viewModel = new RulesViewModel(state, tester);

        var rule = new RuleEditorViewModel
        {
            Match = "domain",
            Pattern = "example.com"
        };

        state.AddRule(rule);
        viewModel.SelectedRule = rule;

        await viewModel.EditRuleCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsEditorOpen);

        viewModel.CloseEditorCommand.Execute(null);

        Assert.False(viewModel.IsEditorOpen);
        Assert.Null(viewModel.ActiveEditor);
    }
}
