using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using FluentAvalonia.UI.Controls;

namespace LinkRouter.Settings.Avalonia.Behaviors;

/// <summary>
/// Automatically toggles the navigation pane based on user navigation and pointer proximity.
/// Keeps the pane expanded until a navigation item is invoked, then collapses after navigation and only
/// re-expands when the pointer returns after leaving the compact pane area.
/// </summary>
public sealed class NavigationViewAutoCollapseBehavior : Behavior<NavigationView>
{
    private bool _hasUserInvokedItem;
    private bool _awaitingPointerExit;
    private bool _suspendAutoCollapse;
    private bool _isProgrammaticPaneChange;
    private bool _userToggledPane;

    public static readonly StyledProperty<double> HoverActivationMarginProperty =
        AvaloniaProperty.Register<NavigationViewAutoCollapseBehavior, double>(
            nameof(HoverActivationMargin), 16d);

    public static readonly StyledProperty<double> CollapseProximityMarginProperty =
        AvaloniaProperty.Register<NavigationViewAutoCollapseBehavior, double>(
            nameof(CollapseProximityMargin), 16d);

    public double HoverActivationMargin
    {
        get => GetValue(HoverActivationMarginProperty);
        set => SetValue(HoverActivationMarginProperty, value);
    }

    public double CollapseProximityMargin
    {
        get => GetValue(CollapseProximityMarginProperty);
        set => SetValue(CollapseProximityMarginProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is not { } navigationView)
        {
            return;
        }

        // Ensure the pane starts expanded for the initial view.
        SetPaneOpen(navigationView, true);

        navigationView.PaneOpening += OnPaneOpening;
        navigationView.PaneClosing += OnPaneClosing;

        navigationView.AddHandler(
            InputElement.PointerMovedEvent,
            OnPointerMoved,
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble);

        navigationView.AddHandler(
            InputElement.PointerExitedEvent,
            OnPointerExited,
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble);

        navigationView.SelectionChanged += OnSelectionChanged;
        navigationView.ItemInvoked += OnItemInvoked;
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject is { } navigationView)
        {
            navigationView.PaneOpening -= OnPaneOpening;
            navigationView.PaneClosing -= OnPaneClosing;
            navigationView.RemoveHandler(InputElement.PointerMovedEvent, OnPointerMoved);
            navigationView.RemoveHandler(InputElement.PointerExitedEvent, OnPointerExited);
            navigationView.SelectionChanged -= OnSelectionChanged;
            navigationView.ItemInvoked -= OnItemInvoked;
        }

        base.OnDetaching();
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (AssociatedObject is not { } navigationView ||
            navigationView.PaneDisplayMode != NavigationViewPaneDisplayMode.Left ||
            e.Pointer.Type != PointerType.Mouse)
        {
            return;
        }

        var pointerPosition = e.GetPosition(navigationView);
        if (!navigationView.IsPaneOpen)
        {
            if (_awaitingPointerExit && pointerPosition.X > navigationView.CompactPaneLength + HoverActivationMargin)
            {
                _awaitingPointerExit = false;
            }

            var activationWidth = navigationView.CompactPaneLength + HoverActivationMargin;
            if (!_awaitingPointerExit &&
                pointerPosition.X <= activationWidth &&
                pointerPosition.X >= -HoverActivationMargin)
            {
                SetPaneOpen(navigationView, true);
            }
        }
        else
        {
            // Don't auto-collapse if user manually toggled pane or auto-collapse is suspended
            if (_suspendAutoCollapse || _userToggledPane)
            {
                return;
            }

            if (!_hasUserInvokedItem)
            {
                return;
            }

            var collapseBoundary = navigationView.OpenPaneLength + CollapseProximityMargin;
            if (pointerPosition.X > collapseBoundary ||
                pointerPosition.Y < -CollapseProximityMargin ||
                pointerPosition.Y > navigationView.Bounds.Height + CollapseProximityMargin)
            {
                SetPaneOpen(navigationView, false);
            }
        }
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        if (e.Pointer.Type != PointerType.Mouse)
        {
            return;
        }

        if (_suspendAutoCollapse || _userToggledPane)
        {
            return;
        }

        if (!_hasUserInvokedItem)
        {
            return;
        }

        _awaitingPointerExit = false;
        CollapsePane();
    }

    private void OnSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (_hasUserInvokedItem && e.SelectedItem != null)
        {
            CollapsePane();
        }
    }

    private void OnItemInvoked(object? sender, NavigationViewItemInvokedEventArgs e)
    {
        _hasUserInvokedItem = true;
        _suspendAutoCollapse = false;
        _userToggledPane = false; // Reset manual toggle state when navigating
        CollapsePane(waitForPointerExit: true);
    }

    private void OnPaneOpening(NavigationView sender, object args)
    {
        if (_isProgrammaticPaneChange)
        {
            return;
        }

        // User manually toggled the pane open
        _userToggledPane = true;
        _suspendAutoCollapse = true;
        _awaitingPointerExit = false;
    }

    private void OnPaneClosing(NavigationView sender, NavigationViewPaneClosingEventArgs args)
    {
        if (_isProgrammaticPaneChange)
        {
            return;
        }

        // User manually toggled the pane closed
        _userToggledPane = false;
        _suspendAutoCollapse = false;
    }

    private void CollapsePane(bool waitForPointerExit = false, bool force = false)
    {
        if (AssociatedObject is not { } navigationView ||
            navigationView.PaneDisplayMode != NavigationViewPaneDisplayMode.Left)
        {
            return;
        }

        if (!force)
        {
            if (!_hasUserInvokedItem || _suspendAutoCollapse)
            {
                return;
            }
        }

        if (navigationView.IsPaneOpen)
        {
            SetPaneOpen(navigationView, false);
        }

        if (waitForPointerExit)
        {
            _awaitingPointerExit = true;
        }
    }

    private void SetPaneOpen(NavigationView navigationView, bool isOpen)
    {
        if (navigationView.IsPaneOpen == isOpen)
        {
            return;
        }

        _isProgrammaticPaneChange = true;
        try
        {
            navigationView.IsPaneOpen = isOpen;
        }
        finally
        {
            _isProgrammaticPaneChange = false;
        }
    }
}
