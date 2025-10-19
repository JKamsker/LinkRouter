using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using LinkRouter.Settings.Services.Abstractions;
using LinkRouter.Settings.ViewModels;
using LinkRouter.Settings.Avalonia.Views;

namespace LinkRouter.Settings.Avalonia.Services;

internal sealed class AvaloniaDialogService : IDialogService
{
    private readonly Func<Window?> _getWindow;

    public AvaloniaDialogService(Func<Window?> getWindow)
    {
        _getWindow = getWindow;
    }

    public async Task<bool> ShowRuleEditorAsync(
        RuleEditorViewModel rule,
        IReadOnlyList<string> matchTypes,
        IReadOnlyList<string> profileOptions,
        CancellationToken cancellationToken = default)
    {
        using var context = new RuleEditorDialogContext(rule, matchTypes, profileOptions);
        var dialog = new RuleEditorDialog
        {
            DataContext = context
        };

        var owner = _getWindow();
        ContentDialogResult result;
        if (owner is not null)
        {
            result = await dialog.ShowAsync(owner).ConfigureAwait(false);
        }
        else
        {
            result = await dialog.ShowAsync().ConfigureAwait(false);
        }

        return result == ContentDialogResult.Primary;
    }

    private sealed partial class RuleEditorDialogContext : ObservableObject, IDisposable
    {
        public RuleEditorDialogContext(
            RuleEditorViewModel rule,
            IReadOnlyList<string> matchTypes,
            IReadOnlyList<string> profileOptions)
        {
            Rule = rule;
            MatchTypes = matchTypes;
            ProfileOptions = profileOptions;
            ClearUseProfileCommand = new RelayCommand(ClearUseProfile, CanClearUseProfile);
            Rule.PropertyChanged += OnRulePropertyChanged;
        }

        public RuleEditorViewModel Rule { get; }

        public IReadOnlyList<string> MatchTypes { get; }

        public IReadOnlyList<string> ProfileOptions { get; }

        public IRelayCommand ClearUseProfileCommand { get; }

        private void ClearUseProfile()
        {
            Rule.UseProfile = null;
        }

        private bool CanClearUseProfile() => !string.IsNullOrWhiteSpace(Rule.UseProfile);

        private void OnRulePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RuleEditorViewModel.UseProfile))
            {
                ClearUseProfileCommand.NotifyCanExecuteChanged();
            }
        }

        public void Dispose()
        {
            Rule.PropertyChanged -= OnRulePropertyChanged;
        }
    }
}
