using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.Themes.Simple;
using Avalonia.Headless.XUnit;
using FluentAvalonia.UI.Controls;
using LinkRouter.Settings.Avalonia.Views;
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
        var dialog = new AutoCloseRuleEditorDialog();

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
            DialogFactory = () => dialog
        };

        var app = Application.Current ?? throw new InvalidOperationException("Avalonia application not initialized.");
        var originalStyles = app.Styles.ToArray();
        app.Styles.Clear();
        app.Styles.Add(new SimpleTheme());

        var window = new Window
        {
            Template = SimpleWindowTemplate,
            Content = page
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();

        try
        {
            await page.ShowRuleEditorAsync();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();

            app.Styles.Clear();
            foreach (var style in originalStyles)
            {
                app.Styles.Add(style);
            }
        }

        Assert.True(dialog.ShowInvoked);
        Assert.NotNull(dialog.CapturedOwner);
    }

    [AvaloniaFact(Timeout = 30_000)]
    public async Task ShowRuleEditorAsync_WhenDialogThrows_PropagatesException()
    {
        var dialog = new ThrowingRuleEditorDialog();

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
            DialogFactory = () => dialog
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => page.ShowRuleEditorAsync());
        Assert.Contains("template", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [AvaloniaFact(Timeout = 30_000)]
    public async Task ShowRuleEditorAsync_WithoutHost_UsesNullOwner()
    {
        var dialog = new AutoCloseRuleEditorDialog();

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
            DialogFactory = () => dialog
        };

        await page.ShowRuleEditorAsync();

        Assert.True(dialog.ShowInvoked);
        Assert.Null(dialog.CapturedOwner);
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
        public void Configure(RuleEditorViewModel rule, IEnumerable<string> matchTypes, IEnumerable<string> profileOptions)
        {
        }

        public Task ShowAsync(Window? owner)
        {
            throw new InvalidOperationException("ContentDialog template has not been applied yet.");
        }
    }

    private static FuncControlTemplate<Window> SimpleWindowTemplate { get; } = new((owner, _) =>
    {
        var presenter = new ContentPresenter();
        presenter.Bind(ContentPresenter.ContentProperty, owner.GetObservable(ContentControl.ContentProperty));
        return presenter;
    });
}
