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
    public async Task EditRuleButton_DoesNotCrash()
    {
        TestAppHost.EnsureLifetime();

        var dialogStub = new StubRuleEditorDialog();

        await Dispatcher.UIThread.InvokeAsync(async () =>
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

            await page.ShowRuleEditorAsync();
        });

        Assert.True(dialogStub.ConfigureInvoked);
        Assert.True(dialogStub.ShowInvoked);
    }

    [Fact]
    public async Task ShowRuleEditorAsync_WithRealDialog_DoesNotThrow()
    {
        TestAppHost.EnsureLifetime();

        await Dispatcher.UIThread.InvokeAsync(async () =>
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
            try
            {
                var page = new RulesWorkspacePage
                {
                    DataContext = viewModel,
                    DialogFactory = static () => new CrashOnMissingHostDialog()
                };

                var contentReady = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
                window.Loaded += (_, _) =>
                {
                    var host = window.FindControl<ContentControl>("ContentHost");
                    host!.Content = page;
                    contentReady.TrySetResult(null);
                };

                window.Show();
                await contentReady.Task;

                var task = page.ShowRuleEditorAsync();
                Assert.True(task.IsCompleted, "Rule editor dialog task should complete immediately in the test stub.");
                Assert.False(task.IsFaulted, task.Exception?.ToString());
            }
            finally
            {
                window.Close();
            }
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
        private static readonly Type? ContentDialogHostType = Type.GetType("FluentAvalonia.UI.Controls.IContentDialogHost, FluentAvalonia");

        public void Configure(RuleEditorViewModel rule, System.Collections.Generic.IEnumerable<string> matchTypes, System.Collections.Generic.IEnumerable<string> profileOptions)
        {
        }

        public Task<ContentDialogResult> ShowAsync(Window? owner)
        {
            if (ContentDialogHostType is null || owner is null || !ContentDialogHostType.IsInstanceOfType(owner))
            {
                return Task.FromException<ContentDialogResult>(new InvalidOperationException("No content dialog host."));
            }

            return Task.FromResult(ContentDialogResult.None);
        }
    }
}
