using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinkRouter;
using LinkRouter.Settings.Services;
using Windows.ApplicationModel.DataTransfer;

namespace LinkRouter.Settings.ViewModels;

public partial class OverviewViewModel : ObservableObject
{
    private readonly ConfigService _configService = AppServices.ConfigService;
    private readonly ConfigurationState _state = AppServices.ConfigurationState;
    private readonly RuleTestService _ruleTestService = AppServices.RuleTestService;

    [ObservableProperty]
    private string _configPath = string.Empty;

    [ObservableProperty]
    private DateTime? _lastModified;

    [ObservableProperty]
    private int _backupCount;

    [ObservableProperty]
    private ConfigBackup? _latestBackup;

    [ObservableProperty]
    private string? _statusMessage;

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
    private bool _isOperationRunning;

    public OverviewViewModel()
    {
        LoadSnapshot();
        _state.StateChanged += OnStateChanged;
    }

    public bool HasUnsavedChanges => _state.HasUnsavedChanges;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool HasLatestBackup => LatestBackup is not null;

    public string LastModifiedDisplay => LastModified?.ToLocalTime().ToString("G") ?? "Never";

    public string LatestBackupDisplay => LatestBackup?.TimestampDisplay ?? "No backups yet";

    public string? LatestBackupSizeDisplay => LatestBackup is null ? null : $"{LatestBackup.SizeBytes / 1024d:0.#} KB";

    private void LoadSnapshot()
    {
        ConfigPath = _configService.ConfigPath;
        if (_state.Document is ConfigDocument document)
        {
            LastModified = document.LastModified;
            BackupCount = document.Backups.Count;
            LatestBackup = document.Backups.Count > 0 ? document.Backups[0] : null;
        }
        else
        {
            LastModified = null;
            BackupCount = 0;
            LatestBackup = null;
        }
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        LoadSnapshot();
        OnPropertyChanged(nameof(HasUnsavedChanges));
    }

    [RelayCommand]
    private void OpenConfigFolder()
    {
        try
        {
            var folder = Path.GetDirectoryName(_configService.ConfigPath);
            if (!string.IsNullOrEmpty(folder))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = folder,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private void CopyConfigPath()
    {
        var package = new DataPackage
        {
            RequestedOperation = DataPackageOperation.Copy
        };
        package.SetText(_configService.ConfigPath);
        Clipboard.SetContent(package);
        StatusMessage = "Config path copied.";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = null;
        StatusMessage = null;
        IsSaving = true;

        try
        {
            var config = _state.BuildConfig();
            await _configService.SaveAsync(config).ConfigureAwait(false);
            var document = await _configService.LoadAsync().ConfigureAwait(false);
            _state.Load(document);
            StatusMessage = "Changes saved.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task RestoreLatestBackupAsync()
    {
        if (LatestBackup is null)
        {
            StatusMessage = "No backups available.";
            return;
        }

        ErrorMessage = null;
        StatusMessage = null;
        IsOperationRunning = true;

        try
        {
            var document = await _configService.RestoreBackupAsync(LatestBackup).ConfigureAwait(false);
            _state.Load(document);
            StatusMessage = $"Restored {LatestBackup.FileName}.";
        }
        catch (FileNotFoundException)
        {
            ErrorMessage = "Latest backup file could not be found.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsOperationRunning = false;
        }
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        await RunRegistrationAsync(DefaultAppRegistrar.RegisterPerUser, "Opened Windows Settings to register LinkRouter.");
    }

    [RelayCommand]
    private async Task UnregisterAsync()
    {
        await RunRegistrationAsync(DefaultAppRegistrar.UnregisterPerUser, "Removed LinkRouter registration.");
    }

    private async Task RunRegistrationAsync(Action registrationAction, string successMessage)
    {
        ErrorMessage = null;
        StatusMessage = null;
        IsOperationRunning = true;

        try
        {
            await Task.Run(registrationAction).ConfigureAwait(false);
            StatusMessage = successMessage;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsOperationRunning = false;
        }
    }

    [RelayCommand]
    private void Simulate()
    {
        ErrorMessage = null;
        SimulationSummary = null;
        SimulationDetails = null;

        if (string.IsNullOrWhiteSpace(TestUrl))
        {
            ErrorMessage = "Enter a URL to test.";
            return;
        }

        try
        {
            var config = _state.BuildConfig();
            var result = _ruleTestService.Test(config, TestUrl);
            if (!result.Success)
            {
                ErrorMessage = result.Error;
                if (!string.IsNullOrWhiteSpace(result.NormalizedUrl))
                {
                    SimulationSummary = $"Normalized URL: {result.NormalizedUrl}";
                }
                return;
            }

            SimulationSummary = result.MatchedRule is null
                ? "No rule matched; the default rule will be used."
                : $"Matched rule: {result.MatchedRule.match} - {result.MatchedRule.pattern}";

            SimulationDetails = result.EffectiveRule is null
                ? null
                : BuildDetails(result.EffectiveRule, result.LaunchArguments ?? string.Empty);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private void ClearSimulation()
    {
        ErrorMessage = null;
        SimulationSummary = null;
        SimulationDetails = null;
        TestUrl = string.Empty;
    }

    private static string BuildDetails(Rule rule, string launchArgs)
    {
        return $"Browser: {rule.browser}\nArgs: {launchArgs}";
    }

    partial void OnErrorMessageChanged(string? value)
    {
        OnPropertyChanged(nameof(HasError));
    }

    partial void OnLastModifiedChanged(DateTime? value)
    {
        OnPropertyChanged(nameof(LastModifiedDisplay));
    }

    partial void OnLatestBackupChanged(ConfigBackup? value)
    {
        OnPropertyChanged(nameof(HasLatestBackup));
        OnPropertyChanged(nameof(LatestBackupDisplay));
        OnPropertyChanged(nameof(LatestBackupSizeDisplay));
    }
}
