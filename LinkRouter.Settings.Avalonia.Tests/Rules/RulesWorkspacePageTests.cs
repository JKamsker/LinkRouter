using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Themes.Simple;
using FluentAvalonia.UI.Controls;
using LinkRouter.Settings.Avalonia;
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
            DialogFactory = () => dialogStub
        };

        var window = new Window { Content = page };
        window.Show();
        try
        {
            await page.ShowRuleEditorAsync();
        }
        finally
        {
            window.Close();
        }

        Assert.True(dialogStub.ConfigureInvoked);
        Assert.True(dialogStub.ShowInvoked);
    }

    [AvaloniaFact(Timeout = 30_000)]
    public async Task ShowRuleEditorAsync_WithContentDialogHost_DoesNotCrash()
    {
        var window = new MainWindow();
        window.Show();

        try
        {
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

            var dialog = new AutoCloseRuleEditorDialog();
            page.DialogFactory = () => dialog;

            await page.ShowRuleEditorAsync();

            Assert.True(dialog.ShowInvoked);
            Assert.Same(window, dialog.CapturedOwner);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact(Timeout = 30_000)]
    public async Task EditRuleButtonClick_WithRealDialog_ThrowsKeyNotFound()
    {
        var window = new MainWindow();
        window.Show();

        var app = Application.Current;
        Assert.NotNull(app);
        var originalStyles = app!.Styles.ToList();

        try
        {
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

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(page.ShowRuleEditorAsync);
            Assert.Contains("PrimaryButton", exception.Message);
        }
        finally
        {
            window.Close();

            if (app is not null)
            {
                app.Styles.Clear();
                foreach (var style in originalStyles)
                {
                    app.Styles.Add(style);
                }
            }

            AppServices.ConfigurationState.Rules.Clear();
        }
    }

    [AvaloniaFact(Timeout = 30_000)]
    public async Task ShowRuleEditorAsync_WithoutHost_ThrowsKeyNotFound()
    {
        var viewModel = new RulesViewModel
        {
            SelectedRule = new RuleEditorViewModel
            {
                Match = "domain",
                Pattern = "example.com"
            }
        };

        var dialog = new ThrowingRuleEditorDialog();

        var page = new RulesWorkspacePage
        {
            DataContext = viewModel,
            DialogFactory = () => dialog
        };

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(page.ShowRuleEditorAsync);

        Assert.True(dialog.ConfigureInvoked);
        Assert.True(dialog.ShowInvoked);
        Assert.Null(dialog.CapturedOwner);
        Assert.Contains("PrimaryButton", exception.Message);
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

    private sealed class ThrowingRuleEditorDialog : IRuleEditorDialog
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
            return Task.FromException(new KeyNotFoundException("PrimaryButton"));
        }
    }
}
