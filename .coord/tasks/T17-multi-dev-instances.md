# T17-multi-dev-instances: several dev copies at once, one color each

Item card (`docs/loop.md`). Claude fills Request to Approve before any build; Codex reads the whole card, follows "For Codex", never edits it. Budget: 4 KB (logs go in reports).

Tier: High · startup, single-instance, hotkey, Platform (item chat confirms in Findings)
State: plan ready
Branch: `chore/T17-multi-dev-instances` in `.claude/worktrees/chore/T17-multi-dev-instances` (created at Approve, from dev's tip)

## Request
User, 2026-11-17: run several dev copies at once, one per worktree, so three items can hand-test side by side. Each copy gets its own color (the soft yellow and blue already in use first; order Blue > Red > Green > Purple > Orange > Silver from the user's list), its own global show/hide hotkey, and if it can be done cleanly a new-note hotkey. The installed copy, its shortcuts and its data stay untouched. In-app shortcuts are not a concern. Full proposal and open questions: `.coord/research/multi-dev-instances-plan.md` (read it first).

- **Goal:** launching the dev copy from any worktree opens its own copy with its own color, identity and hotkeys, without replacing another worktree's copy or touching the installed copy.
- **Done check:** (1) two or three worktrees run dev copies at once, each reachable by its own hotkey; (2) relaunching one replaces only itself; (3) the installed copy's id, hotkey and data are unchanged (proved, not assumed); (4) a seventh launch refuses with a clear message; (5) each copy shows its color on the dev badge and tray icon.
- **Proof:** findings list of everything naming `AntiNotepad-dev` or the dev hotkey; offline selftest of the slot allocator (order, reuse, free, cap) and of two ids not colliding; a showcase page (see below); hand test by the user with two or three copies.
- **Showcase (user asked):** before the Plan is approved, produce one HTML diagram page in `tools/playground/` showing what this allows: worktrees, slots, colors, hotkeys, the installed copy kept apart, and what happens on launch, relaunch, seventh launch and removal. The user reviews it as proof the chat understood the task. Visible design (badge and tray colors) is lab-first.
- **What we know:** the shared instance id, mutex, pipe, hotkey and launcher (`docs/dev-loop.md`, `Platform/SingleInstance.cs`, `tools/dev.ps1`); the plan's four steps (findings, launcher and slots, app side, docs and rules).
- **Constraints:** High tier; never touch the installed copy or run `--quit` without `ANTINOTEPAD_DATA`; dev launcher never creates or edits Start menu or Desktop shortcuts; Codex offline only (no launching AntiNotepad, no ports); the item chat's own hand-test copies must not collide with other running items. Open questions for the user go in the Plan: hotkey scheme (default proposal Ctrl+Alt+Shift+1 to 6), cap of six, whether the badge shows the item name.

## Findings
Tier High confirmed (startup, single-instance, hotkey). Own read-only scout, 2026-11-17; showcase: `tools/playground/t17-dev-slots-showcase.html` (port 47812).
- **Identity:** `AppInfo.DevInstanceId` = `AntiNotepad-dev` (`AppInfo.cs:73`); `Program.cs:48` picks it for any nonempty `ANTINOTEPAD_DATA`, also for `--quit`. `SingleInstance.cs` derives mutex `-Mutex`, owner map `-Owner` and pipe from the id plus user SID.
- **Launcher:** `tools/dev.ps1` hard-codes the stem (`:150`), hotkey (`:270`, `:413`), one `build/dev.pid`, force-close allowlist (`:230-246`), one launcher mutex `-Launcher`. `tools/dev.sh` only forwards.
- **Hotkey:** `ANTINOTEPAD_HOTKEY` read in `HotkeyRegistrar.cs:66` (used only when `Settings.Hotkey` is null). `Settings.NewNoteHotkey` has no env override: a per-slot new-note key needs one small app-side env read.
- **Look:** badge = `DevBadge` (`PadWindow.Layout.cs:400`, text `AppInfo.FormatDevBadge`, color `p.Number`); tray, window and taskbar icon = one `antinotepad-dev.ico` via `AppInfo.IconUri`, tinted hue 207 (blue) by `tools/make-dev-icon.ps1`. Installed icon is amber.
- **Others naming it:** `tools/press-hotkey.ps1` (default `D`); selftest `SelfTest/App.cs:206-222` (asserts id); `tools/gate.ps1:279,310` and `merge-preflight.ps1:48,85` (strip `ANTINOTEPAD_DEV`/`DATA`; must also strip the new slot var); `tools/loop-lint.allow:13`; `guard-bash.ps1:77` (`--quit` rule, unchanged); docs `dev-loop.md` (7, 12, 55, 63-82, 94), `wiki/Parallel-and-Branches.md:50`, skills `next-task`, `review-task`, `merge-task/branches.md:18` ("one dev copy"). Historical cards and reports left alone.
- **Smallest change set:** (1) slot env `ANTINOTEPAD_SLOT` -> id `AntiNotepad-dev-<color>`, badge/icon color, new-note env; (2) `dev.ps1` slot allocator, per-slot replace, `dev-list.ps1`, `press-hotkey -Slot`; (3) selftest; (4) docs/skills. No slot env keeps today's `AntiNotepad-dev`, so gate, selftest and `--quit` helpers stay as they are.
- **Badge in worktrees (lab: `tools/playground/t17-badge-lab.html`):** branch stamped at build by `git branch --show-current`; `FormatDevBadge` strips only `feat/` and reads one-letter IDs, so `fix/`, `chore/`, `docs/` and `Loop12` branches show junk (`chore/T15-ff`); detached HEAD shows plain `dev`. Light pad uses tuned (darkened) text, dark pad keeps the slot color (user, 2026-11-17). Slot-color text on a light pad has contrast 1.6-2.5:1 (blue, red, green, purple, orange, silver); tuned text reaches 5.9-7.3:1.
- **Slot liveness** comes from the slot's own mutex (vacant = free), not a PID file; a remembered worktree-to-color map lives under the main checkout's `build/`, pruned against `git worktree list`.

## Plan
Decided (user, 2026-11-17): show/hide Ctrl+Alt+Shift+1 to 6, new note Ctrl+Alt+Shift+F1 to F6; cap of six, seventh launch refuses; badge reads `ID - name - build time` (e.g. `A22 - wrap-indent - 3:14 PM`), resting `A22-h:mm` (e.g. `A22-3:14`, 12-hour, no AM/PM), full on hover with 12-hour time; the chip unfurls to the left (anchored at the right edge) and cross-fades between the two (user, 2026-11-17), in the slot color instead of today's purple; colors Blue, Red, Green, Purple, Orange (`#ee7a24`), Silver; installed stays soft yellow.
**Now vs after:** one shared id/hotkey/launcher record, one copy at a time -> up to six copies, each with its own id, hotkeys, color, data; installed copy untouched.
- **Steps (each a commit that builds):** (1) app: fix `FormatDevBadge` (drop any `prefix/`, IDs like `Loop12`, selftest rows); `ANTINOTEPAD_SLOT` -> id `AntiNotepad-dev-<color>`, badge and icon color, `ANTINOTEPAD_NEWNOTE_HOTKEY`; badge format; per-color dev icons (`make-dev-icon.ps1 -Color`). (2) `dev.ps1`: slot allocator (own remembered color, else first free; free = id vacant), per-slot replace and quit, per-slot pid file, refuse a seventh, print COLOR; `dev-list.ps1`; `press-hotkey.ps1 -Slot`. (3) selftests. (4) docs and skills.
- **Tests (offline):** `--selftest`: ids for two slots differ and never equal `AntiNotepad`/`AntiNotepad-dev`; no slot var keeps today's id; hotkey/new-note env parse; badge format. Allocator: a pure function with a selftest in `dev.ps1 -SelfTest` (order, reuse, free, cap, removed worktree). Gate: build, selftest, Format, Tokens (new colors go in `Theme/`).
- **Lab first:** the six tones and the badge look get a lab page before step 1 ships; you approve it.
- **Hand test (you):** run 2-3 worktrees at once; press each hotkey; relaunch one; check the installed copy's id, hotkey and notes are unchanged (I print its mutex and hotkey registration before and after); launch a seventh; remove a worktree.
- **Risk:** High (startup, single-instance, hotkey). A wrong id could touch the installed copy: slot ids always carry the `-dev-` stem, the quit helper takes the exact slot id plus `ANTINOTEPAD_DATA`, installed images stay rejected. Codex offline only; I launch the app.

## Files
- `product/AntiNotepad/AppInfo.cs`
- `product/AntiNotepad/Program.cs`
- `product/AntiNotepad/Platform/HotkeyRegistrar.cs`
- `product/AntiNotepad/UI/PadWindow.Layout.cs`
- `product/AntiNotepad/SelfTest/App.cs`
- `product/AntiNotepad/Theme/ChromeTokens.cs`
- `tools/dev.ps1`
- `tools/dev.sh`
- `tools/dev-list.ps1`
- `tools/press-hotkey.ps1`
- `tools/make-dev-icon.ps1`
- `tools/gate.ps1`
- `tools/merge-preflight.ps1`
- `tools/loop-lint.allow`
- `docs/dev-loop.md`
- `docs/wiki/Parallel-and-Branches.md`
- `docs/wiki/Home.md`
- `docs/wiki/Where-Files-Live.md`
- `AGENTS.md`
- `.claude/skills/next-task/SKILL.md`
- `.claude/skills/review-task/SKILL.md`
- `.claude/skills/merge-task/branches.md`
- `tools/dev-list.ps1`
- `product/AntiNotepad/AntiNotepad.csproj`
- `product/AntiNotepad/Motion/MotionTokens.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`
- `product/AntiNotepad/Theme/DevSlotTokens.cs`
- `product/AntiNotepad/antinotepad-dev-blue.ico`
- `product/AntiNotepad/antinotepad-dev-red.ico`
- `product/AntiNotepad/antinotepad-dev-green.ico`
- `product/AntiNotepad/antinotepad-dev-purple.ico`
- `product/AntiNotepad/antinotepad-dev-orange.ico`
- `product/AntiNotepad/antinotepad-dev-silver.ico`
- `tools/playground/t17-badge-lab.html`
- `tools/playground/t17-dev-slots-showcase.html`
Also the `next-task`, `review-task`, `merge-task` skill lines naming "one dev copy".

## Contracts
- No slot var: id `AntiNotepad-dev`, behavior as today.
- Installed id `AntiNotepad` and its hotkey, data and shortcuts never touched by dev tooling.

## Approve
User, 2026-11-17: "FYI since the badge is anchored on the right side of the app, should it unfurl to the left? but yes otherwise its approved" (answer: yes, unfurls left; Plan approved).

## For Codex
Role: quick, because docs and skill wording only. Branch is checked out: never switch, merge or commit. Read `AGENTS.md`, this card (Plan, Decided), `docs/dev-loop.md`, `tools/dev.ps1` header and `tools/dev-list.ps1`. No code, no app launches.
**Part 3 (docs).** Parts 1 and 2 are done: slots Blue, Red, Green, Purple, Orange, Silver (ids `AntiNotepad-dev-<color>`, show/hide `Ctrl+Alt+Shift+1..6`, new note `Ctrl+Alt+Shift+F1..F6`), `tools/dev.ps1` picks the slot (own, remembered, first free, refuses a seventh), `tools/dev-list.ps1`, `tools/press-hotkey.ps1 -Slot <color> [-NewNote]`, badge `A22-3:14` resting and `A22 - name - 3:14 PM` on hover, in the slot color.
1. `docs/dev-loop.md`: rewrite the dev-copy table and "Dev copy" bullets for slots (one copy per worktree, up to six, own color, id, hotkeys, `build/dev-<color>.pid`, registry `build/dev-slots.json` in the main checkout, seventh launch refuses, no-slot runs keep `AntiNotepad-dev`); keep every safety rule (installed copy never touched, `--quit` only with `ANTINOTEPAD_DATA`, Codex never launches). Fix `press-hotkey` and "replaces the current dev copy" lines.
2. `docs/wiki/Parallel-and-Branches.md` line about the shared launcher; `AGENTS.md` dev rule (one short clause: each worktree has its own slot copy); `.claude/skills/next-task`, `review-task`, `merge-task/branches.md`: "dev copy" becomes "your slot's dev copy"; "one dev copy" wording removed.
3. Keep each file's size budget (`tools/loop-lint.ps1`).
**Done means:** the files read consistently with the code above; `pwsh -NoProfile -File tools/gate.ps1 -Task T17-multi-dev-instances -Offline` passes; report `.coord/reports/T17-multi-dev-instances-p3.md` (Changed, Files, Checks run, Not run, Risks).

## Check
Gate PASS (offline; Size WARN: `PadWindow.Layout.cs` 765 -> 821). Independent review (reviewer agent): accept; tooltip removed after its note 4. Live (item chat, 2026-11-17): six copies from six worktrees ran at once, Blue to Silver, each with its own PID, hotkeys and data; relaunching Red changed only Red (PID 20008 -> 46220, Blue and Green PIDs unchanged); a seventh launch refused naming all six holders and replaced nothing; installed copy unchanged (PID 2096, start 11/13 10:31). Not run: Windows tray icons by eye, badge animation by eye, `git worktree remove` pruning.

## Hand test
User, 2026-11-17 4:48pm (early note, Blue slot copy launched 4:35pm): "Ctrl Alt Shift F1 and 1 works for this instance of the dev build". Screenshot: badge unfurled to the left showing `T17 - multi-dev-instances - 4:35 PM` in blue with its tooltip.
User, 2026-11-17, after trying six side-by-side copies (Blue to Silver): "All good".
Cleanup (item chat): five scratch copies quit with their own slot and data, scratch worktrees removed (registry pruned to the item worktree on the next launch), Blue relaunched with the final build; installed copy PID 2096 unchanged throughout.
