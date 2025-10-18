using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinkRouter.Settings.Core.Services;
using LinkRouter.Settings.Core.Infrastructure;

namespace LinkRouter.Settings.Core.ViewModels;

public partial class AdvancedViewModel : ObservableObject
{
    private readonly ConfigService _configService = SettingsServiceLocator.ConfigService;
    private readonly string _logFilePath;
    private readonly string _loggingTogglePath;

    [ObservableProperty]
    private bool _isLoggingEnabled;

    [ObservableProperty]
    private string? _error;

    public AdvancedViewModel()
    {
        var configFolder = Path.GetDirectoryName(_configService.ConfigPath) ?? string.Empty;
        _logFilePath = Path.Combine(configFolder, "args.log");
        _loggingTogglePath = Path.Combine(configFolder, "logging.disabled");
        IsLoggingEnabled = !File.Exists(_loggingTogglePath);
    }

    [RelayCommand]
    private void OpenConfigFolder()
    {
        try
        {
            var folder = Path.GetDirectoryName(_configService.ConfigPath);
            if (!string.IsNullOrEmpty(folder))
            {
                SettingsServiceLocator.Launcher.OpenFolder(folder);
            }
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    [RelayCommand]
    private void OpenLogFile()
    {
        try
        {
            if (File.Exists(_logFilePath))
            {
                SettingsServiceLocator.Launcher.OpenFile(_logFilePath);
            }
            else
            {
                Error = "Log file not found.";
            }
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    partial void OnIsLoggingEnabledChanged(bool value)
    {
        try
        {
            if (value)
            {
                if (File.Exists(_loggingTogglePath))
                {
                    File.Delete(_loggingTogglePath);
                }
            }
            else
            {
                File.WriteAllText(_loggingTogglePath, "disabled");
            }
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    [RelayCommand]
    private void OpenDefaultApps()
    {
        try
        {
            SettingsServiceLocator.Launcher.OpenUri("ms-settings:defaultapps");
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }
}
