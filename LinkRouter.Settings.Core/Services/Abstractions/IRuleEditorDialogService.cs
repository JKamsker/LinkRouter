using System.Threading.Tasks;
using LinkRouter.Settings.ViewModels;

namespace LinkRouter.Settings.Services.Abstractions;

public interface IRuleEditorDialogService
{
    Task<bool> EditRuleAsync(RuleEditorDialogViewModel viewModel);
}
