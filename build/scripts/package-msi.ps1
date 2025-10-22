param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$Version = "1.0.0",
    [string]$OutputDirectory = ""
)

set-strictmode -version Latest
$ErrorActionPreference = "Stop"

if (-not $Version -or $Version -notmatch '^\d+\.\d+\.\d+$') {
    throw "MSI version must include three numeric components (e.g. 1.0.0)."
}

$versionParts = $Version.Split(".")
$major = [int]$versionParts[0]
$minor = [int]$versionParts[1]
$build = [int]$versionParts[2]

if ($major -gt 255 -or $minor -gt 255 -or $build -gt 65535) {
    throw "MSI version components exceed Windows Installer limits (major/minor <= 255, build <= 65535)."
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).ProviderPath

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $repoRoot "artifacts/msi"
} else {
    $OutputDirectory = Join-Path $repoRoot $OutputDirectory
}

if (Test-Path $OutputDirectory) {
    Remove-Item $OutputDirectory -Recurse -Force
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

$publishDir = Join-Path $OutputDirectory "bin"
$configDir = Join-Path $OutputDirectory ".config"
$launcherPublishDir = Join-Path $OutputDirectory "launcher"
$wixIntermediateDir = Join-Path $OutputDirectory "wix"
$msiName = "LinkRouter_${Version}_${Runtime}.msi"
$msiPath = Join-Path $OutputDirectory $msiName

New-Item -ItemType Directory -Path $publishDir,$launcherPublishDir,$wixIntermediateDir,$configDir -Force | Out-Null

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

# Drop the NativeAOT launcher into the published application so the settings UI can launch routes.
$launcherBinary = Join-Path $launcherPublishDir "LinkRouter.Launcher.exe"
if (-not (Test-Path $launcherBinary)) {
    throw "NativeAOT launcher binary not found at $launcherBinary"
}
Copy-Item $launcherBinary -Destination (Join-Path $publishDir "LinkRouter.Launcher.exe") -Force

# Seed default configuration assets into the new .config directory.
$defaultMappingsSource = Join-Path $repoRoot "src/LinkRouter.Launcher/mappings.json"
if (Test-Path $defaultMappingsSource) {
    Copy-Item $defaultMappingsSource -Destination (Join-Path $configDir "mappings.json") -Force
} else {
    Write-Warning "Default mappings.json not found at $defaultMappingsSource; MSI will not include starter config."
}

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

Write-Host "Ensuring WiX Util extension is available (v$wixVersion)..."
dotnet tool run wix -- extension add "WixToolset.Util.wixext/$wixVersion" | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw "Failed to add WixToolset.Util extension."
}

function New-SafeId {
    param(
        [string]$Prefix,
        [string]$RelativePath,
        [System.Collections.Generic.HashSet[string]]$ExistingIds
    )

    $baseId = [System.Text.RegularExpressions.Regex]::Replace($RelativePath, '[^A-Za-z0-9]', '_').Trim('_')
    if ([string]::IsNullOrWhiteSpace($baseId)) {
        $baseId = "Payload"
    }

    $candidate = "${Prefix}_$baseId"
    $suffix = 1
    while (-not $ExistingIds.Add($candidate)) {
        $candidate = "${Prefix}_${baseId}_$suffix"
        $suffix++
    }

    return $candidate
}

function Get-DeterministicGuid {
    param([string]$Seed)

    $md5 = [System.Security.Cryptography.MD5]::Create()
    try {
        $hash = $md5.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($Seed))
    } finally {
        $md5.Dispose()
    }

    return ([Guid]::new($hash)).ToString().ToUpperInvariant()
}

function Escape-Xml {
    param([string]$Value)
    return [System.Security.SecurityElement]::Escape($Value)
}

Write-Host "Generating WiX component fragment..."
$layoutTargets = @(
    [PSCustomObject]@{
        Path = $publishDir
        DirectoryId = "INSTALLBIN"
        VariableName = "BinDir"
    },
    [PSCustomObject]@{
        Path = $configDir
        DirectoryId = "INSTALLCONFIG"
        VariableName = "ConfigDir"
    }
)
$componentIds = [System.Collections.Generic.List[string]]::new()
$usedIds = [System.Collections.Generic.HashSet[string]]::new()
$componentsByDirectory = [System.Collections.Generic.Dictionary[string, System.Collections.Generic.List[string]]]::new()

