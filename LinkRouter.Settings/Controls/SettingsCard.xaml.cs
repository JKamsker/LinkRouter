using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LinkRouter.Settings.Controls;

public sealed partial class SettingsCard : UserControl
{
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(string), typeof(SettingsCard), new PropertyMetadata(null));

    public SettingsCard()
    {
        InitializeComponent();
    }

    public string? Header
    {
        get => (string?)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
}
