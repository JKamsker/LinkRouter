using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinkRouter;
using LinkRouter.Settings.Core.Services;
using LinkRouter.Settings.Services;
using Windows.ApplicationModel.DataTransfer;

namespace LinkRouter.Settings.ViewModels;

public partial class GeneralViewModel : ObservableObject
{
    private readonly ConfigService _configService = AppServices.ConfigService;
    private readonly RuleTestService _ruleTestService = AppServices.RuleTestService;
    private readonly ConfigurationState _state = AppServices.ConfigurationState;

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

    public ObservableCollection<ConfigBackup> Backups { get; } = new();
    public bool HasUnsavedChanges => _state.HasUnsavedChanges;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public string LastModifiedDisplay => LastModified?.ToLocalTime().ToString("G") ?? string.Empty;

    public GeneralViewModel()
    {
        LoadMetadata();
        _state.StateChanged += OnStateChanged;
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        LoadMetadata();
        OnPropertyChanged(nameof(HasUnsavedChanges));

        if (_state.HasUnsavedChanges)
        {
            StatusMessage = "Unsaved changes";
        }
        else if (string.IsNullOrWhiteSpace(StatusMessage))
        {
            StatusMessage = "All changes saved.";
        }
    }

    private void LoadMetadata()
    {
        ConfigPath = _configService.ConfigPath;
        Backups.Clear();
        if (_state.Document is ConfigDocument document)
        {
            LastModified = document.LastModified;
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

        var config = _state.BuildConfig();
        try
        {
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
                ? "No rule matched; using default rule."
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

    private static string BuildDetails(Rule rule, string launchArgs)
    {
        return $"Browser: {rule.browser}\nArgs: {launchArgs}";
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
            await _configService.SaveAsync(config);
            var document = await _configService.LoadAsync();
            _state.Load(document);
            StatusMessage = "Saved.";
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
    private async Task RegisterAsync()
    {
        ErrorMessage = null;
        try
        {
            await Task.Run(DefaultAppRegistrar.RegisterPerUser);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task UnregisterAsync()
    {
        ErrorMessage = null;
        try
        {
            await Task.Run(DefaultAppRegistrar.UnregisterPerUser);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
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
    }

    partial void OnErrorMessageChanged(string? value)
    {
        OnPropertyChanged(nameof(HasError));
    }

    partial void OnLastModifiedChanged(DateTime? value)
    {
        OnPropertyChanged(nameof(LastModifiedDisplay));
    }
}
