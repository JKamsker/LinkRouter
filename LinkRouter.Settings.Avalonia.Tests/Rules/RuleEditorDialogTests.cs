using Avalonia.Headless.XUnit;
using FluentAvalonia.UI.Controls;
using LinkRouter.Settings.Avalonia.Views;
using Xunit;
using System.Threading.Tasks;

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
