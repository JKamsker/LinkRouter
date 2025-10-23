# LinkRouter

[![CI](https://github.com/JKamsker/LinkRouter/actions/workflows/ci.yml/badge.svg)](https://github.com/JKamsker/LinkRouter/actions/workflows/ci.yml)
[![Release](https://img.shields.io/github/v/release/JKamsker/LinkRouter?include_prereleases)](https://github.com/JKamsker/LinkRouter/releases/latest)
[![License](https://img.shields.io/github/license/JKamsker/LinkRouter)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows-blue)]()
[![.NET](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com/)

**LinkRouter** is a lightweight Windows application that intelligently routes URLs to specific browsers and profiles based on customizable rules. Perfect for managing multiple browser contexts, work/personal separation, or testing across different browsers.

## Features

- **Smart URL Routing** - Route URLs to specific browsers based on patterns and rules
- **Profile Support** - Direct links to specific browser profiles (Chrome, Edge, Firefox)
- **Multi-Architecture** - Native support for both x64 and ARM64 Windows devices
- **Minimal Footprint** - Powered by NativeAOT for fast startup and low memory usage
- **Easy Configuration** - Simple JSON-based configuration
- **System Integration** - Set as default browser for seamless URL handling

## Installation

### Recommended: Universal Installer

Download the universal installer that automatically detects your system architecture:

**[Download LinkRouter_Setup.exe](https://github.com/JKamsker/LinkRouter/releases/latest)**

### Alternative Installation Methods

#### MSIX Package (Microsoft Store-style)
- **x64**: `LinkRouter.Settings_{version}_win-x64.msix`
- **ARM64**: `LinkRouter.Settings_{version}_win-arm64.msix`

#### MSI Installer (Traditional)
- **x64**: `LinkRouter_{version}_win-x64.msi`
- **ARM64**: `LinkRouter_{version}_win-arm64.msi`

## Quick Start

1. **Install LinkRouter** using one of the installers above
2. **Launch LinkRouter Settings** from the Start menu
3. **Configure routing rules** in the Settings UI
4. **Set as default browser** (optional) to route all links

## Configuration

LinkRouter uses a simple JSON configuration file located at:
```
%APPDATA%\LinkRouter\.config\mappings.json
```

### Example Configuration

```json
{
  "rules": [
    {
      "pattern": "*.work.com",
      "browser": "chrome",
      "profile": "Work"
    },
    {
      "pattern": "*.github.com",
      "browser": "edge",
      "profile": "Development"
    },
    {
      "pattern": "*.youtube.com",
      "browser": "firefox"
    }
  ]
}
```

## Supported Browsers

- Google Chrome / Chromium
- Microsoft Edge
- Mozilla Firefox

## System Requirements

- **OS**: Windows 10 (19041) or later / Windows 11
- **Architecture**: x64 (Intel/AMD 64-bit) or ARM64
- **.NET**: Not required (self-contained with NativeAOT)

## Building from Source

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Windows 10 SDK (for MSIX packaging)
- [WiX Toolset](https://wixtoolset.org/) v4+ (for MSI packaging)

### Build Commands

```powershell
# Restore dependencies
dotnet restore

# Build the solution
dotnet build -c Release

# Package MSIX (x64)
./build/scripts/package-msix.ps1 -Runtime win-x64 -Version 1.0.0.0

# Package MSI (x64)
./build/scripts/package-msi.ps1 -Runtime win-x64 -Version 1.0.0

# Package universal bundle
./build/scripts/package-bundle.ps1 -Version 1.0.0 -MsiX64Path artifacts/msi/LinkRouter_1.0.0_win-x64.msi -MsiArm64Path artifacts/msi/LinkRouter_1.0.0_win-arm64.msi
```

## Architecture

LinkRouter consists of two main components:

1. **LinkRouter.Settings** - WPF-based configuration UI
2. **LinkRouter.Launcher** - Lightweight NativeAOT launcher for fast URL handling

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built with [.NET 9](https://dotnet.microsoft.com/) and NativeAOT
- Packaged with [WiX Toolset](https://wixtoolset.org/)
- CI/CD powered by [GitHub Actions](https://github.com/features/actions)

---

Made with ❤️ by [JKamsker](https://github.com/JKamsker)
