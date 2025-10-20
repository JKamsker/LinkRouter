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
        Assert.Equal(rule.Pattern, viewModel.ActiveEditor!.Rule.Pattern);
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
    public async Task CloseEditorCommand_CancelsEdits()
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

        viewModel.ActiveEditor!.Rule.Pattern = "changed.example";
        viewModel.CloseEditorCommand.Execute(null);

        Assert.False(viewModel.IsEditorOpen);
        Assert.Null(viewModel.ActiveEditor);
        Assert.Equal("example.com", rule.Pattern);
    }

    [Fact]
    public async Task CommitEditorCommand_PersistsEdits()
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
        state.MarkSaved();
        viewModel.SelectedRule = rule;

        await viewModel.EditRuleCommand.ExecuteAsync(null);

        viewModel.ActiveEditor!.Rule.Pattern = "changed.example";
        viewModel.CommitEditorCommand.Execute(viewModel.ActiveEditor);

        Assert.False(viewModel.IsEditorOpen);
        Assert.Equal("changed.example", rule.Pattern);
        Assert.True(state.HasUnsavedChanges);
    }

    [Fact]
    public void RuleActionCommands_ReflectSelectionAndPosition()
    {
        var state = new ConfigurationState();
        var tester = new RuleTestService();
        var viewModel = new RulesViewModel(state, tester);

        var first = new RuleEditorViewModel { Match = "domain", Pattern = "first.example" };
        var second = new RuleEditorViewModel { Match = "domain", Pattern = "second.example" };

        state.AddRule(first);
        state.AddRule(second);
        state.MarkSaved();

        Assert.False(viewModel.DeleteRuleCommand.CanExecute(null));
        Assert.False(viewModel.DuplicateRuleCommand.CanExecute(null));
        Assert.False(viewModel.MoveUpCommand.CanExecute(first));
        Assert.False(viewModel.MoveDownCommand.CanExecute(second));
        Assert.False(viewModel.TestRuleCommand.CanExecute(null));

        viewModel.SelectedRule = first;

        Assert.True(viewModel.DeleteRuleCommand.CanExecute(first));
        Assert.True(viewModel.DuplicateRuleCommand.CanExecute(first));
        Assert.False(viewModel.MoveUpCommand.CanExecute(first));
        Assert.True(viewModel.MoveDownCommand.CanExecute(first));
        Assert.True(viewModel.TestRuleCommand.CanExecute(null));

        viewModel.SelectedRule = second;

        Assert.True(viewModel.MoveUpCommand.CanExecute(second));
        Assert.False(viewModel.MoveDownCommand.CanExecute(second));
    }
}
