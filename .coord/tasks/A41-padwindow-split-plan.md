# A41-padwindow-split: plan (r2, against dev edea565 after A39 and A40)

Status: PLAN ONLY (this session is an investigator; implementation starts later from a separate flow, after the user decides
from the "Recommendation on handling" below). Evidence: `.coord/reports/A41-padwindow-split-impact.md` (r1, dev b54b476) and
`.coord/reports/A41-padwindow-split-impact-r2.md` (r2, dev edea565), both Codex director, read-only; manager spot-checked r2 against
`PadWindow.cs:357-367, 441-447`, `PadWindow.Keys.cs:261-284`, `Platform/GlobalHotkey.cs:9` (sealed), `ISettingsHost.cs:12-13`.
References: W = `UI/PadWindow.cs`, L = `.Layout.cs`, N = `.Navigation.cs`, K = `.Keys.cs`, B = `.TopBlur.cs`; lines are dev edea565 and
shift, so re-anchor by member name. The five partials are now 2058 lines (W 555, L 493, N 404, K 285, B 321).

## Recommendation on handling
- **Is A41 still worth doing at its original scope? No.** Four extractions in one task mostly redistribute coupled code in a
  2058-line presentation owner. A40 already removed the biggest coupling (SettingsWindow -> PadWindow now goes through
  `ISettingsHost`) and left a 4-line `ScheduleSave` seam, so the save-scheduler extraction has almost nothing left to win.
- **Split into small tasks, do only the first by default:**

| Task | Scope | Risk to show/hide, focus | Order | Verdict |
|---|---|---|---|---|
| `A41a-hotkeys` | Move global-hotkey registration (two slots, swap/rollback, status, disposal; W:29, 35-36, 281-282, 441-492) into one class behind a small fakeable registration seam, with a rollback selftest | Medium: it is the hotkey -> toggle entry of the hot path and also runs at startup before prewarm | 1 | **Do** (the only slice with a real regression-risk reduction) |
| `A41b-clipboard-policy` | Move only the pure rules of `OnClipboardChanged`/`CopyText` (own-copy, last capture, "already contained", trim/indent) into a pure class with a table selftest; OCR-first, privacy check, target pick, document insert and hidden-time stay in the window (N:287-319) | Low: background clipboard events, but `_hiddenAt` update must keep its place | 2 | Optional; do if you want the dedupe/privacy rules tested |
| `A41c-save-scheduler` | Wrap the 400 ms timer | Medium (hide path calls `QueueSave`) | - | **Skip**: debounce is already 4 lines (`ScheduleSave`, W:364-367); the ending/quitting checks differ on purpose between `QueueSave` and `Save` and must not be unified |
| `A41d-navigation-core` | Pure stack transitions | High: shares selection with `SearchOverlay` (`SearchOverlay.cs:266-274, 349-383`), and split repair/cycling now emit settings events during binding | - | **Skip** for now; revisit only with a parity contract that includes settings persistence and `SearchChangedData` |
| Taskbar glue, hidden/Settings trims, timer attention, OCR, layout, keys, overlays, appearance, top-bar blur | - | - | - | **Leave in the window** (races with activate/deactivate, GC timing, or true window work) |

- **Riskiest for show/hide speed and focus, and how you hand-check them (dev copy, `tools/dev.ps1`):**
  1. `A41a-hotkeys` (direct hot-path entry). Check: cold start, then press the hotkey at once and type (first key must land); visible-but-inactive pad + hotkey must focus, not hide; spam the toggle; rebind in Settings to a free key (new works, old stops) and to a taken key (old keeps working, error shown); set and clear the new-note hotkey. Compare the dev `hotkey-to-focus ms/sync/focused` log over 5+ runs before and after; no new dispatcher hop is allowed, and the diff of `ShowResident`/`HideResident` must be empty.
  2. `A41b-clipboard-policy` (background activity, can show/focus via OCR). Check: copy while hidden is captured into the Clipboard note; copying out of the pad is not echoed; password-manager copy is ignored; snip-OCR still lands in the original note after you navigate.
  3. (Only if ever done) save scheduler / navigation: type-then-hide-at-once, forced save failure, session-end edit, search delete/undo, split navigation.
- **What can be skipped:** scheduler wrapper, navigation core, taskbar adapter, trims, timer attention, OCR, and any line-count target. The old W<=430 / total<=1850 targets are dropped as acceptance criteria.

