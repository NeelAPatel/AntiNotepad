# Observation methods for a running Avalonia app

Ordered by cost. Nothing here has been run yet.

| # | Method | Gives | Cannot give |
|---|---|---|---|
| 1 | Avalonia headless platform with Skia renderer | Pixel-accurate frames, baseline comparison, simulated input, full control tree. Needs Skia; the default headless backend draws no pixels. | Real compositor effects (Mica/Acrylic), real display timing. |
| 2 | In-app inspection tools (AvaScope, TomLabs.UiAutomation) | Visual-tree dump, screenshots, injected input. AvaScope targets Avalonia 12 / net10.0. | Not vetted; adds a dependency, so needs the user's OK. |
| 3 | Screen recording of real interaction | Motion, flicker, latency by eye. The only honest check of "feel". | Numbers; pair with frame timing. |
| 4 | Computer-use on the real window | Walkthrough like a person: click, type, hotkey. Closest match to the user's view. | Needs user approval to start; subjective. |

## Knowledge gaps
- Windows UI Automation clients (e.g. FlaUI) and Avalonia automation-peer support were not covered by search. Needed to judge screen-reader experience.

## Rules
- Prefer a method that sees the real window before one that renders a fixture.
- Never claim smoothness from callback timings alone.
- Capturing the client tree excludes DWM shadow, OS snap UI and backdrop material; say so in any finding that depends on them.
