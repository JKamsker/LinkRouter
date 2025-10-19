using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using LinkRouter.Settings.Avalonia.Views;
using LinkRouter.Settings.Services;
using LinkRouter.Settings.ViewModels;
using Xunit;

namespace LinkRouter.Settings.Avalonia.Tests.Rules;

public class RulesWorkspacePageTests
{
    [AvaloniaFact(Timeout = 30_000)]
    public async Task EditRuleButton_DoesNotCrash()
    {
        var dialogStub = new StubRuleEditorDialog();

        var viewModel = new RulesViewModel
        {
            SelectedRule = new RuleEditorViewModel
            {
                Match = "domain",
                Pattern = "example.com"
            }
        };

        var page = new RulesWorkspacePage
        {
            DataContext = viewModel,
            DialogFactory = () => dialogStub
        };

        await page.ShowRuleEditorAsync();

        Assert.True(dialogStub.ConfigureInvoked);
        Assert.True(dialogStub.ShowInvoked);
    }

    [AvaloniaFact(Timeout = 30_000)]
    public async Task ShowRuleEditorAsync_WithContentDialogHost_DoesNotCrash()
    {
        var viewModel = new RulesViewModel
        {
            SelectedRule = new RuleEditorViewModel
            {
                Match = "domain",
                Pattern = "example.com"
            }
        };

        var page = new RulesWorkspacePage
        {
            DataContext = viewModel
        };

        var window = new Window
        {
            Content = page
        };

        window.Show();

        var dialog = new AutoCloseRuleEditorDialog();
        page.DialogFactory = () => dialog;

        try
        {
            await page.ShowRuleEditorAsync();
        }
        finally
        {
            window.Close();
        }

        Assert.True(dialog.ShowInvoked);
        Assert.Same(window, dialog.CapturedOwner);
    }

    [AvaloniaFact(Timeout = 30_000)]
    public async Task EditRuleButtonClick_WithFaultyDialog_PropagatesKeyNotFound()
    {
        var window = new Window();

        var page = new RulesWorkspacePage();

        window.Content = page;
        window.Show();

        var viewModel = new RulesViewModel();
        var state = AppServices.ConfigurationState;
        state.Rules.Clear();

        var rule = new RuleEditorViewModel
        {
            Match = "domain",
            Pattern = "example.com"
        };

        state.AddRule(rule);
        viewModel.SelectedRule = rule;
        page.DataContext = viewModel;

        var throwingDialog = new ThrowingRuleEditorDialog();
        page.DialogFactory = () => throwingDialog;

        try
        {
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => page.ShowRuleEditorAsync());
            Assert.Contains("PrimaryButton", exception.Message);
            Assert.True(throwingDialog.ConfigureInvoked);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact(Timeout = 30_000)]
    public async Task ShowRuleEditorAsync_WithoutHost_InvokesDialogWithNullOwner()
    {
        var viewModel = new RulesViewModel
        {
            SelectedRule = new RuleEditorViewModel
            {
                Match = "domain",
                Pattern = "example.com"
            }
        };

        var capturingDialog = new CapturingRuleEditorDialog();

        var page = new RulesWorkspacePage
        {
            DataContext = viewModel,
            DialogFactory = () => capturingDialog
        };

        await page.ShowRuleEditorAsync();

        Assert.True(capturingDialog.ConfigureInvoked);
        Assert.True(capturingDialog.ShowInvoked);
        Assert.Null(capturingDialog.CapturedOwner);
    }

    private sealed class StubRuleEditorDialog : IRuleEditorDialog
    {
        public bool ConfigureInvoked { get; private set; }

        public bool ShowInvoked { get; private set; }

        public void Configure(RuleEditorViewModel rule, IEnumerable<string> matchTypes, IEnumerable<string> profileOptions)
        {
            ConfigureInvoked = true;
        }

        public Task ShowAsync(Window? owner)
        {
            ShowInvoked = true;
            return Task.CompletedTask;
        }
    }

    private sealed class AutoCloseRuleEditorDialog : IRuleEditorDialog
    {
        public bool ShowInvoked { get; private set; }

        public Window? CapturedOwner { get; private set; }

        public void Configure(RuleEditorViewModel rule, IEnumerable<string> matchTypes, IEnumerable<string> profileOptions)
        {
        }

        public Task ShowAsync(Window? owner)
        {
            ShowInvoked = true;
            CapturedOwner = owner;
            return Task.CompletedTask;
        }
    }

    private sealed class CapturingRuleEditorDialog : IRuleEditorDialog
    {
        public bool ConfigureInvoked { get; private set; }

        public bool ShowInvoked { get; private set; }

        public Window? CapturedOwner { get; private set; }

        public void Configure(RuleEditorViewModel rule, IEnumerable<string> matchTypes, IEnumerable<string> profileOptions)
        {
            ConfigureInvoked = true;
        }

        public Task ShowAsync(Window? owner)
        {
            ShowInvoked = true;
            CapturedOwner = owner;
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingRuleEditorDialog : IRuleEditorDialog
    {
        public bool ConfigureInvoked { get; private set; }

        public void Configure(RuleEditorViewModel rule, IEnumerable<string> matchTypes, IEnumerable<string> profileOptions)
        {
            ConfigureInvoked = true;
        }

        public Task ShowAsync(Window? owner)
        {
            return Task.FromException(new KeyNotFoundException("PrimaryButton"));
        }
    }
}
