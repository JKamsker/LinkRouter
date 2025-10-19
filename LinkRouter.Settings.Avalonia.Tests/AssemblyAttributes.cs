using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: AvaloniaTestApplication(typeof(LinkRouter.Settings.Avalonia.Tests.TestAppBuilder))]
