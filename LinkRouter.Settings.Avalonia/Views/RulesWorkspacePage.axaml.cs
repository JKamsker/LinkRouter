using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using LinkRouter.Settings.ViewModels;

namespace LinkRouter.Settings.Avalonia.Views;

public partial class RulesWorkspacePage : UserControl
{
    public Func<IRuleEditorDialog> DialogFactory { get; set; } = static () => new RuleEditorDialog();

    public RulesWorkspacePage()
    {
        InitializeComponent();
    }

    private async void OnEditRuleClick(object? sender, RoutedEventArgs e)
    {
        await ShowRuleEditorAsync();
    }

    internal Task ShowRuleEditorAsync()
    {
        if (DataContext is not RulesViewModel viewModel)
        {
            return Task.CompletedTask;
        }

        var rule = viewModel.SelectedRule;
        if (rule is null)
        {
            return Task.CompletedTask;
        }

        var dialog = DialogFactory();
        dialog.Configure(rule, viewModel.MatchTypes, viewModel.ProfileOptions);

        var owner = TopLevel.GetTopLevel(this) as Window;
        return dialog.ShowAsync(owner);
    }
}
