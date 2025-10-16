# LinkRouter Settings App — Windows 11 UI Plan

This document outlines a modern Windows 11–style settings companion for LinkRouter. It keeps the existing router CLI intact and adds a graphical app to edit configuration, manage defaults, and test rules.

## Goals
- Native Windows 11 look and feel (Mica, rounded corners, NavigationView, Settings cards).
- Safe, validated editing of the same JSON schema used by the CLI.
- No admin required; per-user registration using existing registrar code.
- Non-destructive saves with automatic backups and atomic writes.

## Tech Stack
- UI framework: WinUI 3 (Windows App SDK), unpackaged desktop app.
- .NET: Target .NET 8 for the settings app for broad SDK support. Keep the CLI on .NET 9/NativeAOT as-is.
- MVVM: CommunityToolkit.Mvvm for observable view models, commands, and validation.
- Optional refactor: Extract shared logic (`ConfigLoader`, `RuleMatcher`, `ProfileResolver`, models) into a `LinkRouter.Core` class library referenced by both CLI and Settings.

## Projects & Structure
- Solution: LinkRouter.sln
  - LinkRouter (existing CLI)
  - LinkRouter.Settings (new WinUI 3 app)
  - LinkRouter.Core (optional shared lib)

```
LinkRouter.Settings/
  App.xaml, App.xaml.cs
  MainWindow.xaml, MainWindow.xaml.cs (Mica backdrop shell)
  Views/
    GeneralPage.xaml
    RulesPage.xaml
    ProfilesPage.xaml
    DefaultPage.xaml
    ImportExportPage.xaml
    AdvancedPage.xaml
    AboutPage.xaml
  ViewModels/
    GeneralViewModel.cs
    RulesViewModel.cs
    ProfilesViewModel.cs
    DefaultViewModel.cs
    ImportExportViewModel.cs
    AdvancedViewModel.cs
    AboutViewModel.cs
  Models/ (UI DTOs & validation helpers)
  Services/
    ConfigService.cs (load/save with backups)
    RuleTestService.cs (simulate, compile regex with timeout)
    BrowserDetectionService.cs (read registry, profiles.ini)
  Resources/
    Strings.resw
    Styles.xaml (density, spacing)
```

## Navigation & Shell
- `NavigationView` (left) with items: General, Rules, Browsers & Profiles, Default, Import/Export, Advanced, About.
- `MainWindow` uses Mica backdrop (`MicaBackdrop`) with dynamic theme (Light/Dark) and accent color.
- `Frame` for page navigation, `BreadcrumbBar` for sub-navigation where needed.

## Data Model Mapping
- Reuse existing config schema as parsed by `LinkRouter/ConfigLoader.cs`:
  - `rules: Rule[]`
  - `default: Rule?`
  - `profiles: Dictionary<string, Profile>?`
- Save and load path: prefer `%AppData%\LinkRouter\mappings.json` (create folder if missing). If a config exists next to the CLI exe, show a notice and a one-click “Copy to AppData” action.
- Atomic saves: write to temp file, move current to `backups/` with timestamp, then replace.

## Pages & Features

### General
- Status card: current config file path, last modified time, backup count.
- Default Apps card:
  - “Register LinkRouter” → calls `DefaultAppRegistrar.RegisterPerUser()` (opens Windows Settings for user confirmation).
  - “Unregister” → `DefaultAppRegistrar.UnregisterPerUser()`.
- Test URL card: textbox + “Simulate”. Shows matching rule, effective browser/profile/args without launching.

### Rules
- Rules list (DataGrid or ListView with `SettingsCard` styling):
  - Columns: Enabled, Type (`domain | regex | contains`), Pattern, Use Profile, Browser, Args Template, Profile, User Data Dir, Working Directory.
  - Drag & drop reordering; Up/Down actions; Enable/Disable toggle.
  - Inline “Test against URL” action.
- Add/Edit rule dialog (`ContentDialog`):
  - Inputs: match type, pattern, useProfile (name from profiles map), browser, argsTemplate, profile, userDataDir, workingDirectory.
  - Live validation: pattern required; regex compilable; after profile resolution, require non-empty `browser` and `argsTemplate`.
  - Live preview: effective browser and final arguments for a sample URL; highlight injected `{url}`, `{profile}`, `{userDataDir}`.

### Browsers & Profiles
- Detected browsers (read registry) with suggested install paths for Chrome, Edge, Firefox; override with file picker.
- Chromium profiles: suggest `--profile-directory` names (Default, Profile 1, …) and `--user-data-dir` path helper.
- Firefox profiles: read `%AppData%\Mozilla\Firefox\profiles.ini` and list `-P` names; show `-no-remote` hint.
- Named reusable Profiles editor (maps to `profiles` in config). Buttons to Test Launch (opens a harmless URL).

### Default
- Configure fallback when no rule matches:
  - Browser path, Args template, Profile, User Data Dir, Working Directory, or Use Profile reference.
- Preview final launch arguments after profile resolution.

### Import / Export
- Import JSON: schema validation + visual diff (added/changed/removed rules and profiles). Options: Merge (by key/name) or Replace.
- Export JSON: save to user-chosen path.
- Backups: view, open folder, restore previous version.

### Advanced
- Open config folder and logs (`%AppData%\LinkRouter`).
- Logging toggle for CLI (minimal; current CLI writes `args.log`).
- Open Windows Default Apps page shortcut.

### About
- App name, version, link to repository, “What’s new” notes, basic privacy note (no telemetry by default).

## Validation & Safety
- Regex compilation with timeout and error surfacing (`InfoBar`).
- File path existence checks with guidance, but allow saving non-existent paths with a warning.
- Prevent save if the effective rule (after profile resolution) lacks `browser` or `argsTemplate`.
- Confirmations for destructive actions (delete rule/profile, replace on import).

## Integration Points
- Registration: reuse `LinkRouter/DefaultAppRegistrar.cs` methods directly.
- Simulation: reuse `LinkRouter/RuleMatcher.cs` and `LinkRouter/ProfileResolver.cs` to match and resolve.
- Config IO: reuse `LinkRouter/ConfigLoader.cs` for load; mirror its schema for save with `System.Text.Json`.
- Optional: move shared code to `LinkRouter.Core` to avoid cross-project internals access.

## UX Details
- Fluent design with Mica backdrop; `SettingsCard` layout for sections.
- Compact spacing that matches Windows 11 Settings.
- Keyboard navigation, high-contrast support, and accessible error messaging.

## Placeholders and Templates
- Supported placeholders in `argsTemplate`:
  - `{url}` (required for launching)
  - `{profile}` (injected if provided)
  - `{userDataDir}` (injected for Chromium when set)
- The app will show a preview of the final command line for the current selection.

## Phased Delivery
1. Shell + General + Default + JSON persistence + backups.
2. Rules list/editor + simulation + validation.
3. Browsers & Profiles detection + Test Launch + named profiles.
4. Import/Export + Advanced + About + a11y polish.
5. Optional: factor shared logic to `LinkRouter.Core`, add unit tests for config roundtrip and rule resolution.

## Implementation Notes
- Use unpackaged WinUI 3 with app-local Windows App SDK to simplify distribution.
- Persist window size/theme; support system theme by default with a manual override setting.
- Keep all file writes in `%AppData%\LinkRouter`; do not write beside the CLI exe unless explicitly requested by the user.
- Guard calls to `Process.Start` with `UseShellExecute = true` only for ms-settings URIs (registration flow); otherwise launch browsers directly (`UseShellExecute = false`).

---

If you want, I can scaffold `LinkRouter.Settings` and wire it to the existing code next.

