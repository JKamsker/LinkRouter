using System;
using System.Threading.Tasks;

namespace LinkRouter.Settings.Services.Abstractions;

public interface IDialogService
{
    Task<TResult?> ShowDialogAsync<TResult>(DialogRequest<TResult> request);
}

public sealed class DialogRequest<TResult>
{
    public DialogRequest(string key, object? viewModel)
    {
        DialogKey = key;
        ViewModel = viewModel;
    }

    public string DialogKey { get; }

    public object? ViewModel { get; }

    public string? Title { get; init; }

    public string? PrimaryButtonText { get; init; }

    public string? SecondaryButtonText { get; init; }

    public string? CloseButtonText { get; init; }

    public Func<DialogAction, TResult?>? ResultSelector { get; init; }
}

public enum DialogAction
{
    None,
    Primary,
    Secondary,
    Close
}
