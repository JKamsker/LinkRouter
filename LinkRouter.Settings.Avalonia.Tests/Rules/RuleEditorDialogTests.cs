using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
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
}
