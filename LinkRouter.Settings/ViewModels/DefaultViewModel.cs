using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinkRouter;
using LinkRouter.Settings.Services;

namespace LinkRouter.Settings.ViewModels;

public partial class DefaultViewModel : ObservableObject
{
    private readonly ConfigurationState _state = AppServices.ConfigurationState;

    [ObservableProperty]
    private string? _preview;

    [ObservableProperty]
    private string? _error;

    public DefaultRuleViewModel DefaultRule => _state.DefaultRule;

    public DefaultViewModel()
    {
        DefaultRule.PropertyChanged += OnDefaultRuleChanged;
        _state.StateChanged += OnStateChanged;
        UpdatePreview();
    }

    private void OnDefaultRuleChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdatePreview();
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        try
        {
            var rule = DefaultRule.ToRuleOrNull();
            if (rule is null)
            {
                Preview = "Default rule disabled.";
                Error = null;
                return;
            }

            var config = _state.BuildConfig();
            var resolved = ProfileResolver.ResolveEffectiveRule(config, rule);
            var args = BrowserLauncher.GetLaunchArguments(resolved, new Uri("https://example.com/"));
            Preview = $"Browser: {resolved.browser}\nArgs: {args}";
            Error = null;
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
    }

    [RelayCommand]
    private void ClearProfile()
    {
        DefaultRule.Profile = null;
        DefaultRule.UseProfile = null;
    }
}
