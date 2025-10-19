using System.Collections.Generic;
using System.Threading.Tasks;
using LinkRouter.Settings.Services;
using LinkRouter.Settings.Services.Abstractions;
using LinkRouter.Settings.ViewModels;
using Xunit;

namespace LinkRouter.Settings.Avalonia.Tests.Rules;

public class RulesWorkspacePageTests
{
    [Fact]
    public async Task EditRuleCommand_WithSelection_InvokesDialogService()
    {
        var state = new ConfigurationState();
        var tester = new RuleTestService();
        var dialog = new RecordingDialogService();
        var viewModel = new RulesViewModel(state, tester, dialog);

        var rule = new RuleEditorViewModel
        {
            Match = "domain",
            Pattern = "example.com"
        };

        state.AddRule(rule);
        viewModel.SelectedRule = rule;

        await viewModel.EditRuleCommand.ExecuteAsync(null);

        Assert.Equal(1, dialog.InvocationCount);
        Assert.Same(rule, dialog.LastRule);
        Assert.Equal(viewModel.MatchTypes, dialog.LastMatchTypes);
        Assert.Equal(viewModel.ProfileOptions, dialog.LastProfileOptions);
    }

    [Fact]
    public async Task EditRuleCommand_WithoutSelection_SkipsDialog()
    {
        var state = new ConfigurationState();
        var tester = new RuleTestService();
        var dialog = new RecordingDialogService();
        var viewModel = new RulesViewModel(state, tester, dialog);

        await viewModel.EditRuleCommand.ExecuteAsync(null);

        Assert.Equal(0, dialog.InvocationCount);
    }

    private sealed class RecordingDialogService : IRuleEditorDialogService
    {
        public int InvocationCount { get; private set; }
        public RuleEditorViewModel? LastRule { get; private set; }
        public IReadOnlyList<string>? LastMatchTypes { get; private set; }
        public IReadOnlyList<string>? LastProfileOptions { get; private set; }

        public Task ShowEditorAsync(RuleEditorViewModel rule, IReadOnlyList<string> matchTypes, IReadOnlyList<string> profileOptions)
        {
            InvocationCount++;
            LastRule = rule;
            LastMatchTypes = matchTypes;
            LastProfileOptions = profileOptions;
            return Task.CompletedTask;
        }
    }
}
