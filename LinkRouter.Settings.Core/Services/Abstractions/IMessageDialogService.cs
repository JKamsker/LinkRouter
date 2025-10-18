using System.Threading.Tasks;

namespace LinkRouter.Settings.Services.Abstractions;

public interface IMessageDialogService
{
    Task<MessageDialogResult> ShowMessageAsync(string title, string message, MessageDialogOptions options);
}

public sealed record MessageDialogOptions(string PrimaryButtonText, string? SecondaryButtonText = null, string? CloseButtonText = null);

public enum MessageDialogResult
{
    None,
    Primary,
    Secondary,
    Close
}
