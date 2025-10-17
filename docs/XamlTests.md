# XAML Validation Tests

## Summary
Automated tests have been added to catch XAML errors before runtime.

## What Was Fixed
1. **Invalid Icon Names** - Replaced `Icon="Up"` and `Icon="Down"` with proper `FontIcon` glyphs
2. **Invalid Theme Resources** - Replaced `TextFillColorCriticalBrush` with `SystemFillColorCriticalBrush`

## Files Changed
- `Views/RulesPage.xaml` - Fixed up/down icons and critical color brush
- `Views/ProfilesPage.xaml` - Fixed critical color brush  
- `Views/ImportExportPage.xaml` - Fixed critical color brush
- `Views/DefaultPage.xaml` - Fixed critical color brush
- `Views/AdvancedPage.xaml` - Fixed critical color brush
- `Views/GeneralPage.xaml` - No changes needed (using valid resources)

## Test Project Added
**Location**: `LinkRouter.Settings.Tests/`

**Test File**: `XamlPageTests.cs`

**What it tests**:
- All 7 pages can be instantiated without XAML parse errors
- SettingsCard custom control can be instantiated
- Catches issues like:
  - Invalid Icon names
  - Missing theme resources
  - Malformed XAML markup

## Running Tests

```powershell
# Run all tests
dotnet test LinkRouter.sln

# Run only XAML tests
dotnet test LinkRouter.Settings.Tests/LinkRouter.Settings.Tests.csproj

# Run in watch mode (auto-rerun on changes)
dotnet watch test --project LinkRouter.Settings.Tests/LinkRouter.Settings.Tests.csproj
```

## CI/CD Integration
Add this to your GitHub Actions or Azure DevOps pipeline:

```yaml
- name: Run XAML validation tests
  run: dotnet test LinkRouter.Settings.Tests/LinkRouter.Settings.Tests.csproj --logger "trx;LogFileName=test-results.trx"
```

## Why These Tests Help
- **Catches errors at build time** instead of runtime
- **Prevents broken tabs** from reaching production
- **Fast feedback** - runs in seconds
- **No manual testing** needed for basic XAML validation

## Future Improvements
1. Add tests for ViewModel instantiation
2. Test x:Bind compilations
3. Validate all theme resource references
4. Check for common XAML anti-patterns
