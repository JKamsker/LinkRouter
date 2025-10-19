using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using LinkRouter.Settings.Services;
using LinkRouter.Settings.Services.Abstractions;
using LinkRouter.Settings.ViewModels;
using Xunit;

namespace LinkRouter.Settings.Avalonia.Tests.Rules;

public class RulesViewModelTests
{
    [Fact]
    public async Task EditRuleCommand_WhenDialogAccepts_UpdatesRule()
    {
        var state = new ConfigurationState();
        var rule = new RuleEditorViewModel
        {
            Match = "domain",
            Pattern = "example.com",
            Browser = "chrome"
        };
        state.AddRule(rule);

        var dialog = new StubDialogService(result: true)
        {
            OnShow = editor => editor.Browser = "edge"
        };

        var viewModel = CreateViewModel(state, dialog);
        viewModel.SelectedRule = rule;

        await ((IAsyncRelayCommand)viewModel.EditRuleCommand).ExecuteAsync(null);

        Assert.Equal("edge", rule.Browser);
        Assert.True(dialog.Invoked);
    }

    [Fact]
    public async Task EditRuleCommand_WhenDialogCancels_DoesNotChangeRule()
    {
        var state = new ConfigurationState();
        var rule = new RuleEditorViewModel
        {
            Match = "domain",
            Pattern = "example.com",
            Browser = "chrome"
        };
        state.AddRule(rule);

        var dialog = new StubDialogService(result: false)
        {
            OnShow = editor => editor.Browser = "edge"
        };

        var viewModel = CreateViewModel(state, dialog);
        viewModel.SelectedRule = rule;

        await ((IAsyncRelayCommand)viewModel.EditRuleCommand).ExecuteAsync(null);

        Assert.Equal("chrome", rule.Browser);
        Assert.True(dialog.Invoked);
    }

    [Fact]
    public async Task EditRuleCommand_WithoutSelection_DoesNotInvokeDialog()
    {
        var state = new ConfigurationState();
        var dialog = new StubDialogService(result: true);
        var viewModel = CreateViewModel(state, dialog);

        await ((IAsyncRelayCommand)viewModel.EditRuleCommand).ExecuteAsync(null);

        Assert.False(dialog.Invoked);
    }

    private static RulesViewModel CreateViewModel(ConfigurationState state, IDialogService dialogService)
    {
        var tester = new RuleTestService();
        return new RulesViewModel(state, tester, dialogService);
    }

    private sealed class StubDialogService : IDialogService
    {
        private readonly bool _result;

        public StubDialogService(bool result)
        {
            _result = result;
        }

        public bool Invoked { get; private set; }

        public Action<RuleEditorViewModel>? OnShow { get; set; }

        public Task<bool> ShowRuleEditorAsync(
            RuleEditorViewModel editor,
            IReadOnlyList<string> matchTypes,
            IReadOnlyList<string> profileOptions)
        {
            Invoked = true;
            OnShow?.Invoke(editor);
            return Task.FromResult(_result);
        }
    }
}
