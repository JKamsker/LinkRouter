using System.Threading.Tasks;

namespace LinkRouter.Settings.Core.Infrastructure;

public interface IMessageDialogService
{
    Task ShowMessageAsync(string title, string message);
    Task<bool> ShowConfirmationAsync(string title, string message, string confirmButtonText = "OK", string cancelButtonText = "Cancel");
}
