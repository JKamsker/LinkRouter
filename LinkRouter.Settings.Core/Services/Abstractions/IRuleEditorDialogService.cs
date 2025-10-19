using System.Collections.Generic;
using System.Threading.Tasks;
using LinkRouter.Settings.ViewModels;

namespace LinkRouter.Settings.Services.Abstractions;

public interface IRuleEditorDialogService
{
    Task ShowEditorAsync(
        RuleEditorViewModel rule,
        IReadOnlyList<string> matchTypes,
        IReadOnlyList<string> profileOptions);
}
