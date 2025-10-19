using System.Threading.Tasks;
using LinkRouter.Settings.Services.Abstractions;
using LinkRouter.Settings.ViewModels;
using Xunit;

namespace LinkRouter.Settings.Avalonia.Tests.Rules;

public class RulesWorkspacePageTests
{
    [Fact]
    public void EditSelectedRuleCommand_CannotExecuteWithoutSelection()
    {
        var (viewModel, _) = CreateViewModel();

        Assert.False(viewModel.EditSelectedRuleCommand.CanExecute(null));
    }

    [Fact]
    public async Task EditSelectedRuleCommand_ShowsDialogForSelection()
    {
        var (viewModel, dialogService) = CreateViewModel();
        viewModel.AddRuleCommand.Execute(null);
        viewModel.SelectedRule = viewModel.Rules[^1];

        await viewModel.EditSelectedRuleCommand.ExecuteAsync(null);

        Assert.True(dialogService.WasInvoked);
        Assert.Same(viewModel.SelectedRule, dialogService.CapturedDialog?.Rule);
    }

    private static (RulesViewModel ViewModel, TestDialogService DialogService) CreateViewModel()
    {
        var state = new ConfigurationState();
        var tester = new LinkRouter.Settings.Services.RuleTestService();
        var dialogService = new TestDialogService();
        var viewModel = new RulesViewModel(state, tester, dialogService);
        return (viewModel, dialogService);
    }

    private sealed class TestDialogService : IDialogService
    {
        public bool WasInvoked { get; private set; }

        public RuleEditorDialogViewModel? CapturedDialog { get; private set; }

        public Task ShowRuleEditorAsync(RuleEditorDialogViewModel dialogViewModel)
        {
            WasInvoked = true;
            CapturedDialog = dialogViewModel;
            return Task.CompletedTask;
        }
    }
}
