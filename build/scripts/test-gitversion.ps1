#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Dry-run GitVersion locally to test versioning configuration.

.DESCRIPTION
    This script runs GitVersion in dry-run mode to show what version would be calculated
    without actually making any changes. Useful for testing GitVersion.yml configuration.

.PARAMETER ShowConfig
    If specified, also displays the effective GitVersion configuration.

.EXAMPLE
    .\test-gitversion.ps1
    Shows the calculated version information.

.EXAMPLE
    .\test-gitversion.ps1 -ShowConfig
    Shows both the configuration and calculated version.
#>

param(
    [switch]$ShowConfig
)

$ErrorActionPreference = 'Stop'

# Check if GitVersion is installed
$gitVersionPath = Get-Command dotnet-gitversion -ErrorAction SilentlyContinue
if (-not $gitVersionPath) {
    Write-Host "GitVersion tool not found. Installing..." -ForegroundColor Yellow
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to restore .NET tools. Please run 'dotnet tool restore' manually."
        exit 1
    }
}

Write-Host "`n=== GitVersion Dry-Run ===" -ForegroundColor Cyan
Write-Host "Repository: $PWD" -ForegroundColor Gray
Write-Host "Current Branch: " -NoNewline -ForegroundColor Gray
git branch --show-current
Write-Host ""

if ($ShowConfig) {
    Write-Host "--- GitVersion Configuration ---" -ForegroundColor Yellow
    dotnet gitversion /showconfig
    Write-Host ""
}

Write-Host "--- Calculated Version ---" -ForegroundColor Green
dotnet gitversion /showvariable FullSemVer
$fullVersion = dotnet gitversion /showvariable FullSemVer

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nVersion Details:" -ForegroundColor Cyan
    Write-Host "  MajorMinorPatch: $(dotnet gitversion /showvariable MajorMinorPatch)" -ForegroundColor White
    Write-Host "  SemVer:          $(dotnet gitversion /showvariable SemVer)" -ForegroundColor White
    Write-Host "  FullSemVer:      $(dotnet gitversion /showvariable FullSemVer)" -ForegroundColor White
    Write-Host "  InformationalVersion: $(dotnet gitversion /showvariable InformationalVersion)" -ForegroundColor White
    Write-Host "  BranchName:      $(dotnet gitversion /showvariable BranchName)" -ForegroundColor White
    Write-Host "  Sha:             $(dotnet gitversion /showvariable Sha)" -ForegroundColor White
    Write-Host ""

    Write-Host "--- Full JSON Output ---" -ForegroundColor Yellow
    dotnet gitversion
} else {
    Write-Error "GitVersion failed to calculate version. Check your GitVersion.yml configuration."
    exit 1
}

Write-Host "`n=== Dry-Run Complete ===" -ForegroundColor Cyan
Write-Host "No changes were made to your repository." -ForegroundColor Gray
