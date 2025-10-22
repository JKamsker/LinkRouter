using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinkRouter;
using LinkRouter.Settings.Services;
using LinkRouter.Settings.Services.Abstractions;

namespace LinkRouter.Settings.ViewModels;

public partial class GeneralViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private readonly RuleTestService _ruleTestService;
    private readonly ConfigurationState _state;
    private readonly IShellService _shellService;
    private readonly IClipboardService _clipboardService;
    private readonly IRouterPathResolver _routerPathResolver;
    private readonly IAutostartService _autostartService;
    private bool _simulationOwnsError;
    private Rule? _lastEffectiveRule;
    private string? _lastLaunchArguments;
    private bool _suppressAutostartPropagation;

    [ObservableProperty]
    private string _configPath = string.Empty;

    [ObservableProperty]
    private DateTime? _lastModified;

    [ObservableProperty]
    private int _backupCount;

    [ObservableProperty]
    private string _testUrl = string.Empty;

    [ObservableProperty]
    private string? _simulationSummary;

    [ObservableProperty]
    private string? _simulationDetails;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _isAutostartEnabled;

    public ObservableCollection<ConfigBackup> Backups { get; } = new();
    public bool HasUnsavedChanges => _state.HasUnsavedChanges;
    public bool CanSave => HasUnsavedChanges && !IsSaving;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public string LastModifiedDisplay => LastModified?.ToLocalTime().ToString("G") ?? string.Empty;
    public bool CanConfigureAutostart => _autostartService.IsSupported;
    public bool CannotConfigureAutostart => !CanConfigureAutostart;

    public GeneralViewModel(
        ConfigService configService,
        RuleTestService ruleTestService,
        ConfigurationState state,
        IShellService shellService,
        IClipboardService clipboardService,
        IRouterPathResolver routerPathResolver,
        IAutostartService autostartService)
    {
        _configService = configService;
        _ruleTestService = ruleTestService;
        _state = state;
        _shellService = shellService;
        _clipboardService = clipboardService;
        _routerPathResolver = routerPathResolver;
        _autostartService = autostartService;

        LoadMetadata();
        _state.StateChanged += OnStateChanged;
        SyncAutostartFromState();
        SetLaunchContext(null, null);
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        LoadMetadata();
        OnPropertyChanged(nameof(HasUnsavedChanges));
        OnPropertyChanged(nameof(CanSave));

        if (_state.HasUnsavedChanges)
        {
            StatusMessage = "Unsaved changes";
        }
        else if (string.IsNullOrWhiteSpace(StatusMessage))
        {
            StatusMessage = "All changes saved.";
        }

        UpdateSimulation();
        SyncAutostartFromState();
    }

    private void LoadMetadata()
    {
        ConfigPath = _configService.ConfigPath;
        Backups.Clear();
        if (_state.Document is ConfigDocument document)
        {
            LastModified = document.SettingsLastModified;
            BackupCount = document.Backups.Count;
            foreach (var backup in document.Backups)
            {
                Backups.Add(backup);
            }
        }
        else
        {
            LastModified = null;
            BackupCount = 0;
        }
    }

    private void UpdateSimulation()
    {
        SimulationSummary = null;
        SimulationDetails = null;
        SetSimulationError(null);
        SetLaunchContext(null, null);

        if (string.IsNullOrWhiteSpace(TestUrl))
        {
            return;
        }

        var config = _state.BuildConfig();
        try
        {
            var result = _ruleTestService.Test(config, TestUrl);
            if (!result.Success)
            {
                if (!string.IsNullOrWhiteSpace(result.NormalizedUrl))
                {
                    SimulationSummary = $"Normalized URL: {result.NormalizedUrl}";
                }

                SetSimulationError(result.Error);
                return;
            }

            SimulationSummary = result.MatchedRule is null
                ? "No rule matched; using default rule."
                : $"Matched rule: {result.MatchedRule.match} - {result.MatchedRule.pattern}";

            SimulationDetails = result.EffectiveRule is null
                ? null
                : BuildDetails(result.EffectiveRule, result.LaunchArguments ?? string.Empty);
            SetLaunchContext(result.EffectiveRule, result.LaunchArguments);
        }
        catch (Exception ex)
        {
            SimulationSummary = null;
            SimulationDetails = null;
            SetSimulationError(ex.Message);
            SetLaunchContext(null, null);
        }
    }

    private static string BuildDetails(Rule rule, string launchArgs)
    {
        var argsDisplay = string.IsNullOrWhiteSpace(launchArgs) ? "(none)" : launchArgs;
        return $"Would launch:{Environment.NewLine}Browser: {rule.browser}{Environment.NewLine}Args: {argsDisplay}";
    }

    partial void OnTestUrlChanged(string value)
    {
        UpdateSimulation();
    }

    [RelayCommand(CanExecute = nameof(CanLaunchTest))]
    private void LaunchTest()
    {
        if (_lastEffectiveRule?.browser is null)
        {
            return;
        }

        try
        {
            SetOperationError(null);

            var startInfo = new ProcessStartInfo
            {
                FileName = _lastEffectiveRule.browser,
                Arguments = _lastLaunchArguments ?? string.Empty,
                UseShellExecute = false
            };

            if (!string.IsNullOrWhiteSpace(_lastEffectiveRule.workingDirectory))
            {
                startInfo.WorkingDirectory = _lastEffectiveRule.workingDirectory;
            }

            Process.Start(startInfo);
            StatusMessage = "Launched test URL.";
        }
        catch (Exception ex)
        {
            SetOperationError(ex.Message);
        }
    }

    private bool CanLaunchTest() => _lastEffectiveRule?.browser is not null;

    [RelayCommand]
    private async Task SaveAsync()
    {
        SetOperationError(null);
        StatusMessage = null;
        IsSaving = true;
        StatusMessage = "Saving...";

        try
        {
            var snapshot = _state.BuildSettingsSnapshot();
            await _configService.SaveAsync(snapshot);
            var document = await _configService.LoadAsync();
            _state.Load(document);
            if (_autostartService.IsSupported)
            {
                _autostartService.SetEnabled(snapshot.ApplicationSettings.AutostartEnabled);
            }
            StatusMessage = "Saved.";
        }
        catch (Exception ex)
        {
            SetOperationError(ex.Message);
        }
        finally
        {
            IsSaving = false;
        }
    }

    partial void OnIsSavingChanged(bool value)
    {
        OnPropertyChanged(nameof(CanSave));
    }

    partial void OnIsAutostartEnabledChanged(bool value)
    {
        if (_suppressAutostartPropagation)
        {
            return;
        }

        _state.SetAutostartEnabled(value);
    }

    [RelayCommand]
    private async Task DiscardChangesAsync()
    {
        SetOperationError(null);
        StatusMessage = null;

        try
        {
            var document = await _configService.LoadAsync();
            _state.Load(document);
            StatusMessage = "Changes discarded.";
        }
        catch (Exception ex)
        {
            SetOperationError(ex.Message);
        }
    }

    [RelayCommand]
    [SupportedOSPlatform("windows")]
    private async Task RegisterAsync()
    {
        SetOperationError(null);
        StatusMessage = null;

        if (!_routerPathResolver.TryGetRouterExecutable(out var routerPath))
        {
            SetOperationError("Unable to locate the LinkRouter.Launcher executable. Launch the settings installer or build the CLI project.");
            return;
        }

        try
        {
            await Task.Run(() => DefaultAppRegistrar.RegisterPerUser(routerPath));
            StatusMessage = "Registration command executed. Windows Settings will prompt for defaults.";
        }
        catch (Exception ex)
        {
            SetOperationError(ex.Message);
        }
    }

    [RelayCommand]
    [SupportedOSPlatform("windows")]
    private async Task UnregisterAsync()
    {
        SetOperationError(null);
        StatusMessage = null;
        try
        {
            await Task.Run(DefaultAppRegistrar.UnregisterPerUser);
            StatusMessage = "Unregistration command executed.";
        }
        catch (Exception ex)
        {
            SetOperationError(ex.Message);
        }
    }

    [RelayCommand]
    private void OpenConfigFolder()
    {
        try
        {
            var folder = Path.GetDirectoryName(_configService.ConfigPath);
            if (!string.IsNullOrEmpty(folder))
            {
                _shellService.OpenFolder(folder);
            }
        }
        catch (Exception ex)
        {
            SetOperationError(ex.Message);
        }
    }

    [RelayCommand]
    private void CopyConfigPath()
    {
        try
        {
            _clipboardService.SetText(_configService.ConfigPath);
        }
        catch (Exception ex)
        {
            SetOperationError(ex.Message);
        }
    }

    private void SetLaunchContext(Rule? rule, string? arguments)
    {
        _lastEffectiveRule = rule;
        _lastLaunchArguments = arguments;
        LaunchTestCommand.NotifyCanExecuteChanged();
    }

    private void SetOperationError(string? message)
    {
        _simulationOwnsError = false;
        ErrorMessage = message;
    }

    private void SetSimulationError(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            if (_simulationOwnsError)
            {
                ErrorMessage = null;
                _simulationOwnsError = false;
            }

            return;
        }

        ErrorMessage = message;
        _simulationOwnsError = true;
    }

    partial void OnErrorMessageChanged(string? value)
    {
        OnPropertyChanged(nameof(HasError));
    }

    partial void OnLastModifiedChanged(DateTime? value)
    {
        OnPropertyChanged(nameof(LastModifiedDisplay));
    }

    private void SyncAutostartFromState()
    {
        var desired = _state.IsAutostartEnabled;
        if (IsAutostartEnabled == desired && _suppressAutostartPropagation == false)
        {
            return;
        }

        _suppressAutostartPropagation = true;
        try
        {
            IsAutostartEnabled = desired;
        }
        finally
        {
            _suppressAutostartPropagation = false;
        }
    }
}
