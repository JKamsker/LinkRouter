using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(LinkRouter.Settings.Avalonia.Tests.TestAppBuilder))]
[assembly: CollectionBehavior(DisableTestParallelization = true)]
