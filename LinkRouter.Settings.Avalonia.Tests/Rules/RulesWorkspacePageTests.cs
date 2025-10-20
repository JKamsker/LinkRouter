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
        Assert.NotSame(rule, viewModel.ActiveEditor.Rule);
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

    [Fact]
    public async Task CloseEditorCommand_DiscardsPendingChanges()
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

        var editor = viewModel.ActiveEditor!;
        editor.Rule.Pattern = "changed.com";

        viewModel.CloseEditorCommand.Execute(null);

        Assert.Equal("example.com", rule.Pattern);
        Assert.False(viewModel.IsEditorOpen);
        Assert.Null(viewModel.ActiveEditor);
    }

    [Fact]
    public async Task ApplyEditorCommand_CommitsChanges()
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

        var editor = viewModel.ActiveEditor!;
        editor.Rule.Pattern = "changed.com";

        viewModel.ApplyEditorCommand.Execute(null);

        Assert.Equal("changed.com", rule.Pattern);
        Assert.False(viewModel.IsEditorOpen);
        Assert.Null(viewModel.ActiveEditor);
    }

    [Fact]
    public void RuleCommands_RespectSelectionAndPosition()
    {
        var state = new ConfigurationState();
        var tester = new RuleTestService();
        var viewModel = new RulesViewModel(state, tester);

        var first = new RuleEditorViewModel
        {
            Match = "domain",
            Pattern = "alpha.com"
        };

        var second = new RuleEditorViewModel
        {
            Match = "domain",
            Pattern = "beta.com"
        };

        state.AddRule(first);
        state.AddRule(second);

        Assert.False(viewModel.DeleteRuleCommand.CanExecute(null));
        Assert.False(viewModel.DuplicateRuleCommand.CanExecute(null));
        Assert.False(viewModel.MoveUpCommand.CanExecute(null));
        Assert.False(viewModel.MoveDownCommand.CanExecute(null));

        viewModel.SelectedRule = first;

        Assert.True(viewModel.DeleteRuleCommand.CanExecute(viewModel.SelectedRule));
        Assert.True(viewModel.DuplicateRuleCommand.CanExecute(viewModel.SelectedRule));
        Assert.False(viewModel.MoveUpCommand.CanExecute(viewModel.SelectedRule));
        Assert.True(viewModel.MoveDownCommand.CanExecute(viewModel.SelectedRule));

        viewModel.SelectedRule = second;

        Assert.True(viewModel.MoveUpCommand.CanExecute(viewModel.SelectedRule));
        Assert.False(viewModel.MoveDownCommand.CanExecute(viewModel.SelectedRule));

        viewModel.TestUrl = "https://example.com";
        Assert.True(viewModel.TestRuleCommand.CanExecute(null));

        viewModel.TestUrl = string.Empty;
        Assert.False(viewModel.TestRuleCommand.CanExecute(null));

        viewModel.SelectedRule = null;
        Assert.False(viewModel.TestRuleCommand.CanExecute(null));
    }
}
