namespace LinkRouter.Settings.Services.Abstractions;

/// <summary>
/// Platform-specific service for registering LinkRouter as a default browser/URL handler.
/// </summary>
public interface IDefaultAppRegistrar
{
    /// <summary>
    /// Registers LinkRouter as a URL handler (http/https) for the current user.
    /// </summary>
    /// <param name="executablePath">Optional path to the launcher executable. If null, will be auto-detected.</param>
    /// <param name="appUserModelId">Optional Windows AppUserModelId for deep linking to settings.</param>
    void RegisterPerUser(string? executablePath = null, string? appUserModelId = null);

    /// <summary>
    /// Unregisters LinkRouter as a URL handler for the current user.
    /// </summary>
    void UnregisterPerUser();
}
