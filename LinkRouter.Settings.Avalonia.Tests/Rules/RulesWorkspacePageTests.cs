using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LinkRouter.Settings.Services;
using LinkRouter.Settings.Services.Abstractions;
using LinkRouter.Settings.ViewModels;
using Xunit;

namespace LinkRouter.Settings.Avalonia.Tests.Rules;

public class RulesWorkspacePageTests
{
    [Fact]
    public async Task EditRuleCommand_WhenDialogAccepted_AppliesChanges()
    {
        var state = new ConfigurationState();
        var tester = new RuleTestService();
        var dialog = new StubDialogService { Result = true };
        var viewModel = new RulesViewModel(state, tester, dialog);

        var rule = new RuleEditorViewModel
        {
            Match = "domain",
            Pattern = "example.com",
            Browser = "old.exe"
        };

        state.AddRule(rule);
        viewModel.SelectedRule = rule;

        dialog.EditedRule = new RuleEditorViewModel
        {
            Match = "regex",
            Pattern = "new",
            Browser = "new.exe",
            ArgsTemplate = "{url}"
        };

        await viewModel.EditRuleCommand.ExecuteAsync(null);

        Assert.Equal("regex", rule.Match);
        Assert.Equal("new", rule.Pattern);
        Assert.Equal("new.exe", rule.Browser);
        Assert.Equal("{url}", rule.ArgsTemplate);
    }

    [Fact]
    public async Task EditRuleCommand_WhenDialogCancelled_DoesNotApplyChanges()
    {
        var state = new ConfigurationState();
        var tester = new RuleTestService();
        var dialog = new StubDialogService { Result = false };
        var viewModel = new RulesViewModel(state, tester, dialog);

        var rule = new RuleEditorViewModel
        {
            Match = "domain",
            Pattern = "example.com",
            Browser = "old.exe"
        };

        state.AddRule(rule);
        viewModel.SelectedRule = rule;

        dialog.EditedRule = new RuleEditorViewModel
        {
            Match = "regex",
            Pattern = "new",
            Browser = "new.exe",
            ArgsTemplate = "{url}"
        };

        await viewModel.EditRuleCommand.ExecuteAsync(null);

        Assert.Equal("domain", rule.Match);
        Assert.Equal("example.com", rule.Pattern);
        Assert.Equal("old.exe", rule.Browser);
    }

    [Fact]
    public async Task EditRuleCommand_WithoutSelection_DoesNotInvokeDialog()
    {
        var dialog = new StubDialogService { Result = true };
        var viewModel = new RulesViewModel(new ConfigurationState(), new RuleTestService(), dialog);

        await viewModel.EditRuleCommand.ExecuteAsync(null);

        Assert.False(dialog.Invoked);
    }

    private sealed class StubDialogService : IDialogService
    {
        public bool Result { get; set; }

        public bool Invoked { get; private set; }

        public RuleEditorViewModel? EditedRule { get; set; }

        public Task<bool> ShowRuleEditorAsync(RuleEditorViewModel rule, IReadOnlyList<string> matchTypes, IReadOnlyList<string> profileOptions, CancellationToken cancellationToken = default)
        {
            Invoked = true;

            if (EditedRule is not null)
            {
                rule.Enabled = EditedRule.Enabled;
                rule.Match = EditedRule.Match;
                rule.Pattern = EditedRule.Pattern;
                rule.Browser = EditedRule.Browser;
                rule.ArgsTemplate = EditedRule.ArgsTemplate;
                rule.Profile = EditedRule.Profile;
                rule.UserDataDir = EditedRule.UserDataDir;
                rule.WorkingDirectory = EditedRule.WorkingDirectory;
                rule.UseProfile = EditedRule.UseProfile;
            }

            return Task.FromResult(Result);
        }
    }
}