## What
`A41a-hotkeys` (default scope): a new internal class (proposed `Platform/HotkeyRegistrar.cs` or `UI/PadHotkeys.cs`; Platform preferred so
the pure-UI ban rows work without self-violation) owning:
- the two slots `_hotkey` and `_newNoteHotkey` (id 0x4151), `HotkeyStatus`, `NewNoteHotkeyStatus`, `HotkeyStatusChanged`, `SwapHotkey` (verbatim: same label no-op; release old first; restore old if Windows refuses; leave empty if restore fails), disposal.
- a minimal registration seam (label, `IsRegistered`, error text, `Dispose`) because `GlobalHotkey` is `sealed` and registers against a Window in its constructor (`Platform/GlobalHotkey.cs:9, 34-43`), so a "fake registrar" needs an adapter around it.
`PadWindow` keeps `RegisterHotkey`, `RegisterNewNoteHotkey`, `NewNoteHotkeyStatus`, `HotkeyStatus`/`HotkeyStatusChanged` as thin forwards (they satisfy `ISettingsHost`, `ISettingsHost.cs:12-13`; Program subscribes at `Program.cs:362`). Call sites unchanged: `InstallHooks` (W:110-111), `ResetSettings` Apply (K:261-262), `SettingsWindow` hotkey edit via host.
`A41b-clipboard-policy`: new pure class (Editor) holding `_ownClipboard`/`_lastCaptured` and `Decide(...)`; `PadWindow.CopyText`/`OnClipboardChanged` call it.

## Why
- Hotkey rollback (`SwapHotkey`, W:461-476) is subtle (Windows can hold two registrations with one id; old must be released first and restored on refusal) and has no selftest because it only runs inside a live window. Registration is also tied into startup order (register before prewarm) and into `ResetSettings` (registration return values are ignored there). A silent change in any of these means "hotkey stops working" found late.
- Clipboard dedupe/privacy rules are only reachable through a live `PadView.DocumentFor`, so they cannot be selftested; a regression echoes copies or captures secrets.
- What A39/A40 already removed (do not redo): `SettingsWindow` -> `PadWindow` dependency (guarded by `LAYERS`, `SelfTest.cs:119`); raw settings writes (setter guard); `Editor` -> `UI` cycle.

