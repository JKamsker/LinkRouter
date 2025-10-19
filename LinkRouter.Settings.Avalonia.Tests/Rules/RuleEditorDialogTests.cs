using FluentAvalonia.UI.Controls;
using LinkRouter.Settings.Avalonia.Views;
using Xunit;

namespace LinkRouter.Settings.Avalonia.Tests.Rules;

public class RuleEditorDialogTests
{
    [Fact]
    public void RuleEditorDialog_InheritsContentDialog()
    {
        Assert.True(typeof(ContentDialog).IsAssignableFrom(typeof(RuleEditorDialog)));
    }
}
