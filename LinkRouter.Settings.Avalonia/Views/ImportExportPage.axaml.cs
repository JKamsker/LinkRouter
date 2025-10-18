using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LinkRouter.Settings.Core.ViewModels;

namespace LinkRouter.Settings.Avalonia.Views;

public partial class ImportExportPage : UserControl
{
    public ImportExportPage()
    {
        InitializeComponent();
        DataContext = new ImportExportViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
