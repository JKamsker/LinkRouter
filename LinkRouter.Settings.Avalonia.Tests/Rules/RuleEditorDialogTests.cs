using Avalonia.Headless.XUnit;
using FluentAvalonia.UI.Controls;
using LinkRouter.Settings;
using LinkRouter.Settings.Views;
using System.Threading.Tasks;
using Xunit;

namespace LinkRouter.Settings.Avalonia.Tests.Rules;

public class RuleEditorDialogTests
{
    [AvaloniaFact(Timeout = 30_000)]
    public Task RuleEditorDialog_IsContentDialog()
    {
        var dialog = new RuleEditorDialog();

        Assert.NotNull(dialog);
        Assert.IsAssignableFrom<ContentDialog>(dialog);
        return Task.CompletedTask;
    }
}
