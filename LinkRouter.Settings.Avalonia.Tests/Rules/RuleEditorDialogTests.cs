using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using LinkRouter.Settings.Avalonia;
using LinkRouter.Settings.Avalonia.Views;
using Xunit;

namespace LinkRouter.Settings.Avalonia.Tests.Rules;

public class RuleEditorDialogTests
{
    [Fact]
    public void RuleEditorDialog_IsWindow()
    {
        TestAppHost.EnsureLifetime();

        Window? dialogWindow = null;
        Dispatcher.UIThread.Invoke(() =>
        {
            dialogWindow = new RuleEditorDialog();
        });

        Assert.NotNull(dialogWindow);
        Assert.IsAssignableFrom<Window>(dialogWindow);
    }

    [Fact]
    public async Task RuleEditorDialog_ShowAsync_CompletesWhenClosed()
    {
        TestAppHost.EnsureLifetime();

        var dialogTask = await Dispatcher.UIThread.InvokeAsync<Task>(() =>
        {
            var dialog = new RuleEditorDialog();
            var showTask = dialog.ShowAsync(null);
            Dispatcher.UIThread.Post(dialog.Close);
            return showTask;
        });

        await dialogTask;
    }
}
