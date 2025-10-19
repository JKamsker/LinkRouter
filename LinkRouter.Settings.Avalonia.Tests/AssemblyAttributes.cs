using Avalonia.Headless;
using LinkRouter.Settings.Avalonia;
using LinkRouter.Settings.Avalonia.Tests;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]
[assembly: CollectionBehavior(DisableTestParallelization = true)]
