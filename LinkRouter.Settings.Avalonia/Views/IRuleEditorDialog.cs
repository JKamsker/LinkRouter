using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using LinkRouter.Settings.ViewModels;

namespace LinkRouter.Settings.Avalonia.Views;

public interface IRuleEditorDialog
{
    void Configure(RuleEditorViewModel rule, IEnumerable<string> matchTypes, IEnumerable<string> profileOptions);
    Task ShowAsync(Window? owner);
}
