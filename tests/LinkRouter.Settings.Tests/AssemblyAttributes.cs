using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(LinkRouter.Settings.Tests.TestAppBuilder))]
[assembly: CollectionBehavior(DisableTestParallelization = true)]
