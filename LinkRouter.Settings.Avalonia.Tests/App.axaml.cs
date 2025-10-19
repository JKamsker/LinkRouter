using Avalonia;
using Avalonia.Markup.Xaml;

namespace LinkRouter.Settings.Avalonia.Tests;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
