using System;
using System.Collections.ObjectModel;
using System.IO;
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
    public bool CanSave => HasUnsavedChanges && !IsSaving;

    public GeneralViewModel(
        ConfigService configService,
        RuleTestService ruleTestService,
        ConfigurationState state,
        IShellService shellService,
        IClipboardService clipboardService)
    {
        _configService = configService;
        _ruleTestService = ruleTestService;
        _state = state;
        _shellService = shellService;
        _clipboardService = clipboardService;

        LoadMetadata();
        _state.StateChanged += OnStateChanged;
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
        IsSaving = true;
        StatusMessage = "Saving...";

        try
        {
            var snapshot = _state.BuildSettingsSnapshot();
            await _configService.SaveAsync(snapshot);
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

    partial void OnIsSavingChanged(bool value)
    {
        OnPropertyChanged(nameof(CanSave));
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
                _shellService.OpenFolder(folder);
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
        try
        {
            _clipboardService.SetText(_configService.ConfigPath);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
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
