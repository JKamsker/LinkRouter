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
    [AvaloniaFact(Timeout = AvaloniaTestTimeouts.Default)]
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

    [AvaloniaFact(Timeout = AvaloniaTestTimeouts.Default)]
    public async Task ShowRuleEditorAsync_WithContentDialogHost_DoesNotCrash()
    {
        var lifetime = TestAppHost.EnsureLifetime();

        AutoCloseRuleEditorDialog? dialog = null;
        Task? dialogTask = null;

        Dispatcher.UIThread.Invoke(() =>
        {
            var window = Assert.IsType<MainWindow>(lifetime.MainWindow);

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
            window.Close();
        });

        await dialogTask!;

        Assert.NotNull(dialog);
        Assert.True(dialog!.ShowInvoked);
        Assert.NotNull(dialog.CapturedOwner);
    }

    [AvaloniaFact(Timeout = AvaloniaTestTimeouts.Default)]
    public async Task EditRuleButtonClick_WithRealDialog_ThrowsKeyNotFound()
    {
        var lifetime = TestAppHost.EnsureLifetime();

        var operation = Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var window = Assert.IsType<MainWindow>(lifetime.MainWindow);
            window.Show();

            var rulesItem = window.NavView.MenuItems
                .OfType<NavigationViewItem>()
                .First(item => string.Equals(item.Tag as string, "rules", StringComparison.Ordinal));

            window.NavView.SelectedItem = rulesItem;

            var page = Assert.IsType<RulesWorkspacePage>(window.ContentHost.Content);
            var viewModel = Assert.IsType<RulesViewModel>(page.DataContext);

            var state = AppServices.ConfigurationState;
            state.Rules.Clear();

            var rule = new RuleEditorViewModel
            {
                Match = "domain",
                Pattern = "example.com"
            };

            state.AddRule(rule);
            viewModel.SelectedRule = rule;

            if (Application.Current is { } app)
            {
                app.Styles.Clear();
                app.Styles.Add(new SimpleTheme());

                var brokenTemplate = new FuncControlTemplate<ContentDialog>((_, _) => new Border());
                app.Styles.Add(new Style(x => x.OfType<ContentDialog>())
                {
                    Setters =
                    {
                        new Setter(TemplatedControl.TemplateProperty, brokenTemplate)
                    }
                });
            }

            try
            {
                await page.ShowRuleEditorAsync();
            }
            finally
            {
                window.Close();
            }
        });

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () => await operation);
        Assert.Contains("PrimaryButton", exception.Message);
    }

    [AvaloniaFact(Timeout = AvaloniaTestTimeouts.Default)]
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

        var page = new RulesWorkspacePage
        {
            DataContext = viewModel
        };

        var operation = Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await page.ShowRuleEditorAsync();
        });

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () => await operation);
        Assert.Contains("PrimaryButton", exception.Message);
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
}
