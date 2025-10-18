using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
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
}
