using System.Threading.Tasks;
using LinkRouter.Settings.Services.Abstractions;
using LinkRouter.Settings.ViewModels;

namespace LinkRouter.Settings.Services;

public sealed class AppInitializationService
{
    private readonly ConfigService _configService;
    private readonly ConfigurationState _configurationState;
    private readonly IAutostartService _autostartService;

    public AppInitializationService(ConfigService configService, ConfigurationState configurationState, IAutostartService autostartService)
    {
        _configService = configService;
        _configurationState = configurationState;
        _autostartService = autostartService;
    }

    public async Task InitializeAsync()
    {
        try
        {
            var document = await _configService.LoadAsync().ConfigureAwait(false);
            _configurationState.Load(document);
            if (_autostartService.IsSupported)
            {
                _autostartService.SetEnabled(document.ApplicationSettings.AutostartEnabled);
            }
        }
        catch
        {
            // Allow the UI layer to surface initialization errors when the user interacts with the app.
        }
    }
}
