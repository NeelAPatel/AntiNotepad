# A44-platform-seam-impact: READ-ONLY impact analysis

Role: director (gpt-6.1-sol medium; may delegate reading to luna subagents). Branch `reorg/A44-platform-seam` (this worktree).
Product: `product/AntiNotepad`. Read `AGENTS.md` first. You are READ-ONLY: do not edit, create or delete any file except
your report `.coord/reports/A44-platform-seam-impact.md`. Do not build, launch or kill AntiNotepad.

## Context
The A38-mvvm-audit (`git show feat/A38-mvvm-audit:docs/A38-mvvm-audit.md`, use `MSYS_NO_PATHCONV=1` in Git Bash) chose
behavior-neutral modularity refactors over MVVM. Task `A44-platform-seam` routes Win32/OS access from `UI` and `Editor` through
a small `Platform` facade, so OS details live in one place and a Win32 change cannot ripple into UI. Behavior-neutral; window
frame styles must not change (A32/A34 broke corners/shadow/bars and were reverted). Sibling tasks in flight on other branches:
A39-motion-out-of-ui (adds a table-driven `LAYERS` selftest of forbidden `from -> to` namespace rules, scanning IL),
A40-settings-change-event, A41-padwindow-split (moves code out of `PadWindow`), A42-padview-commands, A43-app-context.
Assume A39-motion-out-of-ui lands first. Known starting points (verify, find all): `Native.*` in `UI/PadWindow.cs`
(`ListenClipboard`, `CursorPosition`, `TrimWorkingSet`), `UI/PadWindow.Keys.cs`, `UI/PadWindow.Layout.cs`,
`UI/PadWindow.Navigation.cs` (`ClipboardIsPrivate`, `Chime`, `StartScreenSnip`), `UI/PadMenu.cs` and `UI/PadWindow.TopBlur.cs`
(`ConfigurePopupWindow`), `Editor/Elements.cs` (`OpenUrl`), plus `TaskbarPresence`, `GlobalHotkey`, `Tray`, `SingleInstance`,
`SessionEnd`, `Ocr`, `Startup`, `NotesLocation` use from outside `Platform`.

## Work
1. Inventory every P/Invoke / `DllImport` / `LibraryImport` / `Marshal`/`IntPtr`/`TryGetPlatformHandle`/`WndProc`/
   `Win32Properties`/`DwmSetWindowAttribute`-style OS touchpoint **anywhere** in `product/AntiNotepad` (also `Program.cs`,
   `Model/`, `Theme/`, `Calc/`), and every `Platform` type (public surface). Note which are declared outside `Platform`.
2. Inventory every call or reference from OUTSIDE `Platform` (UI, Editor, Model, Program.cs, SelfTest.cs) into `Platform` types.
3. For each: file:line, what it does, thread (UI thread / background / timer), HWND or handle use and lifetime, and timing
   sensitivity (show/hide path, activation/focus, taskbar stand-in A22-A25, top-bar blur popup, OCR/screen snip, hotkey
   registration, clipboard listener lifetime, working-set trim after hide).
4. Decide for each whether it can move behind a facade unchanged, or must stay as is (hot path, handle lifetime, ordering
   with Avalonia calls), with the reason. Look for hidden ordering dependencies a mechanical wrap could break.

## Report: `.coord/reports/A44-platform-seam-impact.md`
Sections: (a) inventory table (columns: location, member, purpose, thread, handle, timing sensitivity, can-move?);
(b) proposed facade shape: exact type name(s) and member signatures, grouped by intent (cursor, clipboard, sound, memory,
window styling, hotkeys, taskbar, tray, url/snip, startup) and whether it is one static class or a few; the facade must be
trivially thin (same code, same call order); note `Native` visibility changes (e.g. make P/Invoke holders private/internal to
Platform); (c) what must stay as is and why; (d) risks, ranked, with concrete examples from the code; (e) proposed selftest /
`LAYERS` rules compatible with A39-motion-out-of-ui's table (e.g. `UI`/`Editor` must not reference `Native` or any type that
declares a P/Invoke; only `Platform` files declare `DllImport`/`LibraryImport`; a source-level grep guard is acceptable as a
second check) and how to prove a guard fails; (f) conflicts/ordering with A39-A43 (file-level overlaps, especially A41 which
moves code out of `PadWindow`; check the other branches with `git diff dev..reorg/A41-padwindow-split --stat` etc.);
(g) estimate: files touched, lines changed.
Keep it factual and compact (tables, no prose padding). Quote file:line for every claim.
