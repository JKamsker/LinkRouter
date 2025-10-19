using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.Styling;
using Avalonia.Themes.Simple;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.Styling;
using LinkRouter.Settings.Avalonia;
using LinkRouter.Settings.Avalonia.Views;
using LinkRouter.Settings.Services;
using LinkRouter.Settings.ViewModels;
using Xunit;

namespace LinkRouter.Settings.Avalonia.Tests.Rules;

public class RulesWorkspacePageTests
{
    [AvaloniaFact]
    public Task EditRuleButton_DoesNotCrash()
    {
        TestAppHost.EnsureLifetime();

        var dialogStub = new StubRuleEditorDialog();

        Dispatcher.UIThread.Invoke(() =>
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
                DataContext = viewModel,
                DialogFactory = () => dialogStub
            };

            page.ShowRuleEditorAsync().GetAwaiter().GetResult();
        });

        Assert.True(dialogStub.ConfigureInvoked);
        Assert.True(dialogStub.ShowInvoked);
        return Task.CompletedTask;
    }

    [AvaloniaFact]
    public async Task ShowRuleEditorAsync_WithContentDialogHost_DoesNotCrash()
    {
        TestAppHost.EnsureLifetime();

        AutoCloseRuleEditorDialog? dialog = null;
        Task? dialogTask = null;
        MainWindow? window = null;

        Dispatcher.UIThread.Invoke(() =>
        {
            window = new MainWindow();
            window.Show();

            var rulesItem = window.NavView.MenuItems
                .OfType<NavigationViewItem>()
                .First(item => string.Equals(item.Tag as string, "rules", StringComparison.Ordinal));

            window.NavView.SelectedItem = rulesItem;

            var page = Assert.IsType<RulesWorkspacePage>(window.ContentHost.Content);
            var viewModel = Assert.IsType<RulesViewModel>(page.DataContext);
            viewModel.SelectedRule = new RuleEditorViewModel
            {
                Match = "domain",
                Pattern = "example.com"
            };

            dialog = new AutoCloseRuleEditorDialog();
            page.DialogFactory = () => dialog;

            dialogTask = page.ShowRuleEditorAsync();
        });

        await dialogTask!;

        Assert.NotNull(dialog);
        Assert.True(dialog!.ShowInvoked);
        Assert.NotNull(dialog.CapturedOwner);
        Dispatcher.UIThread.Invoke(() => window!.Close());
    }

    [AvaloniaFact]
    public async Task EditRuleButtonClick_WithRealDialog_TimesOutWithoutHost()
    {
        TestAppHost.EnsureLifetime();

        Task? dialogTask = null;
        Dispatcher.UIThread.Invoke(() =>
        {
            var page = new RulesWorkspacePage
            {
                DataContext = new RulesViewModel
                {
                    SelectedRule = new RuleEditorViewModel
                    {
                        Match = "domain",
                        Pattern = "example.com"
                    }
                },
                DialogFactory = static () => new NeverCompletingRuleEditorDialog()
            };

            dialogTask = page.ShowRuleEditorAsync();
        });

        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(1));
        var completed = await Task.WhenAny(dialogTask!, timeoutTask);
        Assert.Same(timeoutTask, completed);
    }

    [AvaloniaFact]
    public async Task ShowRuleEditorAsync_WithoutHost_ThrowsKeyNotFound()
    {
        TestAppHost.EnsureLifetime();

        var viewModel = new RulesViewModel
        {
            SelectedRule = new RuleEditorViewModel
            {
                Match = "domain",
                Pattern = "example.com"
            }
        };

        RulesWorkspacePage? page = null;
        Dispatcher.UIThread.Invoke(() =>
        {
            page = new RulesWorkspacePage
            {
                DataContext = viewModel
            };
        });

        var operation = Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await page!.ShowRuleEditorAsync();
        });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await operation);
        Assert.Contains("OverlayLayer", exception.Message);
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

    private sealed class NeverCompletingRuleEditorDialog : IRuleEditorDialog
    {
        public void Configure(RuleEditorViewModel rule, IEnumerable<string> matchTypes, IEnumerable<string> profileOptions)
        {
        }

        public Task ShowAsync(Window? owner)
        {
            return new TaskCompletionSource().Task;
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
}
