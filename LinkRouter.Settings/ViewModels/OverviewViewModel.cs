using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinkRouter;
using LinkRouter.Settings.Services;

namespace LinkRouter.Settings.ViewModels;

public partial class OverviewViewModel : ObservableObject
{
    private readonly ConfigurationState _state = AppServices.ConfigurationState;
    private readonly ConfigService _configService = AppServices.ConfigService;
    private readonly RuleTestService _ruleTestService = AppServices.RuleTestService;

    [ObservableProperty]
    private string _configPath = string.Empty;

    [ObservableProperty]
    private string _defaultDestination = "Not configured";

    [ObservableProperty]
    private string? _lastModifiedDisplay;

    [ObservableProperty]
    private int _backupCount;

    [ObservableProperty]
    private bool _hasUnsavedChanges;

    [ObservableProperty]
    private bool _isRegistered;

    [ObservableProperty]
    private bool _isOperationRunning;

    [ObservableProperty]
    private ConfigBackup? _latestBackup;

    [ObservableProperty]
    private string? _operationMessage;

    [ObservableProperty]
    private string? _operationError;

    [ObservableProperty]
    private string _testUrl = string.Empty;

    [ObservableProperty]
    private string? _simulationSummary;

    [ObservableProperty]
    private string? _simulationDetails;

    [ObservableProperty]
    private string? _simulationError;

    public OverviewViewModel()
    {
        RefreshMetadata();
        _state.StateChanged += OnStateChanged;
    }

    public string RegistrationStatus => IsRegistered ? "Registered as default handler" : "Not currently registered";

    public string ConfigFolder => Path.GetDirectoryName(ConfigPath) ?? string.Empty;

    public string LatestBackupSummary => LatestBackup is null
        ? "No backups created yet"
        : $"{LatestBackup.TimestampDisplay} ({LatestBackup.FileName})";

    public bool HasSimulationError => !string.IsNullOrEmpty(SimulationError);

    public bool HasSimulationDetails => !string.IsNullOrEmpty(SimulationSummary) || !string.IsNullOrEmpty(SimulationDetails);

    public bool HasOperationMessage => !string.IsNullOrEmpty(OperationMessage);

    public bool HasOperationError => !string.IsNullOrEmpty(OperationError);

