using System.Threading.Tasks;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using LinkRouter.Settings.Avalonia;
using LinkRouter.Settings.Avalonia.Views;
using Xunit;

namespace LinkRouter.Settings.Avalonia.Tests.Rules;

public class RuleEditorDialogTests
{
    [AvaloniaFact]
    public Task RuleEditorDialog_IsContentDialog()
    {
        TestAppHost.EnsureLifetime();

        ContentDialog? dialog = null;
        Dispatcher.UIThread.Invoke(() =>
        {
            dialog = new RuleEditorDialog();
        });

        Assert.NotNull(dialog);
        Assert.IsAssignableFrom<ContentDialog>(dialog);
        return Task.CompletedTask;
    }
}
