using System.Threading.Tasks;
using LinkRouter.Settings.ViewModels;

namespace LinkRouter.Settings.Services;

public sealed class AppInitializationService
{
    private readonly ConfigService _configService;
    private readonly ConfigurationState _configurationState;

    public AppInitializationService(ConfigService configService, ConfigurationState configurationState)
    {
        _configService = configService;
        _configurationState = configurationState;
    }

    public async Task InitializeAsync()
    {
        try
        {
            var document = await _configService.LoadAsync().ConfigureAwait(false);
            _configurationState.Load(document);
        }
        catch
        {
            // Allow the UI layer to surface initialization errors when the user interacts with the app.
        }
    }
}
