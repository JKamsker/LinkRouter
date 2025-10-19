using System.Threading.Tasks;
using LinkRouter.Settings.ViewModels;

namespace LinkRouter.Settings.Services.Abstractions;

public interface IDialogService
{
    Task ShowRuleEditorAsync(RuleEditorDialogViewModel dialogViewModel);
}
