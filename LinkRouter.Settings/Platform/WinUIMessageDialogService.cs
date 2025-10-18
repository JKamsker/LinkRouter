using System.Threading.Tasks;
using LinkRouter.Settings.Services.Abstractions;

namespace LinkRouter.Settings.Platform;

internal sealed class WinUIMessageDialogService : IMessageDialogService
{
    public Task<MessageDialogResult> ShowMessageAsync(string title, string message, MessageDialogOptions options)
    {
        return Task.FromResult(MessageDialogResult.None);
    }
}
