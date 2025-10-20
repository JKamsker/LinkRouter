using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace LinkRouter.Settings.Avalonia;

public partial class MainWindow : Window
{
    private const double DragRegionHeight = 56;

    public MainWindow()
    {
        InitializeComponent();
        AddHandler(PointerPressedEvent, OnPointerPressed, RoutingStrategies.Bubble);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        var pointerPosition = e.GetPosition(this);

        if (pointerPosition.Y > DragRegionHeight)
        {
            return;
        }

        BeginMoveDrag(e);
        e.Handled = true;
    }
}
