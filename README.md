# KeepAlive

A Windows CLI tool that prevents the system from sleeping and maintains Microsoft Teams presence.

## How It Works

- **Prevents sleep** — Uses the Win32 `SetThreadExecutionState` API to stop Windows from idling, sleeping, or turning off the display.
- **Maintains Teams presence** — Sends a periodic F15 keypress via `SendInput` to reset the OS idle timer. Teams uses `GetLastInputInfo` to detect user activity, so this keeps your status as "Available". F15 is unmapped on modern keyboards and won't interfere with your work.

## Prerequisites

- Windows 10/11
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

## Installation

### As a global .NET tool

```bash
dotnet tool install --global QuinntyneBrown.KeepAlive.Cli
```

### From local source

```bash
dotnet pack
dotnet tool install --global --add-source src/KeepAlive.Cli/nupkg QuinntyneBrown.KeepAlive.Cli
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

## Build

```bash
dotnet build
```

## Project Structure

```
KeepAlive.sln
src/
  KeepAlive.Cli/
    Program.cs            # Entry point, System.CommandLine setup, DI wiring
    KeepAliveWorker.cs    # BackgroundService — sleep prevention and input simulation
    KeepAliveOptions.cs   # Configuration POCO
    NativeMethods.cs      # Win32 P/Invoke declarations
    appsettings.json      # Default configuration
```
