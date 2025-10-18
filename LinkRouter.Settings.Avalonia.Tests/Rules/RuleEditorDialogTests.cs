using Avalonia.Controls;
using Avalonia.Threading;
using LinkRouter.Settings.Avalonia.Views;
using Xunit;

namespace LinkRouter.Settings.Avalonia.Tests.Rules;

public class RuleEditorDialogTests
{
    [Fact]
    public void RuleEditorDialog_UsesWindowHost()
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
}
