# Repository Guidelines

## Project Structure & Module Organization
- `LinkRouter/` contains the console router that reads `mappings.json` and delegates link handling through the core library.
- `LinkRouter.Core/` is the cross-platform rules and launcher engine shared by every host.
- `LinkRouter.Settings/` hosts the Avalonia desktop settings app and shared settings logic; XAML layouts live under `Views/`, view-models under `ViewModels/`, and cross-layer services under `Services/`.
- `LinkRouter.Settings.Avalonia.Tests/` provides headless UI and integration tests; supporting fixtures sit under `Rules/` and `Startup/`.
- `docs/` holds design notes (e.g., `SettingsAppPlan.md`), and `run-tests.ps1` centralizes scripted `dotnet` invocations.

## Build, Test, and Development Commands
- Restore once per environment: `dotnet restore LinkRouter.sln`.
- Build Windows or cross-platform artifacts: `dotnet build LinkRouter.sln -c Debug`.
- Launch the router CLI: `dotnet run --project LinkRouter/LinkRouter.csproj -- <url>`.
- Launch the settings UI: `dotnet run --project LinkRouter.Settings/LinkRouter.Settings.csproj`.
- Run the full suite (2-minute timeout baked in): `pwsh ./run-tests.ps1`. Pass extra `dotnet test` arguments via `-Arguments @('test','--filter', 'FullyQualifiedName~RuleEditor')`.

## Coding Style & Naming Conventions
- .NET 9.0 (`global.json`) is required; keep SDKs aligned before pushing.
- Follow `.editorconfig`: UTF-8, LF endings, trimmed trailing whitespace, final newline.
- Prefer four-space indentation, `PascalCase` for types, `camelCase` for locals/fields, and interface names with an `I` prefix.
- Nullable reference types are enabled; treat warnings as build blockers.
- Run `dotnet format LinkRouter.sln` (with the .NET SDK) before submitting to enforce analyzer and formatting rules.

## Testing Guidelines
- Use xUnit with the custom `[AvaloniaFact]` attribute for UI-facing tests; ensure `TestAppHost.EnsureLifetime()` is called before touching Avalonia UI types.
- Name test classes after the feature under test (e.g., `RuleEditorDialogTests`) and test methods in PascalCase with scenario-focused suffixes.
- Keep UI assertions on the dispatcher thread via `Dispatcher.UIThread.Invoke` as seen in existing tests.
- Honor the shared `tests.runsettings` (30 s session timeout) and extend it locally if longer-running tests are required.

## Commit & Pull Request Guidelines
- Follow the existing history’s present-tense, single-line summaries (e.g., `Refine rule editor modal and stabilize tests`); keep the first line under ~72 characters.
- Reference related issues in the body, outline behavioral changes, and attach screenshots or GIFs for UI updates.
- Before opening a PR, run `pwsh ./run-tests.ps1`, note any deviations, and describe testing coverage and risk areas explicitly.

## Environment & Tooling Notes
- `install-winsdk.ps1` installs the Windows 10/11 SDK needed for packaging; run it once on fresh Windows machines.
- Non-Windows builds write intermediates to `/tmp/linkrouter2` (see `Directory.Build.props`); avoid hard-coded absolute paths in scripts or tests.
- If you are running from Codex Web without a windowing environment, wrap GUI tests with `xvfb-run --server-args="-screen 0 1920x1080x24" pwsh ./run-tests.ps1` (or your specific command) to provision a virtual display.
