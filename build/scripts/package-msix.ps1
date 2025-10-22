param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$Version = "1.0.0.0",
    [string]$OutputDirectory = ""
)

set-strictmode -version Latest
$ErrorActionPreference = "Stop"

if (-not $Version -or $Version -notmatch '^\d+\.\d+\.\d+\.\d+$') {
    throw "MSIX version must include four numeric components (e.g. 1.0.0.0)."
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).ProviderPath

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $repoRoot "artifacts/msix"
} else {
    $OutputDirectory = Join-Path $repoRoot $OutputDirectory
}

if (Test-Path $OutputDirectory) {
    Remove-Item $OutputDirectory -Recurse -Force
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

$publishDir = Join-Path $OutputDirectory "publish"
$layoutDir = Join-Path $OutputDirectory "layout"
$assetsDir = Join-Path $layoutDir "Assets"
$launcherPublishDir = Join-Path $OutputDirectory "launcher"
$msixName = "LinkRouter.Settings_${Version}_${Runtime}.msix"
$msixPath = Join-Path $OutputDirectory $msixName

New-Item -ItemType Directory -Path $publishDir,$layoutDir,$assetsDir,$launcherPublishDir -Force | Out-Null

Write-Host "Restoring solution ($Runtime)..."
dotnet restore "$repoRoot/LinkRouter.sln" -r $Runtime
if ($LASTEXITCODE -ne 0) {
    throw "dotnet restore failed."
}

Write-Host "Publishing LinkRouter.Settings ($Configuration | $Runtime)..."
dotnet publish "$repoRoot/src/LinkRouter.Settings/LinkRouter.Settings.csproj" `
    -c $Configuration `
    -r $Runtime `
    --self-contained `
    --no-restore `
    -p:Version=$Version `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $publishDir
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed."
}

Write-Host "Publishing LinkRouter.Launcher as NativeAOT ($Configuration | $Runtime)..."
dotnet publish "$repoRoot/src/LinkRouter.Launcher/LinkRouter.Launcher.csproj" `
    -c $Configuration `
    -r $Runtime `
    --self-contained `
    -p:Version=$Version `
    -p:PublishAot=true `
    -p:StripSymbols=true `
    -o $launcherPublishDir
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish (launcher native AOT) failed."
}

Write-Host "Preparing MSIX layout..."
Get-ChildItem $publishDir -Recurse | ForEach-Object {
    $targetPath = $_.FullName.Replace($publishDir, $layoutDir)
    if ($_.PSIsContainer) {
        if (-not (Test-Path $targetPath)) {
            New-Item -ItemType Directory -Path $targetPath -Force | Out-Null
        }
    } else {
        Copy-Item $_.FullName -Destination $targetPath -Force
    }
}

# Drop the NativeAOT launcher into the application root
$launcherBinary = Join-Path $launcherPublishDir "LinkRouter.Launcher.exe"
if (-not (Test-Path $launcherBinary)) {
    throw "NativeAOT launcher binary not found at $launcherBinary"
}
Copy-Item $launcherBinary -Destination (Join-Path $layoutDir "LinkRouter.Launcher.exe") -Force

$assetsSource = Join-Path $PSScriptRoot "windows/assets"
Copy-Item -Path (Join-Path $assetsSource "*") -Destination $assetsDir -Recurse -Force

$manifestTemplate = Get-Content (Join-Path $PSScriptRoot "windows/AppxManifest.template.xml") -Raw
$manifestContent = $manifestTemplate -replace "{{Version}}", $Version
Set-Content -Path (Join-Path $layoutDir "AppxManifest.xml") -Value $manifestContent -Encoding UTF8

Write-Host "Locating makeappx.exe..."
$makeAppx = Get-ChildItem "${env:ProgramFiles(x86)}\Windows Kits\10\bin" -Recurse -Filter makeappx.exe `
    | Where-Object { $_.FullName -match '\\x64\\' } `
    | Sort-Object { [version]($_.VersionInfo.ProductVersion) } -Descending `
    | Select-Object -First 1

if (-not $makeAppx) {
    $makeAppx = Get-ChildItem "${env:ProgramFiles(x86)}\Windows Kits\10\bin" -Recurse -Filter makeappx.exe `
        | Sort-Object { [version]($_.VersionInfo.ProductVersion) } -Descending `
        | Select-Object -First 1
}

if (-not $makeAppx) {
    throw "Unable to find makeappx.exe. Install the Windows 10/11 SDK."
}

Write-Host "Packing MSIX using $($makeAppx.FullName)..."
& $makeAppx.FullName pack /o /d $layoutDir /p $msixPath | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw "makeappx pack failed."
}

Write-Host "MSIX package created at $msixPath"
return $msixPath
