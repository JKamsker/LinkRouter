using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using LinkRouter.Settings.ViewModels;

namespace LinkRouter.Settings.Avalonia.Views;

public partial class RulesWorkspacePage : UserControl
{
    public RulesWorkspacePage()
    {
        InitializeComponent();
    }

    private async void OnEditRuleClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not RulesViewModel vm)
        {
            return;
        }

        if (vm.SelectedRule is null)
        {
            return;
        }

        if (TopLevel.GetTopLevel(this) is not Window window)
        {
            return;
        }

        var dialog = new RuleEditorDialog
        {
            DataContext = vm.SelectedRule,
            MatchTypes = vm.MatchTypes.ToList(),
            ProfileOptions = vm.ProfileOptions.ToList()
        };

        await dialog.ShowAsync(window);
    }
}
