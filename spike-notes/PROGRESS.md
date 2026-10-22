# Antipad spike: PROGRESS (claude/product-spike, never merged)

STATUS: COMPLETE (2026-10-22 07:00). Release build in build/antipad includes P16. Watchdog stops on this line.

Goal: recreate AntiNote on Windows (feature + UI paired) with full creative freedom. Avalonia + AvaloniaEdit.
Project: product/Antipad. User-facing summary: spike-notes/WRITEUP.md.
Isolation (runs beside AntiQuickPad): instance id "Antipad", data %LOCALAPPDATA%\Antipad, hotkey Ctrl+Alt+Shift+S (env ANTIPAD_HOTKEY), Run-key value "Antipad".
Rules: no TRACKER/STATUS/board, never touch dev/main. No import/export or file dialogs. No questions (user asleep). Commit per item `P<NN>-slug`.
Watchdog: spike-notes/watchdog.ps1 (detached pwsh, log spike-notes/watchdog.log). `spike-notes/STOP` halts it.

## RULES FOR HEADLESS (watchdog-resumed) RUNS — read first
- The user may be running build\antipad\Antipad.exe against their REAL data. Never taskkill Antipad.exe, never build into build/antipad, never run spike-notes/dev.sh.
- Build to build/headless: `dotnet build product/Antipad -c Release -o build/headless` and verify with `build/headless/Antipad.exe --selftest`.
- No computer use in headless runs (tool unavailable). Only code-only work verifiable by --selftest.
- Commit each item; update this file's Log and Next.

## Live-session dev loop (interactive only)
- spike-notes/dev.sh [--fresh]: kills only its own instance (build/dev.pid), refuses if another Antipad.exe runs, builds Debug to build/antipad, launches on build/testdata.
- F12 writes build/shots/shot.png, Shift+F12 shot-b.png, Ctrl+F12 gallery-NN.png (needs ANTIPAD_SHOTS). Captures miss the Acrylic backdrop and the bottom pill/toast contents.
- spike-notes/press-hotkey.ps1 presses the hotkey from a script; timings land in <data>/antipad.log.
- Computer-use grant is keyed to build\antipad\Antipad.exe; keep that path and AssemblyName.

## Done
- [x] P01-resident-shell, P02-note-stack, P03-editor-basics, P04-math-mode, P05-code-mode, P06-timer-and-clipboard
- [x] P07-search-trash, P08-slots-split, P09-hardening (hidden start, privacy, focus), P10-theme-maker, P11-writeup
- [x] P12 extras: drawn checkboxes, clean markdown, drop-down presentation, search highlight + drag reorder, tray slots menu

## Next (headless-safe items)
- Nothing required. Optional: more calculator edge cases with --selftest checks.

## Log
- 2026-10-22 03:20 branch created, watchdog started, computer-use granted for build\antipad\Antipad.exe
- 03:50 P01-P08 built and verified live via computer use + F12 captures.
- 04:00 hardening verified: hotkey 24-30 ms show-to-focus (87 ms first show after --hidden), swipe, Esc/popup, OCR read a test image exactly, drag reorder.
- 04:10 undo paths verified against antipad.json; looks verified (mica, solid, dotted, grid, contrast, split palette). Drop-down: sync focus 13 ms (the 130 ms number is the post-animation confirmation).
- 06:55 headless run: /checklist /bullets /numbered /plain /uncheck /tidy via pure TextCommands (selftest-covered); whole-note commands are one undo step. Built to build/headless only.

## State 2026-10-22 ~11:00 (before compaction)
- Tag antipad-v1 = first overnight build. Since then: V2 (memory/per-note files, detent swipe, structure features, smooth scroll/caret), V3 (polish), V4 (native-weight ink icons, bubbles baseline+column, pane-wide code panels, fence language picker, Settings > Code languages), fence-Enter crash fix (not yet re-tested live by user).
- User's copy runs from build/antipad on real data (%LOCALAPPDATA%\Antipad, v2 index.json + notes/). Swap procedure: `build/antipad/Antipad.exe --quit`, `dotnet build product/Antipad -c Release -o build/antipad`, `--selftest`, relaunch.
- Dev builds: spike-notes/dev.sh -> build/dev, instance Antipad-dev, hotkey Ctrl+Alt+Shift+D (spike-notes/press-hotkey.ps1 presses D by default). Computer-use grant covers build\dev\Antipad.exe and build\antipad\Antipad.exe; never type into the user's copy.
- Crashes now log to <data>/antipad.log ("crash ..."); also Windows Application log (.NET Runtime).

## Scroll decisions 2026-10-22 (Scroll Lab, https://claude.ai/artifact/JkwihBpKANjcujnsrsTxCJ)
- User's mouse: Logitech MX Master 3S. Browser and Windows deliver whole clicks (deltaY 100, wheelDelta 120) on both wheels; speed shows only as click rate.
- LOCKED for vertical scrolling: Eased glide (Antipad "Lines" today), Rate-matched glide (40 px slow click, up to 3.5x at 30 clicks/s, glide <= gap between clicks), Wheel-driven sheet (click rate sets sheet speed, quick stop). Lab source: scratchpad scroll-lab.html, techniques "glide", "rate", "flywheel".
- Horizontal (note paging): must follow the input visibly before a note change fires, and change exactly one note per gesture. Still being tuned in the lab.
