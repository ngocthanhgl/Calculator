# Calculator

A tiny Fluent Design calculator for your desktop.

Calculator is a simple calculator app designed to save desktop space. Instead of a large number pad, it stays as small as possible — just an expression input field and a live result box. The result is a small rectangular window that always shows both what you just typed and the answer, so you always know exactly what you're calculating and never get surprised by an accidental typo the way you can with the classic Windows Calculator.

## Features

- **Compact window** — only the expression input and result box. Default 440×320, resizable down to 200×120, minimum footprint on your desktop.
- **Live expression display** — see exactly what you typed before the result is computed.
- **Pin on top** — keep the calculator above all other windows with one click.
- **Adjustable font sizes** — change the expression font (10–72) and result font (16–96) to suit your screen.
- **Fluent Design style** — Segoe UI, flat controls with hover/pressed states, and a custom title bar.
- **Settings auto-save** — font sizes and window size/position are remembered between sessions.
- **Portable or installed** — run the single exe anywhere, or use the installer.

## Download

Grab the latest release from the [Releases page](https://github.com/ngocthanhgl/Calculator/releases) — the `Calculator.exe` asset is a portable build.

## Installer

`InstallCalculator.exe` installs the app to `%LocalAppData%\Calculator` and copies a shortcut to your Desktop. It stops any running instance before installing.

## Building from source

Requires the .NET Framework 4.x C# compiler (`csc.exe`, ships with the .NET Framework SDK or Visual Studio).

```powershell
./build.ps1
```

This produces `Calculator.exe` in the project folder.

### GitHub Actions

The repository includes an automated release workflow (`.github/workflows/release.yml`). It builds the exe on a Windows runner and publishes it as a GitHub Release. Trigger it from the **Actions** tab (manual run with a version input) or by pushing a `v*` tag.

## Requirements

- Windows (built against .NET Framework 4.x)
- C# / Windows Forms
