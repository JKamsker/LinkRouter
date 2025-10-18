using System;
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

            var result = page.ShowRuleEditorAsync().GetAwaiter().GetResult();
            Assert.Equal(ContentDialogResult.None, result);
        });

        Assert.True(dialogStub.ConfigureInvoked);
        Assert.True(dialogStub.ShowInvoked);
    }

    [Fact]
    public void ShowRuleEditorAsync_WithRealDialog_DoesNotThrow()
    {
        TestAppHost.EnsureLifetime();

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

            var window = new MainWindow();
            var page = new RulesWorkspacePage
            {
                DataContext = viewModel,
                DialogFactory = static () => new CrashOnMissingHostDialog()
            };

            window.Show();

            var host = window.FindControl<ContentControl>("ContentHost");
            host!.Content = page;

            var task = page.ShowRuleEditorAsync();
            Assert.True(task.IsCompleted, "Rule editor dialog task should complete immediately in the test stub.");
            Assert.False(task.IsFaulted, task.Exception?.ToString());
            window.Close();
        });
    }

    private sealed class StubRuleEditorDialog : IRuleEditorDialog
    {
        public bool ConfigureInvoked { get; private set; }

        public bool ShowInvoked { get; private set; }

        public void Configure(RuleEditorViewModel rule, System.Collections.Generic.IEnumerable<string> matchTypes, System.Collections.Generic.IEnumerable<string> profileOptions)
        {
            ConfigureInvoked = true;
        }

        public Task<ContentDialogResult> ShowAsync(Window? owner)
        {
            ShowInvoked = true;
            return Task.FromResult(ContentDialogResult.None);
        }
    }

    private sealed class CrashOnMissingHostDialog : IRuleEditorDialog
    {
        public void Configure(RuleEditorViewModel rule, System.Collections.Generic.IEnumerable<string> matchTypes, System.Collections.Generic.IEnumerable<string> profileOptions)
        {
        }

        public Task<ContentDialogResult> ShowAsync(Window? owner)
        {
            return Task.FromResult(ContentDialogResult.None);
        }
    }
}
