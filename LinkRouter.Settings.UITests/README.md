# LinkRouter.Settings.UITests

UI-focused, opt-in tests that instantiate WinUI 3 pages/controls on a real UI thread.

These tests are skipped by default to keep regular `dotnet test` runs fast and stable. Enable them only when you want to validate runtime instantiation in a proper WinUI host.

## Prerequisites

- Windows 10 2004 (19041) or later
- .NET 9 SDK
- x64 test host (don’t run AnyCPU/x86)
- Windows App SDK compatible environment

## Enable And Run (Command Line)

The UI tests are disabled by default and only run when `LINKROUTER_UI_TESTS=1` is set.

PowerShell:

```powershell
$env:LINKROUTER_UI_TESTS = '1'
dotnet test LinkRouter.sln --filter FullyQualifiedName~LinkRouter.Settings.UITests
```

Cmd:

```cmd
set LINKROUTER_UI_TESTS=1
dotnet test LinkRouter.sln --filter FullyQualifiedName~LinkRouter.Settings.UITests
```

To run only this project:

```powershell
$env:LINKROUTER_UI_TESTS = '1'
dotnet test LinkRouter.Settings.UITests/LinkRouter.Settings.UITests.csproj
```

## Rider

- Create a Unit Test run configuration targeting `LinkRouter.Settings.UITests`.
- Environment variables: add `LINKROUTER_UI_TESTS=1`.
- Platform/architecture: force `x64`.
- Prefer “run in external process” (if available).
- If you see host crashes, run from terminal with the commands above.

## Visual Studio

- Test Settings → Configure Run Settings → ensure `x64`.
- Test Explorer: run only the `LinkRouter.Settings.UITests` project.
- Set environment variable for the test session (Test → Configure Run Settings) or run from Developer PowerShell:

```powershell
$env:LINKROUTER_UI_TESTS = '1'
dotnet test LinkRouter.Settings.UITests/LinkRouter.Settings.UITests.csproj
```

## Build Notes (WinUI 3)

If you hit build/runtime issues with WinUI, build once using the Visual Studio DevShell + MSBuild (this primes the environment), then `dotnet build/test` should work fine afterwards:

```powershell
Import-Module 'C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\Tools\Microsoft.VisualStudio.DevShell.dll'
Enter-VsDevShell 3e25b51d
cd C:\Users\Jonas\repos\private\LinkRouter
msbuild LinkRouter.sln /t:Rebuild /p:Configuration=Release
```

After that, `dotnet test` should also run without issues.

## What These Tests Do

- Start a minimal WinUI `Application` on a dedicated STA thread with a message pump.
- Marshal test code onto that UI thread and instantiate:
  - `Views.GeneralPage`, `RulesPage`, `ProfilesPage`, `DefaultPage`, `ImportExportPage`, `AdvancedPage`, `AboutPage`
  - `Controls.SettingsCard`

## Why They’re Opt-In

- IDE test hosts sometimes aren’t friendly to WinUI app initialization (no message loop, wrong thread model, parallel runners).
- Opt-in prevents intermittent testhost crashes during normal CI and developer loops.

## Troubleshooting

- Testhost crashes or COMException 0x8001010E / 0x80040111:
  - Ensure `LINKROUTER_UI_TESTS=1` is set.
  - Force `x64` host.
  - Run only this project.
  - Prefer running from terminal with `dotnet test`.
- Builds failing on WinUI targets:
  - Run the VS DevShell + `msbuild` sequence above once, then use `dotnet test`.
- Rider still unstable:
  - Disable parallelism for this run; run a single test assembly per process.
  - Use terminal execution instead of the built-in runner.

## Related

- Non-UI XAML validation tests live in `LinkRouter.Settings.Tests` and run in all environments by default.

