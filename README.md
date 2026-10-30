# KeepAlive

[![NuGet](https://img.shields.io/nuget/v/QuinntyneBrown.KeepAlive.Cli.svg)](https://www.nuget.org/packages/QuinntyneBrown.KeepAlive.Cli)
[![NuGet Downloads](https://img.shields.io/nuget/dt/QuinntyneBrown.KeepAlive.Cli.svg)](https://www.nuget.org/packages/QuinntyneBrown.KeepAlive.Cli)

A cross-platform CLI tool (Windows and macOS) that prevents the system from sleeping and maintains Microsoft Teams presence.

## How It Works

- **Prevents sleep**
  - Windows: uses the Win32 `SetThreadExecutionState` API to stop the system from idling, sleeping, or turning off the display.
  - macOS: holds an IOKit `PreventUserIdleDisplaySleep` power assertion (visible in `pmset -g assertions`).
- **Maintains Teams presence** — Sends a periodic F15 keypress (`SendInput` on Windows, `CGEventPost` on macOS) to reset the OS idle timer. Teams reads that timer to detect user activity, so this keeps your status as "Available". F15 is unmapped on modern keyboards and won't interfere with your work. On macOS the tool also declares user activity to power management so a dimmed display wakes up.

## Prerequisites

- Windows 10/11 or macOS
- [.NET 9 Runtime](https://dotnet.microsoft.com/download/dotnet/9.0) (or SDK)

> **macOS note:** posting synthetic key events may require your terminal app to be allowed under
> **System Settings → Privacy & Security → Accessibility** (or **Input Monitoring**). Sleep prevention
> works without any permission.

## Installation

```bash
dotnet tool install --global QuinntyneBrown.KeepAlive.Cli
```

### Update

```bash
dotnet tool update --global QuinntyneBrown.KeepAlive.Cli
```

### Uninstall

```bash
dotnet tool uninstall --global QuinntyneBrown.KeepAlive.Cli
```

## Usage

```bash
keepalive
```

### Options

| Option | Alias | Default | Description |
|---|---|---|---|
| `--interval` | `-i` | `30` | Seconds between activity simulations |
| `--help` | `-h` | | Show help |
| `--version` | | | Show version |

### Examples

```bash
# Run with default 30-second interval
keepalive

# Run with a 60-second interval
keepalive -i 60
```

Press `Ctrl+C` to stop. Normal power behaviour is restored on exit.

## Configuration

Default settings can be changed in `appsettings.json` alongside the tool binary:

```json
{
  "KeepAlive": {
    "IntervalSeconds": 30
  }
}
```

Command-line options override `appsettings.json` values.

## Building from Source

```bash
git clone https://github.com/QuinntyneBrown/KeepAlive.git
cd KeepAlive
dotnet build
```

### Install from local build

```bash
dotnet pack
dotnet tool install --global --add-source src/KeepAlive.Cli/nupkg QuinntyneBrown.KeepAlive.Cli
```

## Project Structure

```
KeepAlive.sln
src/
  KeepAlive.Cli/
    Program.cs            # Entry point, System.CommandLine setup, DI wiring
    KeepAliveWorker.cs    # BackgroundService — sleep prevention and input simulation loop
    KeepAliveOptions.cs   # Configuration POCO
    appsettings.json      # Default configuration
    Platforms/
      IPlatformKeepAlive.cs         # OS abstraction used by the worker
      PlatformKeepAliveFactory.cs   # Picks the implementation for the current OS
      WindowsKeepAlive.cs           # SetThreadExecutionState + SendInput
      WindowsNativeMethods.cs       # Win32 P/Invoke declarations
      MacOSKeepAlive.cs             # IOKit power assertion + CGEvent key press
      MacOSNativeMethods.cs         # IOKit / CoreGraphics P/Invoke declarations
```

## License

[MIT](LICENSE)