foreach ($target in $layoutTargets) {
    if (-not (Test-Path $target.Path)) {
        continue
    }

    $baseLength = $target.Path.Length
    Get-ChildItem $target.Path -Recurse -File | Sort-Object FullName | ForEach-Object {
        $relativePath = $_.FullName.Substring($baseLength + 1)
        $componentId = New-SafeId -Prefix "Component" -RelativePath $relativePath -ExistingIds $usedIds
        $fileId = New-SafeId -Prefix "File" -RelativePath $relativePath -ExistingIds $usedIds
        $componentGuid = Get-DeterministicGuid -Seed "LinkRouter.Settings::$($target.DirectoryId)::${relativePath}"
        $sourcePath = "__$($target.VariableName)__\$relativePath"
        $escapedSource = Escape-Xml $sourcePath

        if (-not $componentsByDirectory.ContainsKey($target.DirectoryId)) {
            $componentsByDirectory[$target.DirectoryId] = [System.Collections.Generic.List[string]]::new()
        }

        $componentsByDirectory[$target.DirectoryId].Add("      <Component Id=""$componentId"" Guid=""{$componentGuid}"">")
        $componentsByDirectory[$target.DirectoryId].Add("        <File Id=""$fileId"" Source=""$escapedSource"" KeyPath=""yes"" />")
        $componentsByDirectory[$target.DirectoryId].Add('      </Component>')

        $componentIds.Add($componentId)
    }
}

$sb = [System.Text.StringBuilder]::new()
[void]$sb.AppendLine('<?xml version="1.0" encoding="utf-8"?>')
[void]$sb.AppendLine('<Wix xmlns="http://wixtoolset.org/schemas/v4/wxs">')
[void]$sb.AppendLine('  <Fragment>')

foreach ($dirId in ($componentsByDirectory.Keys | Sort-Object)) {
    [void]$sb.AppendLine("    <DirectoryRef Id=""$dirId"">")
    foreach ($line in $componentsByDirectory[$dirId]) {
        [void]$sb.AppendLine($line)
    }
    [void]$sb.AppendLine('    </DirectoryRef>')
}

[void]$sb.AppendLine('  </Fragment>')
[void]$sb.AppendLine('  <Fragment>')
[void]$sb.AppendLine('    <ComponentGroup Id="ProductComponents">')
$componentIds | ForEach-Object {
    [void]$sb.AppendLine("      <ComponentRef Id=""$_"" />")
}
[void]$sb.AppendLine('    </ComponentGroup>')
[void]$sb.AppendLine('  </Fragment>')
[void]$sb.AppendLine('</Wix>')

$componentFragmentPath = Join-Path $wixIntermediateDir "Components.wxs"
$componentsXml = $sb.ToString()
foreach ($target in $layoutTargets) {
    $placeholder = "__$($target.VariableName)__"
    $binderVariable = '$' + "(var.$($target.VariableName))"
    $componentsXml = $componentsXml.Replace($placeholder, $binderVariable)
}
Set-Content -Path $componentFragmentPath -Value $componentsXml -Encoding UTF8

Write-Host "Building MSI package with WiX..."
$wixArgs = @(
    "tool",
    "run",
    "wix",
    "build",
    (Join-Path $repoRoot "build/msi/Product.wxs"),
    $componentFragmentPath,
    "-ext", "WixToolset.Util.wixext",
    "-arch", "x64",
    "-d", "BinDir=$publishDir",
    "-d", "ConfigDir=$configDir",
    "-d", "ProductVersion=$Version",
    "-out", $msiPath
)

dotnet @wixArgs
if ($LASTEXITCODE -ne 0) {
    throw "WiX CLI build failed."
}

Write-Host "MSI package created at $msiPath"
return $msiPath
