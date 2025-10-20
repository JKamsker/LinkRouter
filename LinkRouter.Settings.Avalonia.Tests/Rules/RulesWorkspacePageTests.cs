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
        Assert.Same(rule, viewModel.ActiveEditor!.OriginalRule);
        Assert.NotSame(rule, viewModel.ActiveEditor!.Rule);
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
    public async Task CancelEditorCommand_ClosesEditorWithoutChanges()
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

        viewModel.ActiveEditor!.Rule.Pattern = "changed.com";

        viewModel.CancelEditorCommand.Execute(null);

        Assert.False(viewModel.IsEditorOpen);
        Assert.Null(viewModel.ActiveEditor);
        Assert.Equal("example.com", rule.Pattern);
    }

    [Fact]
    public async Task SaveEditorCommand_CommitsBufferedChanges()
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

        viewModel.ActiveEditor!.Rule.Pattern = "updated.com";

        viewModel.SaveEditorCommand.Execute(null);

        Assert.False(viewModel.IsEditorOpen);
        Assert.Null(viewModel.ActiveEditor);
        Assert.Equal("updated.com", rule.Pattern);
    }

    [Fact]
    public void RuleCommands_RespectSelectionAndPosition()
    {
        var state = new ConfigurationState();
        var tester = new RuleTestService();
        var viewModel = new RulesViewModel(state, tester);

        var firstRule = new RuleEditorViewModel
        {
            Match = "domain",
            Pattern = "example.com"
        };

        var secondRule = new RuleEditorViewModel
        {
            Match = "domain",
            Pattern = "sample.com"
        };

        state.AddRule(firstRule);
        state.AddRule(secondRule);

        Assert.False(viewModel.DeleteRuleCommand.CanExecute(null));
        Assert.False(viewModel.DuplicateRuleCommand.CanExecute(null));
        Assert.False(viewModel.MoveUpCommand.CanExecute(null));
        Assert.False(viewModel.MoveDownCommand.CanExecute(null));
        Assert.False(viewModel.TestRuleCommand.CanExecute(null));

        viewModel.SelectedRule = firstRule;

        Assert.True(viewModel.DeleteRuleCommand.CanExecute(firstRule));
        Assert.True(viewModel.DuplicateRuleCommand.CanExecute(firstRule));
        Assert.False(viewModel.MoveUpCommand.CanExecute(firstRule));
        Assert.True(viewModel.MoveDownCommand.CanExecute(firstRule));
        Assert.False(viewModel.TestRuleCommand.CanExecute(null));

        viewModel.TestUrl = "https://example.com";
        Assert.True(viewModel.TestRuleCommand.CanExecute(null));

        viewModel.SelectedRule = secondRule;

        Assert.True(viewModel.MoveUpCommand.CanExecute(secondRule));
        Assert.False(viewModel.MoveDownCommand.CanExecute(secondRule));

        viewModel.SelectedRule = null;

        Assert.False(viewModel.DeleteRuleCommand.CanExecute(null));
        Assert.False(viewModel.DuplicateRuleCommand.CanExecute(null));
        Assert.False(viewModel.MoveUpCommand.CanExecute(null));
        Assert.False(viewModel.MoveDownCommand.CanExecute(null));
        Assert.False(viewModel.TestRuleCommand.CanExecute(null));
    }
}
