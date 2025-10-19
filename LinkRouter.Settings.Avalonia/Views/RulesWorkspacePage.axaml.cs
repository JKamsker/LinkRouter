using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
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
        var dialogHost = this.FindAncestorOfType<DialogHost>();
        var originalContent = dialogHost?.Content;
        Control? placeholder = null;

        if (dialogHost is not null)
        {
            var brush = this.TryFindResource("SystemControlBackgroundAltHighBrush", out var resource)
                ? resource as IBrush
                : null;

            brush ??= this.TryFindResource("SystemControlBackgroundMediumBrush", out resource)
                ? resource as IBrush
                : null;

            brush ??= new SolidColorBrush(Color.FromArgb(0xFF, 0x21, 0x21, 0x21));

            placeholder = new Border
            {
                Background = brush,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            dialogHost.Content = placeholder;
        }

        async Task ShowAsync()
        {
            try
            {
                await dialog.ShowAsync(owner);
            }
            finally
            {
                if (dialogHost is not null)
                {
                    dialogHost.Content = originalContent;
                }
            }
        }

        return ShowAsync();
    }
}
