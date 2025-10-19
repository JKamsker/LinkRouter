using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;

namespace LinkRouter.Settings.Avalonia.Views;

public partial class RuleEditorDialog : ContentDialog
{
    public RuleEditorDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
