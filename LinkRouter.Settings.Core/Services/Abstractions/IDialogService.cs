using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LinkRouter.Settings.ViewModels;

namespace LinkRouter.Settings.Services.Abstractions;

public interface IDialogService
{
    Task<DialogResult> ShowRuleEditorAsync(
        RuleEditorViewModel rule,
        IReadOnlyList<string> matchTypes,
        IReadOnlyList<string> profileOptions,
        CancellationToken cancellationToken = default);
}

public enum DialogResult
{
    None,
    Primary,
    Secondary,
    Close
}
