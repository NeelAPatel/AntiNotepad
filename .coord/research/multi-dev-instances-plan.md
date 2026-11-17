# Plan: several dev copies at once, one color each (Master 6, 2026-11-17)

Asked by the user 2026-11-17 after A22 and A23 hand tests collided: only one dev copy can run, because every worktree shares the instance id `AntiNotepad-dev`, its mutex, the hotkey Ctrl+Alt+Shift+D and one launcher that replaces the running dev copy (`docs/dev-loop.md` lines 7-13, 64-82; `Platform/SingleInstance.cs`).

User's words: launch several AntiNotepads from their worktrees' own code, each picking a color in the fixed order **Blue > Red > Green > Purple > Orange > Silver**; dev worktrees use a different launch mechanism and leave the current Prod shortcuts for the installed copy alone.

## Design (proposal, nothing built)
- **Slot = color.** At a worktree's first dev launch it takes the first free color in the order above and keeps it (stored in that worktree's `build/dev-slot`; a registry of live slots in the main checkout's `build/`). It frees when the worktree's process is gone or the worktree is removed. Six at once is the cap; a seventh launch says so instead of replacing one.
- **Identity per slot.** Instance id becomes `AntiNotepad-dev-<color>` (mutex, pipe, window name), so slots never fight. Prod stays `AntiNotepad`.
- **Hotkey per slot** (open question, below): default proposal Ctrl+Alt+Shift+1 to 6 in color order, set by the launcher through `ANTINOTEPAD_HOTKEY`. Prod keeps Ctrl+Alt+Shift+S.
- **Data per slot.** Already per checkout (`build/testdata` inside each worktree); unchanged. A slot never touches real data.
- **Look.** The dev badge shows the color and item: `dev · blue · A22`; a colored accent on the badge and the tray icon so the user can tell windows apart. Visible, so lab-first.
- **Launcher.** `tools/dev.ps1` keeps its shared transaction mutex (slot assignment must not race) but replaces only its own slot's copy, never another worktree's. It prints PID, EXE, DATA, HOTKEY and COLOR. The quit helper targets the exact slot identity with an exact path match (rule unchanged: never `--quit` without `ANTINOTEPAD_DATA`).
- **Prod shortcuts reserved.** Dev never creates, edits or reuses Start menu or Desktop shortcuts; those belong to `tools/install.ps1` and the installed copy. Dev copies start by exe path from the launcher only. A small `tools/dev-list.ps1` shows running slots (color, worktree, PID, hotkey) and `tools/press-hotkey.ps1 -Key` takes a slot.

## Steps (each its own item when numbered)
1. **Findings (read-only):** everything that names `AntiNotepad-dev` or the D hotkey: `AppInfo`, `SingleInstance`, tray, quit helper, `press-hotkey`, `guard-bash`, selftests, docs. Output: a list and the smallest change set.
2. **Launcher and slots (tooling, T):** slot allocator, per-slot replace, `dev-list`, docs. Offline selftest of the allocator (order, reuse, free, cap).
3. **App side (A):** instance id and badge color from the slot (env or flag), hotkey from env, selftest for two ids not colliding. Badge and tray color lab first.
4. **Docs and rules:** `docs/dev-loop.md`, the AGENTS rule, item-chat briefs ("dev copy" becomes "your slot").

## Update (user, 2026-11-17, after the roadmap talk)
- Wants this tooling item soon so three worktree items can run at once with a hand test each.
- Colors: keeps the soft yellow and blue already in use (look of the current dev and installed badges); the rest of the order is still the user's list. Item chat reads today's badge and tray colors first and picks matching soft tones.
- Shortcuts: only the global ones matter: **show/hide** (conflicts today) and, if it can be done cleanly, **create a new note** (the shortcut launches or shows the app and makes a note). In-app shortcuts are not a concern.
- Each slot gets its own show/hide hotkey; the installed copy keeps its own, untouched.

## Open questions for the user
- Hotkeys per slot: digits 1-6, or another scheme?
- Cap: is six enough, and what should a seventh launch do (refuse, or reuse the oldest)?
- Does the badge need the item name, or only the color?

## Risk
High tier: startup, single-instance, hotkey registration and `Platform/`; a wrong id could touch the installed copy. Hand test must prove the installed copy is untouched (its id, hotkey and data stay as they are).
