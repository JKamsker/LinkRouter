using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LinkRouter.Settings.Avalonia.Views;

public partial class ImportExportPage : UserControl
{
    public ImportExportPage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

