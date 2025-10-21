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
        var viewModel = new RulesViewModel(state);

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
        Assert.Same(rule, viewModel.ActiveEditor!.TargetRule);
        Assert.NotSame(rule, viewModel.ActiveEditor.Rule);
    }

    [Fact]
    public async Task EditRuleCommand_WithoutSelection_DoesNotOpenEditor()
    {
        var state = new ConfigurationState();
        var viewModel = new RulesViewModel(state);

        await viewModel.EditRuleCommand.ExecuteAsync(null);

        Assert.False(viewModel.IsEditorOpen);
        Assert.Null(viewModel.ActiveEditor);
    }

    [Fact]
    public async Task CancelEditorCommand_ClosesEditorWithoutSaving()
    {
        var state = new ConfigurationState();
        var viewModel = new RulesViewModel(state);

        var rule = new RuleEditorViewModel
        {
            Match = "domain",
            Pattern = "example.com"
        };

        state.AddRule(rule);
        viewModel.SelectedRule = rule;

        await viewModel.EditRuleCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsEditorOpen);

        viewModel.ActiveEditor!.Rule.Pattern = "changed";

        viewModel.CancelEditorCommand.Execute(null);

        Assert.False(viewModel.IsEditorOpen);
        Assert.Null(viewModel.ActiveEditor);
        Assert.Equal("example.com", rule.Pattern);
    }

    [Fact]
    public async Task SaveEditorCommand_CommitsChanges()
    {
        var state = new ConfigurationState();
        var viewModel = new RulesViewModel(state);

        var rule = new RuleEditorViewModel
        {
            Match = "domain",
            Pattern = "example.com"
        };

        state.AddRule(rule);
        viewModel.SelectedRule = rule;

        await viewModel.EditRuleCommand.ExecuteAsync(null);

        viewModel.ActiveEditor!.Rule.Pattern = "changed";

        viewModel.SaveEditorCommand.Execute(null);

        Assert.Equal("changed", rule.Pattern);
    }

    [Fact]
    public void ActionCommands_DisableWithoutSelection()
    {
        var state = new ConfigurationState();
        var viewModel = new RulesViewModel(state);

        Assert.False(viewModel.DeleteRuleCommand.CanExecute(null));
        Assert.False(viewModel.DuplicateRuleCommand.CanExecute(null));
        Assert.False(viewModel.MoveUpCommand.CanExecute(null));
        Assert.False(viewModel.MoveDownCommand.CanExecute(null));

        var rule = new RuleEditorViewModel();
        state.AddRule(rule);
        viewModel.SelectedRule = rule;

        Assert.True(viewModel.DeleteRuleCommand.CanExecute(rule));
        Assert.True(viewModel.DuplicateRuleCommand.CanExecute(rule));
        Assert.False(viewModel.MoveUpCommand.CanExecute(rule));
        Assert.False(viewModel.MoveDownCommand.CanExecute(rule));
    }

    [Fact]
    public void MoveCommandsRespectBoundaries()
    {
        var state = new ConfigurationState();
        var viewModel = new RulesViewModel(state);

        var first = new RuleEditorViewModel { Pattern = "first" };
        var second = new RuleEditorViewModel { Pattern = "second" };
        state.AddRule(first);
        state.AddRule(second);

        viewModel.SelectedRule = first;
        Assert.False(viewModel.MoveUpCommand.CanExecute(first));
        Assert.True(viewModel.MoveDownCommand.CanExecute(first));

        viewModel.SelectedRule = second;
        Assert.True(viewModel.MoveUpCommand.CanExecute(second));
        Assert.False(viewModel.MoveDownCommand.CanExecute(second));
    }
}
