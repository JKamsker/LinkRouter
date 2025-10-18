using System;
using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinkRouter.Settings.Services;

namespace LinkRouter.Settings.ViewModels;

public partial class AdvancedViewModel : ObservableObject
{
    private readonly ConfigService _configService = AppServices.ConfigService;
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
                Process.Start(new ProcessStartInfo
                {
                    FileName = folder,
                    UseShellExecute = true
                });
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
                Process.Start(new ProcessStartInfo
                {
                    FileName = _logFilePath,
                    UseShellExecute = true
                });
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
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "ms-settings:defaultapps",
                    UseShellExecute = true
                });
            }
            else
            {
                Error = "Default apps settings are only available on Windows.";
            }
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }
}
