# ThemeTrayApp (Windows 11 Theme Toggle Tray Utility)

A minimal, production-grade Windows 11 tray utility built with C# / .NET 8.

- Left click: instant Light/Dark toggle
- Right click menu: `Toggle Theme`, `Light Mode`, `Dark Mode`, `Start with Windows`, `Exit`
- Global hotkey: `Ctrl + Alt + T`
- Single instance
- Runs silently in tray (no main window)

## Setup (Quick Start)

### 1. Requirements
- Windows 11
- .NET 8 SDK

### 2. Clone the repository
```powershell
git clone <repo-url>
cd ThemeAppW11\ThemeTrayApp
```

### 3. Build
```powershell
dotnet restore
dotnet build -c Release
```

### 4. Run
```powershell
dotnet run -c Release
```

You will see the tray icon in the Windows taskbar notification area.

## Usage (Immediate)

- **Left click (tray icon):** Toggle theme (`Light <-> Dark`)
- **Right click (tray icon):**
  - `Toggle Theme`
  - `Light Mode`
  - `Dark Mode`
  - `Start with Windows` (on/off)
  - `Exit`
- **Global hotkey:** `Ctrl + Alt + T`

## Single-File EXE Publish

This project is already configured for self-contained single-file `win-x64` publish.

```powershell
cd ThemeAppW11\ThemeTrayApp
dotnet publish -c Release
```

Output:

`bin\Release\net8.0-windows\win-x64\publish\ThemeTrayApp.exe`

## Features

- WinForms + `ApplicationContext` architecture, no main window
- Reads/writes Windows theme via Registry:
  - `HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize`
  - `AppsUseLightTheme`
  - `SystemUsesLightTheme`
- Sends Windows broadcast signals after theme change for immediate UI refresh
- `Start with Windows` handled via Registry Run key
- Icon switches by active mode
  - Light: sun-like icon
  - Dark: crescent moon emoji (`??`) rendering, with fallback icon if emoji rendering is unavailable
- Safe global hotkey registration/unregistration
- Robust error handling (no silent crash; tray balloon notifications on failures)

## Project Structure

```text
ThemeTrayApp/
+� ThemeTrayApp.csproj
+� Program.cs
+� Services/
-  +� ThemeMode.cs
-  +� NativeMethods.cs
-  +� ThemeService.cs
-  +� StartupService.cs
-  +� HotkeyService.cs
-  L� IconService.cs
L� UI/
   L� TrayApplicationContext.cs
```

## Architecture Summary

- `Program.cs`
  - Single-instance mutex check
  - Service initialization
  - Starts `Application.Run(ApplicationContext)`
- `TrayApplicationContext`
  - Tray icon, context menu, click behavior
  - App lifecycle and shutdown cleanup
- `ThemeService`
  - Theme read/write
  - Broadcast for immediate refresh
- `StartupService`
  - Add/remove app entry under `HKCU\...\Run`
- `HotkeyService`
  - Global hotkey (`Ctrl + Alt + T`)
- `IconService`
  - Light/Dark icon generation with fallback

## Security and Stability Notes

- No external package dependencies (minimal attack surface)
- No network calls
- User-scope Registry writes only (`HKCU`)
- No admin rights required
- If the global hotkey is already occupied, app continues running and shows a notification

## GitHub Publishing Recommendation

Commit source code only.

Keep build artifacts out of Git with `.gitignore`:
- `bin/`
- `obj/`

If you want to distribute the built EXE, upload it as a **GitHub Release asset** instead of committing binaries to the repository.

## Troubleshooting

### Hotkey does not work
- `Ctrl + Alt + T` may already be in use by another app.
- The app will display a tray notification.

### Start with Windows is enabled but app does not launch
- The executable path may have changed.
- Toggle `Start with Windows` off/on from the tray menu.

### Theme changed but some UI areas update later
- Some Windows components refresh asynchronously.
- The app already sends the required broadcast signals.

## Development Commands

```powershell
# Debug
dotnet build

# Release
dotnet build -c Release

# Publish
dotnet publish -c Release
```

## License

Add your preferred license (for example, MIT). If no `LICENSE` file exists yet, it is recommended to add one.