## How
Design rules for every sub-task:
- Collaborator created in the `PadWindow` constructor, before `InstallHooks`; no lazy init, no subscription to `SettingsService.Changed` (A40's `Changed` has no registration branch and registration must stay "register first, commit on success", `SettingsWindow.cs:489-496`), no extra dispatcher hop. Keep the two existing `Dispatcher.UIThread.Post` calls (toggle with original timestamp; new-note = ShowResident then NewNote) exactly where they are in composition (`W:449, 487`).
- Read settings through `Settings`/`SettingsService.Current` at call time; never cache the `Settings` object (`Replace` swaps it).
- Do not touch: `ShowResident`, `HideResident`, `Prewarm`, `FocusActive`, `QueueHideOnBlur`, `PrepareForSessionEnd` bodies, the `Deactivated` handler, frame styles, the A40 `HandleSettingsChanged` order (Topmost -> taskbar -> appearance -> persistence, K:269-284), the constructor order (split repair in `BindPanes` runs before the `Changed` subscription, `Program.cs:347-349`).

`A41a-hotkeys` steps: (1) add the seam interface + adapter over `GlobalHotkey`; (2) move slot/status/swap/dispose into the class with the toggle/new-note actions injected as `Action<long>`/`Action` and a `Action<string>` for the parse-failure toast; (3) PadWindow forwards; `CompleteQuit` disposes via the class; (4) selftest + `LAYERS` rows; (5) build, gate, hand-check.
`A41b-clipboard-policy` steps: pure decision class, keep order `own-copy recorded before SetTextAsync`, `_lastCaptured` set before `DocumentFor` insert, OCR-before-text, private check before text read.

Files touched: `UI/PadWindow.cs` (A41a), `UI/PadWindow.Navigation.cs` (A41b), new file(s), `SelfTest.cs`. Unchanged: `Program.cs`, `Tray`, `SettingsWindow`, `SearchOverlay`, `NoteSwipe`, `PadView`, `Store`, `SettingsService`.

## Success looks like
- Build 0 warnings, 0 errors; `--selftest` PASS, existing lines unchanged plus:
  - `HOTKEYS swap` (fake registration seam): same label no-op; old released before new created; refused new restores old; failed restore leaves empty; blank new-note gesture disables; each slot disposed once; parse failure notifies and returns false.
  - (A41b) `CLIPBOARD policy` table: blank, own text, repeat of last, already contained, CRLF/multiline indentation.
- New rows in the existing table-driven `LAYERS` (`SelfTest.cs:109-134`, `LayerRule` ExactFrom/ExactTo; external namespaces like `Avalonia.Controls` cannot be To rows, so ban individual types such as `Window`/`Control`/`Dispatcher`/`PadView`/`PadWindow`/`Native`/`Program`; a helper declared in `UI` cannot use a `UI` ban row because it self-violates, hence Platform/Editor placement): new classes must not reference `PadWindow`, `PadView`, `Program`; `ClipboardCapture` must not reference `Native`, `Platform` or `Motion`.
- A "PadWindow inbound allowlist" needs a separate small check (the table has no "all except" form): only `Program`, `SearchOverlay`, `HelpOverlay` (and `PadWindow` itself); not `SettingsWindow` (A40 forbids it).
- Guard proof: add a deliberate violation per new rule, see the selftest fail and name it, remove it (reported).
- Grep/diff checks: post count of `Dispatcher.UIThread.Post` summed over PadWindow + new class equals the count before (no new hop); `rg "ShowInTaskbar|WindowDecorations|ExtendClientArea|SetWindowLong|DwmSetWindowAttribute"` diff empty; `git diff` of `ShowResident`, `HideResident`, `Prewarm`, `FocusActive`, `QueueHideOnBlur`, `HandleSettingsChanged` empty (A41c, if ever done, would need an explicit allowance for the session-end timer stop).
- Line counts are informational only (A41a saves tens of lines in W; the value is the testable lifetime boundary, not a quota).
- Hand checks: listed per task in "Recommendation on handling"; the user approves in the implementing session before anything is reported done.

## Impact radius
Code: `PadWindow.cs` hotkey region (A41a), `PadWindow.Navigation.cs` clipboard region (A41b), `SelfTest.cs`, new files. Behavior radius: global hotkey registration/rebinding; clipboard-mode capture. Outside: window frame, layout, bars, blur, taskbar, tray, `Store`, `SettingsService`, `SettingsWindow` (it keeps calling `ISettingsHost.RegisterHotkey`).

## Risks
- Show/hide latency: hotkey is the first link of the hot path. Mitigation above (no new hop/subscription/lazy init; direct delegates; before/after `hotkey-to-focus` log).
- Startup order: registration must still precede prewarm and a valid native handle is assumed (`GlobalHotkey` retries on Opened, `Platform/GlobalHotkey.cs:41-55`); untested in source, hand-check cold start.
- `ResetSettings` ignores registration return values (K:258-265); keep that, do not "fix" it here.
- A40 interplay: `Changed` is synchronous and owner-thread only; hotkey code must not be wired to it; settings replaced by `Replace` means no cached `Settings`.
- Selftest limits: a fake seam proves rollback logic, not Windows focus or real registration conflicts.
- Silent regressions the selftest cannot see (focus, first-show timing, DWM) are covered only by the hand checks.
- Prewarm still has no generation guard (existing hazard, not changed); do not add a post near it.

## Depends on / conflicts with
- **A39 (landed)**: moved `Anim`/`ChromeTokens`/`MenuEntry` and added the IL `LAYERS` table; r2 corrected that the menu now goes through a host delegate `ShowMenu` (`L:113-132`). A41 only adds rows to that table.
- **A40 (landed)**: r1 plan claimed `SettingsChanged` called `RegisterHotkey`; wrong. Registration happens in `InstallHooks`, `ResetSettings`, and `SettingsWindow` via `ISettingsHost`, never inside `Changed`. A41 keeps that.
- **A42-padview-commands (next)**: `DocumentFor`/`Flush`/`Forget`/`Arrive` calls in clipboard and navigation stay in the window as adapter callbacks, so A42 can move them freely; A41b should land before or after A42 without touching `PadView`. Conflict risk on `N:287-319` if A42 edits `DocumentFor` usage: do A41b after A42 or rebase.
- **A43-app-context**: new classes must take values/delegates, never `Program.*` (existing refs at W:23 timers liveness and dev logging stay for A43).
- **A44-platform-seam**: `Native.ClipboardIsPrivate()` (N:~307) and `Native.ListenClipboard` (W:111) stay in the window; `ClipboardCapture` takes `isPrivate` as a parameter. No new `Native.*` in extracted classes.
