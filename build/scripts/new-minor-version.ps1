#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Creates a new minor version tag.

.DESCRIPTION
    This script calculates the next minor version based on GitVersion and creates a git tag.
    Minor versions are for new features that are backward compatible.
    Example: 0.1.5 -> 0.2.0

.PARAMETER Message
    Optional message for the git tag annotation.

.PARAMETER DryRun
    If specified, shows what would be done without creating the tag.

.EXAMPLE
    .\new-minor-version.ps1
    Creates a new minor version tag.

.EXAMPLE
    .\new-minor-version.ps1 -Message "Added new routing features"
    Creates a new minor version with a custom message.

.EXAMPLE
    .\new-minor-version.ps1 -DryRun
    Shows what tag would be created without actually creating it.
#>

param(
    [string]$Message = "",
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'

# Ensure we're on master branch
$currentBranch = git branch --show-current
if ($currentBranch -ne "master") {
    Write-Error "You must be on the master branch to create a minor version. Current branch: $currentBranch"
    exit 1
}

# Ensure working directory is clean
$status = git status --porcelain
if ($status) {
    Write-Error "Working directory is not clean. Please commit or stash your changes first."
    exit 1
}

# Get current version from GitVersion
Write-Host "Calculating current version..." -ForegroundColor Cyan
$currentMajor = [int](dotnet gitversion /showvariable Major)
$currentMinor = [int](dotnet gitversion /showvariable Minor)
$currentPatch = [int](dotnet gitversion /showvariable Patch)
$currentVersion = "$currentMajor.$currentMinor.$currentPatch"

# Calculate next minor version
$nextMinor = $currentMinor + 1
$nextVersion = "$currentMajor.$nextMinor.0"
$tagName = "v$nextVersion"

Write-Host "`nCurrent Version: $currentVersion" -ForegroundColor Yellow
Write-Host "Next Minor Version: $nextVersion" -ForegroundColor Green
Write-Host "Tag Name: $tagName" -ForegroundColor Green

if ($DryRun) {
    Write-Host "`n[DRY RUN] Would create tag: $tagName" -ForegroundColor Magenta
    if ($Message) {
        Write-Host "[DRY RUN] With message: $Message" -ForegroundColor Magenta
    }
    Write-Host "[DRY RUN] No changes made." -ForegroundColor Magenta
    exit 0
}

# Confirm with user
Write-Host "`nThis will create a new minor version tag." -ForegroundColor Yellow
$confirmation = Read-Host "Continue? (y/N)"
if ($confirmation -ne 'y' -and $confirmation -ne 'Y') {
    Write-Host "Cancelled." -ForegroundColor Red
    exit 0
}

# Create the tag
Write-Host "`nCreating tag $tagName..." -ForegroundColor Cyan
if ($Message) {
    git tag -a $tagName -m $Message
} else {
    git tag -a $tagName -m "Release version $nextVersion"
}

if ($LASTEXITCODE -eq 0) {
    Write-Host "Tag created successfully!" -ForegroundColor Green
    Write-Host "`nTo push the tag to remote, run:" -ForegroundColor Yellow
    Write-Host "  git push origin $tagName" -ForegroundColor White
    Write-Host "`nOr to push all tags:" -ForegroundColor Yellow
    Write-Host "  git push --tags" -ForegroundColor White
} else {
    Write-Error "Failed to create tag."
    exit 1
}
