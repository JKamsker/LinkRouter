using LinkRouter.Settings.Core.Services;
using LinkRouter.Settings.Core.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LinkRouter.Settings.Views;

public sealed partial class ImportExportPage : Page
{
    public ImportExportViewModel ViewModel => (ImportExportViewModel)DataContext;

    public ImportExportPage()
    {
        InitializeComponent();
    }

    private void RestoreBackup_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: ConfigBackup backup })
        {
            var command = ViewModel.RestoreBackupCommand;
            if (command.CanExecute(backup))
            {
                command.Execute(backup);
            }
        }
    }
}
