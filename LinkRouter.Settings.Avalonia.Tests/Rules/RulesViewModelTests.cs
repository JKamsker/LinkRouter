using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LinkRouter.Settings.Services;
using LinkRouter.Settings.Services.Abstractions;
using LinkRouter.Settings.ViewModels;
using Xunit;

namespace LinkRouter.Settings.Avalonia.Tests.Rules;

public class RulesViewModelTests
{
    [Fact]
    public async Task EditRuleCommand_NoSelection_DoesNotInvokeDialog()
    {
        var dialog = new StubDialogService();
        var viewModel = CreateViewModel(dialog);

        await viewModel.EditRuleCommand.ExecuteAsync(null);

        Assert.False(dialog.Invoked);
    }

    [Fact]
    public async Task EditRuleCommand_WithSelection_InvokesDialog()
    {
        var dialog = new StubDialogService();
        var viewModel = CreateViewModel(dialog);
        var rule = new RuleEditorViewModel
        {
            Match = "domain",
            Pattern = "example.com"
        };

        viewModel.SelectedRule = rule;
        await viewModel.EditRuleCommand.ExecuteAsync(null);

        Assert.True(dialog.Invoked);
        Assert.Same(rule, dialog.LastRule);
    }

    private static RulesViewModel CreateViewModel(IDialogService dialog)
    {
        var state = new ConfigurationState();
        var tester = new RuleTestService();
        return new RulesViewModel(state, tester, dialog);
    }

    private sealed class StubDialogService : IDialogService
    {
        public bool Invoked { get; private set; }

        public RuleEditorViewModel? LastRule { get; private set; }

        public Task<DialogResult> ShowRuleEditorAsync(
            RuleEditorViewModel rule,
            IReadOnlyList<string> matchTypes,
            IReadOnlyList<string> profileOptions,
            CancellationToken cancellationToken = default)
        {
            Invoked = true;
            LastRule = rule;
            return Task.FromResult(DialogResult.Primary);
        }
    }
}
