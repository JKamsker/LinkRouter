using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Themes.Simple;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using FluentAvalonia.UI.Controls;
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

        var dialog = new AutoCloseRuleEditorDialog();
        var (window, page) = await ShowRulesWindowAsync(viewModel, () => dialog);

        try
        {
            await page.ShowRuleEditorAsync();
        }
        finally
        {
            window.Close();
        }

        Assert.True(dialog.ShowInvoked);
        Assert.NotNull(dialog.CapturedOwner);
    }

    [AvaloniaFact(Timeout = 30_000)]
    public async Task EditRuleButtonClick_WithThrowingDialog_PropagatesException()
    {
        var state = AppServices.ConfigurationState;
        state.Rules.Clear();

        var rule = new RuleEditorViewModel
        {
            Match = "domain",
            Pattern = "example.com"
        };

        state.AddRule(rule);

        var viewModel = new RulesViewModel
        {
            SelectedRule = rule
        };

        var dialog = new ThrowingRuleEditorDialog();
        var (window, page) = await ShowRulesWindowAsync(viewModel, () => dialog);

        InvalidOperationException? exception = null;

        try
        {
            await page.ShowRuleEditorAsync();
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }
        finally
        {
            window.Close();
        }

        Assert.NotNull(exception);
        Assert.Contains("template has not been applied", exception!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [AvaloniaFact(Timeout = 30_000)]
    public async Task ShowRuleEditorAsync_WithoutHost_ThrowsInvalidOperation()
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

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(page.ShowRuleEditorAsync);
        Assert.Contains("No TopLevel", exception.Message);
    }

    private sealed class StubRuleEditorDialog : IRuleEditorDialog
    {
        public bool ConfigureInvoked { get; private set; }

        public bool ShowInvoked { get; private set; }

        public void Configure(RuleEditorViewModel rule, System.Collections.Generic.IEnumerable<string> matchTypes, System.Collections.Generic.IEnumerable<string> profileOptions)
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

        public void Configure(RuleEditorViewModel rule, System.Collections.Generic.IEnumerable<string> matchTypes, System.Collections.Generic.IEnumerable<string> profileOptions)
        {
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
        public void Configure(RuleEditorViewModel rule, IEnumerable<string> matchTypes, IEnumerable<string> profileOptions)
        {
        }

        public Task ShowAsync(Window? owner)
        {
            throw new InvalidOperationException("Attempted to setup ContentDialog but the template has not been applied yet.");
        }
    }

    private static async Task<(Window Window, RulesWorkspacePage Page)> ShowRulesWindowAsync(
        RulesViewModel viewModel,
        Func<IRuleEditorDialog>? dialogFactory = null)
    {
        var page = new RulesWorkspacePage
        {
            DataContext = viewModel
        };

        if (dialogFactory is not null)
        {
            page.DialogFactory = dialogFactory;
        }

        var window = new Window
        {
            Content = page
        };

        window.Show();
        await Task.Yield();

        return (window, page);
    }
}
