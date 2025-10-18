using System;
using System.Linq;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using LinkRouter.Settings.Avalonia;
using LinkRouter.Settings.Avalonia.Views;
using Xunit;

namespace LinkRouter.Settings.Avalonia.Tests.Rules;

public class RuleEditorDialogTests
{
    [Fact]
    public void RuleEditorDialog_IsContentDialog()
    {
        TestAppHost.EnsureLifetime();

        ContentDialog? contentDialog = null;
        Dispatcher.UIThread.Invoke(() =>
        {
            contentDialog = new RuleEditorDialog();
        });

        Assert.NotNull(contentDialog);
        Assert.IsAssignableFrom<ContentDialog>(contentDialog);
    }

    [Fact]
    public void MainWindow_MissingContentDialogHost()
    {
        TestAppHost.EnsureLifetime();

        var hostType = Type.GetType("FluentAvalonia.UI.Controls.DialogHost, FluentAvalonia");
        object? host = null;

        Dispatcher.UIThread.Invoke(() =>
        {
            var window = new MainWindow();
            if (hostType is null)
            {
                return;
            }

            window.Show();

            host = window.GetVisualDescendants().FirstOrDefault(hostType.IsInstanceOfType);

            window.Close();
        });

        Assert.NotNull(host);
    }
}
