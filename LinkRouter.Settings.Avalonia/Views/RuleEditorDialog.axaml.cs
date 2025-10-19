using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using LinkRouter.Settings.ViewModels;

namespace LinkRouter.Settings.Avalonia.Views;

public partial class RuleEditorDialog : ContentDialog, IRuleEditorDialog
{
    public RuleEditorDialog()
    {
        InitializeComponent();
    }

    public void Configure(RuleEditorViewModel rule, IEnumerable<string> matchTypes, IEnumerable<string> profileOptions)
    {
        DataContext = rule;
        MatchTypeCombo.ItemsSource = matchTypes;
        UseProfileCombo.ItemsSource = profileOptions;
    }

    public new async Task ShowAsync(Window? owner)
    {
        var target = (Visual?)owner ?? TopLevel.GetTopLevel(this);
        OverlayLayer? overlay = target is null ? null : OverlayLayer.GetOverlayLayer(target);
        Border? dimmer = null;
        IDisposable? boundsSubscription = null;

        if (overlay is not null)
        {
            dimmer = new Border
            {
                IsHitTestVisible = false
            };

            void UpdateSize(Rect rect)
            {
                dimmer.Width = rect.Width;
                dimmer.Height = rect.Height;
            }

            UpdateSize(overlay.Bounds);
            boundsSubscription = overlay.GetObservable(Visual.BoundsProperty).Subscribe(new ActionObserver<Rect>(UpdateSize));

            Canvas.SetLeft(dimmer, 0);
            Canvas.SetTop(dimmer, 0);
            overlay.Children.Add(dimmer);
        }

        try
        {
            if (owner is not null)
            {
                await base.ShowAsync(owner);
            }
            else
            {
                await base.ShowAsync();
            }
        }
        finally
        {
            boundsSubscription?.Dispose();

            if (overlay is not null && dimmer is not null)
            {
                overlay.Children.Remove(dimmer);
            }
        }
    }

    private void OnClearUseProfileClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is RuleEditorViewModel rule)
        {
            rule.UseProfile = null;
        }

        UseProfileCombo.SelectedItem = null;
    }

    private sealed class ActionObserver<T> : IObserver<T>
    {
        private readonly Action<T> _onNext;

        public ActionObserver(Action<T> onNext)
        {
            _onNext = onNext;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(T value)
        {
            _onNext(value);
        }
    }
}
