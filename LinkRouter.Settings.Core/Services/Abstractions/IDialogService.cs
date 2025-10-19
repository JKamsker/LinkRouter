using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LinkRouter.Settings.ViewModels;

namespace LinkRouter.Settings.Services.Abstractions;

public interface IDialogService
{
    Task<bool> ShowRuleEditorAsync(
        RuleEditorViewModel rule,
        IReadOnlyList<string> matchTypes,
        IReadOnlyList<string> profileOptions,
        CancellationToken cancellationToken = default);
}
