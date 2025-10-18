using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using LinkRouter.Settings.Avalonia;
using LinkRouter.Settings.Avalonia.Views;
using LinkRouter.Settings.ViewModels;
using Xunit;

namespace LinkRouter.Settings.Avalonia.Tests.Rules;

public class RulesWorkspacePageTests
{
    [Fact]
    public void EditRuleButton_DoesNotCrash()
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
    }

    [Fact]
    public async Task ShowRuleEditorAsync_WithContentDialogHost_DoesNotCrash()
    {
        var lifetime = TestAppHost.EnsureLifetime();

        AutoCloseRuleEditorDialog? dialog = null;

        var dialogTask = await Dispatcher.UIThread.InvokeAsync<Task>(() =>
        {
            var window = Assert.IsType<MainWindow>(lifetime.MainWindow);
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

            return page.ShowRuleEditorAsync();
        });

        await dialogTask;

        Assert.NotNull(dialog);
        Assert.True(dialog!.ShowInvoked);
        Assert.NotNull(dialog.CapturedOwner);
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

    private sealed class AutoCloseRuleEditorDialog : RuleEditorDialog
    {
        public bool ShowInvoked { get; private set; }

        public Window? CapturedOwner { get; private set; }

        public new Task ShowAsync(Window? owner)
        {
            ShowInvoked = true;
            CapturedOwner = owner;
            return base.ShowAsync(owner);
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            Dispatcher.UIThread.Post(() => Close());
        }
    }
}
