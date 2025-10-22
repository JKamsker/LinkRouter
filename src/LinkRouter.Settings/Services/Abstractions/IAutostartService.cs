namespace LinkRouter.Settings.Services.Abstractions;

public interface IAutostartService
{
    bool IsSupported { get; }

    bool IsEnabled();

    void SetEnabled(bool enabled);
}
