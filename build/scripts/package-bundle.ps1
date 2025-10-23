param(
    [string]$Version = "1.0.0",
    [string]$MsiX64Path = "",
    [string]$MsiArm64Path = "",
    [string]$OutputDirectory = ""
)

set-strictmode -version Latest
$ErrorActionPreference = "Stop"

if (-not $Version -or $Version -notmatch '^\d+\.\d+\.\d+$') {
    throw "Bundle version must include three numeric components (e.g. 1.0.0)."
}

if (-not (Test-Path $MsiX64Path)) {
    throw "x64 MSI not found at: $MsiX64Path"
}

if (-not (Test-Path $MsiArm64Path)) {
    throw "ARM64 MSI not found at: $MsiArm64Path"
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).ProviderPath

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $repoRoot "artifacts/bundle"
} else {
    $OutputDirectory = Join-Path $repoRoot $OutputDirectory
}

if (Test-Path $OutputDirectory) {
    Remove-Item $OutputDirectory -Recurse -Force
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

$wixIntermediateDir = Join-Path $OutputDirectory "wix"
$bundleName = "LinkRouter_${Version}_Setup.exe"
$bundlePath = Join-Path $OutputDirectory $bundleName

New-Item -ItemType Directory -Path $wixIntermediateDir -Force | Out-Null

Write-Host "Restoring WiX CLI via dotnet tool..."
$toolManifest = Join-Path $repoRoot ".config/dotnet-tools.json"
if (-not (Test-Path $toolManifest)) {
    throw "WiX tool manifest missing. Expected at $toolManifest"
}
dotnet tool restore --tool-manifest $toolManifest
if ($LASTEXITCODE -ne 0) {
    throw "dotnet tool restore (WiX) failed."
}

Write-Host "Validating WiX CLI availability..."
$wixVersionOutput = dotnet tool run wix -- --version
if ($LASTEXITCODE -ne 0) {
    throw "WiX CLI failed to execute after dotnet tool restore."
}
$wixVersionMatch = [regex]::Match(($wixVersionOutput | Out-String), '\d+\.\d+\.\d+')
if (-not $wixVersionMatch.Success) {
    throw "Unable to determine WiX version from output: $wixVersionOutput"
}
$wixVersion = $wixVersionMatch.Value

Write-Host "Ensuring WiX Bal extension is available (v$wixVersion)..."
dotnet tool run wix -- extension add "WixToolset.Bal.wixext/$wixVersion" | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw "Failed to add WixToolset.Bal extension."
}

Write-Host "Building Bundle installer with WiX..."
$wixArgs = @(
    "tool",
    "run",
    "wix",
    "build",
    (Join-Path $repoRoot "build/msi/Bundle.wxs"),
    "-ext", "WixToolset.Bal.wixext",
    "-d", "MsiX64Path=$MsiX64Path",
    "-d", "MsiArm64Path=$MsiArm64Path",
    "-d", "ProductVersion=$Version",
    "-out", $bundlePath
)

dotnet @wixArgs
if ($LASTEXITCODE -ne 0) {
    throw "WiX CLI build (bundle) failed."
}

Write-Host "Bundle installer created at $bundlePath"
return $bundlePath