    public async Task InitializeAsync()
    {
        await UpdateRegistrationStatusAsync().ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        await RunOperationAsync(async () =>
        {
            await Task.Run(DefaultAppRegistrar.RegisterPerUser).ConfigureAwait(false);
            await UpdateRegistrationStatusAsync().ConfigureAwait(false);
            OperationMessage = "Registration request sent.";
        }).ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task UnregisterAsync()
    {
        await RunOperationAsync(async () =>
        {
            await Task.Run(DefaultAppRegistrar.UnregisterPerUser).ConfigureAwait(false);
            await UpdateRegistrationStatusAsync().ConfigureAwait(false);
            OperationMessage = "LinkRouter unregistered.";
        }).ConfigureAwait(false);
    }

    [RelayCommand]
    private void Simulate()
    {
        SimulationError = null;
        SimulationSummary = null;
        SimulationDetails = null;

        if (string.IsNullOrWhiteSpace(TestUrl))
        {
            SimulationError = "Enter a URL to test.";
            return;
        }

        var config = _state.BuildConfig();
        try
        {
            var result = _ruleTestService.Test(config, TestUrl);
            if (!result.Success)
            {
                SimulationError = result.Error;
                if (!string.IsNullOrWhiteSpace(result.NormalizedUrl))
                {
                    SimulationSummary = $"Normalized URL: {result.NormalizedUrl}";
                }
                return;
            }

            SimulationSummary = result.MatchedRule is null
                ? "No rule matched; using default rule."
                : $"Matched rule: {result.MatchedRule.match} - {result.MatchedRule.pattern}";

            SimulationDetails = result.EffectiveRule is null
                ? null
                : BuildDetails(result.EffectiveRule, result.LaunchArguments ?? string.Empty);
        }
        catch (Exception ex)
        {
            SimulationError = ex.Message;
        }
    }

    [RelayCommand]
    private void OpenConfigFolder()
    {
        try
        {
            if (!string.IsNullOrEmpty(ConfigFolder))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = ConfigFolder,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            OperationError = ex.Message;
        }
    }

    [RelayCommand]
    private async Task RestoreLatestBackupAsync()
    {
        await RunOperationAsync(async () =>
        {
            var backup = LatestBackup;
            if (backup is null)
            {
                OperationError = "No backups available.";
                return;
            }

            if (!File.Exists(backup.Path))
            {
                OperationError = "Latest backup file is missing.";
                return;
            }

            var json = await File.ReadAllTextAsync(backup.Path).ConfigureAwait(false);
            var tempPath = Path.Combine(Path.GetTempPath(), $"LinkRouter_restore_{Guid.NewGuid():N}.json");
            await File.WriteAllTextAsync(tempPath, json).ConfigureAwait(false);

            var restored = ConfigLoader.LoadConfig(tempPath);
            await _configService.SaveAsync(restored).ConfigureAwait(false);
            var document = await _configService.LoadAsync().ConfigureAwait(false);
            _state.Load(document);
            OperationMessage = $"Restored backup {backup.FileName}.";
        }).ConfigureAwait(false);
    }

    private async Task RunOperationAsync(Func<Task> action)
    {
        OperationMessage = null;
        OperationError = null;
        IsOperationRunning = true;

        try
        {
            await action().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            OperationError = ex.Message;
        }
        finally
        {
            IsOperationRunning = false;
        }
    }

    private void RefreshMetadata()
    {
        ConfigPath = _configService.ConfigPath;
        HasUnsavedChanges = _state.HasUnsavedChanges;

        if (_state.Document is ConfigDocument document)
        {
            LastModifiedDisplay = document.LastModified?.ToLocalTime().ToString("G") ?? "Never saved";
            BackupCount = document.Backups.Count;
            LatestBackup = document.Backups.FirstOrDefault();
        }
        else
        {
            LastModifiedDisplay = "Never saved";
            BackupCount = 0;
            LatestBackup = null;
        }

        DefaultDestination = BuildDefaultDestination();
    }

    private async Task UpdateRegistrationStatusAsync()
    {
        OperationError = null;
        try
        {
            IsRegistered = await Task.Run(DefaultAppRegistrar.IsRegistered).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            OperationError = ex.Message;
            IsRegistered = false;
        }
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        RefreshMetadata();
    }

    private string BuildDefaultDestination()
    {
        var defaultRule = _state.DefaultRule;
        if (!defaultRule.Enabled)
        {
            return "Default rule disabled";
        }

        if (!string.IsNullOrWhiteSpace(defaultRule.Profile))
        {
            return $"Profile: {defaultRule.Profile}";
        }

        if (!string.IsNullOrWhiteSpace(defaultRule.UseProfile))
        {
            return $"Use profile hint: {defaultRule.UseProfile}";
        }

        if (!string.IsNullOrWhiteSpace(defaultRule.Browser))
        {
            return defaultRule.Browser!;
        }

        return "System default browser";
    }

    private static string BuildDetails(Rule rule, string launchArgs)
    {
        return $"Browser: {rule.browser}\nArgs: {launchArgs}";
    }

    partial void OnIsRegisteredChanged(bool value)
    {
        OnPropertyChanged(nameof(RegistrationStatus));
    }

    partial void OnLatestBackupChanged(ConfigBackup? value)
    {
        OnPropertyChanged(nameof(LatestBackupSummary));
    }

    partial void OnSimulationErrorChanged(string? value)
    {
        OnPropertyChanged(nameof(HasSimulationError));
    }

    partial void OnSimulationSummaryChanged(string? value)
    {
        OnPropertyChanged(nameof(HasSimulationDetails));
    }

    partial void OnSimulationDetailsChanged(string? value)
    {
        OnPropertyChanged(nameof(HasSimulationDetails));
    }

    partial void OnOperationMessageChanged(string? value)
    {
        OnPropertyChanged(nameof(HasOperationMessage));
    }

    partial void OnOperationErrorChanged(string? value)
    {
        OnPropertyChanged(nameof(HasOperationError));
    }
}
