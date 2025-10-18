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
        if (DataContext is not RulesViewModel viewModel)
        {
            return;
        }

        var rule = viewModel.SelectedRule;
        if (rule is null)
        {
            return;
        }

        var dialog = new RuleEditorDialog();
        dialog.Configure(rule, viewModel.MatchTypes, viewModel.ProfileOptions);

        if (TopLevel.GetTopLevel(this) is Window window)
        {
            await dialog.ShowAsync(window);
        }
    }
}
