# Calculator

A tiny Fluent Design calculator for your desktop.

Calculator is a simple calculator app designed to save desktop space. Instead of a large number pad, it stays as small as possible — just an expression input and a live result box. The result is a small rectangular window that always shows both what you just typed and the answer, so you always know exactly what you're calculating and never get surprised by an accidental typo the way you can with the classic Windows Calculator.

## Features

- **Compact, resizable window** — borderless design with just an expression field and a bold result label. Default **440×320**, minimum **200×120**, up to your screen's working area. Drag the title bar to move, drag any edge or corner to resize. Size, position and state (normal/minimized) are restored on the next launch.
- **Live result** — the result updates as you type, with thousands separators (e.g. `= 1,234,567`) and scientific notation for very large or small numbers.
- **Smart input display** — `*` and `/` are automatically converted to `×` and `÷` as you type, and all operators (`+ - × ÷ %`) are highlighted in red so the expression is easy to read.
- **Clear feedback** — invalid input like division by zero shows `Error` in red; an incomplete expression (trailing operator or unclosed parenthesis) keeps the last valid result on screen.
- **Pin on top** — keep the calculator above all other windows with one click (the pin button is highlighted while active).
- **Adjustable font sizes** — the settings panel (⚙) lets you change the expression font size (**default 10 pt**, range 10–72) and the result font size (**default 16 pt bold**, range 16–96), in steps of 2, applied live.
- **Settings auto-save** — font sizes, window size, position and state are remembered between sessions (stored in `%LocalAppData%\Calculator\calculator.dat`).
- **Custom Fluent-style UI** — Segoe UI, a minimal custom title bar (pin ⦿, minimize `_`, close ✕), flat controls with hover/pressed states, and a clean white background.
- **Keyboard friendly** — `Esc` clears the input; `Enter` is disabled so the expression can't wrap to a new line.

## Calculator syntax

Type a normal expression with `+`, `-`, `*` (shown as ×), `/` (shown as ÷), `%` (modulo) and parentheses, e.g. `(12 + 34) * 5`. Decimal numbers are supported. Evaluation is left-to-right, so `2 + 3 * 4` = 20.

## Download & portable use

Calculator is fully portable — no installation needed.

1. Download `Calculator.exe` from the [Releases page](https://github.com/ngocthanhgl/Calculator/releases).
2. Double-click it anywhere — it runs straight away from any folder or USB stick.
3. Settings are saved to `%LocalAppData%\Calculator\calculator.dat`, not next to the exe, so the app stays portable.

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
