# A10-hotkey-recorder: set a hotkey in Settings by pressing the keys

Item card (`docs/loop.md`). Claude fills Request, Findings, Plan and Approve before any build; Codex reads the whole card and
follows "For Codex". Codex never edits this file; Check and Hand test are filled by the sub-session afterwards.

Tier: Medium · visible Settings change, lab-first (rules: `docs/loop.md`)
Branch: `feat/A10-hotkey-recorder` in `.claude/worktrees/feat/A10-hotkey-recorder` (created at Approve)

## Request
2026-10-26 (user wish): binding a hotkey in Settings should record the actual key press, not typed text. Lab-first. (Backlog line A47-hotkey-recorder, retired with era 0.)
2026-11-09 (user): confirmed this scope; numbered A10. Runs in its own worktree beside A9-pinned-accent-ring and A11-settings-thread-count; each dev copy shows its own branch and launch time in the dev badge.

## Findings
- **Kind and why:** feature, polish; today the user must type text like `Ctrl+Alt+Shift+S`.
- **Now:** `HotkeyEditor` (`UI/SettingsWindow.cs:540-593`) shows key caps with a Change button; Change swaps in a text box (Enter applies, Esc cancels). The text goes to `_host.RegisterHotkey` / `RegisterNewNoteHotkey` (`UI/ISettingsHost.cs:12-13`, `UI/PadWindow.cs:462-464`) and `Platform/HotkeyRegistrar.cs`; the parser in `Platform/GlobalHotkey.cs` (~line 100) requires a modifier and a key. Two hotkey rows use the editor (`SettingsWindow.cs:498-502`).
- **Where:** `UI/SettingsWindow.cs` (794 lines, over the 600-line "one job?" check; the editor could become its own file), `Platform/GlobalHotkey.cs`, `SelfTest/`.
- **Risk:** capturing keys inside Avalonia: modifier-only presses, keys Windows reserves (Win key, Ctrl+Alt+Del), and Esc being both "cancel" and a possible key. Registration failure message already exists ("Windows won't give ...").
- **Source:** master scout (grep and reads).

## Plan
Step A10b-hotkey-recorder-build, after the user approved the lab (`tools/lab/A10a-hotkey-recorder-lab.html`, last commit ebba09a).
- **After:** the shortcut row keeps its key caps and Change button. Change swaps the caps for a recorder (match the lab):
  - Waiting: a pill "Press the new shortcut" (accent border, static dot, no pulse), a Save button (disabled), Cancel, and on the optional new-note row (`allowEmpty`) Clear. Hint line under the row: "Esc or Cancel cancels" (plus " · Clear removes the shortcut" when Clear shows).
  - Holding keys: modifiers (Ctrl, Alt, Shift, Win) and one main key show live as accent-bordered caps. Only modifiers held: a dashed ghost "key" cap, Save disabled. Letting go keeps the last captured combo on the caps; pressing more keys replaces it.
  - Save, or Enter with no modifier, applies it: text like `Ctrl+Alt+K`, order Ctrl, Alt, Shift, Win, then the key. Success returns to the key caps. If `apply` returns false the recorder stays open with the red hint "Windows won't give <App> <combo>; another app may use it." and the old shortcut stays active.
  - Esc with no modifier, or Cancel: back to the old caps. Esc with modifiers is a recordable combo (Ctrl+Esc).
  - Clear (optional row only): applies empty text, caps show "None".
  - Main keys are the ones the parser supports: A-Z, 0-9, F1-F24, Space, Esc, Tab, Enter, Insert, Delete. Any other key (arrows, punctuation) leaves the caps unchanged and shows the hint "That key can't be a shortcut. Use A-Z, 0-9, F1-F24, Space, Tab, Enter, Insert or Delete."
  - The typed box is gone.
- **One job? (`docs/coding.md`):** `UI/SettingsWindow.cs` is 794 lines and holds one job, the Settings window and its pages. The hotkey editor is a self-contained control with its own state and its own reasons to change, and this item rewrites it, so the new recorder gets its own file `UI/HotkeyRecorder.cs` and `SettingsWindow.cs` loses the old 55-line `HotkeyEditor` (net smaller). The two `SystemPage` call sites keep their shape so the sibling A11-settings-thread-count has little to merge. No further split is planned.
- **Expected time:** about 1 hour (one Codex run, one review).
- **Must not change:** how hotkeys register (`HotkeyRegistrar`, `GlobalHotkey`), the stored `Settings.Hotkey` and `Settings.NewNoteHotkey` text format, default hotkeys, the `ISettingsHost` methods.
- **Scope questions:** answered in the lab review (2026-11-09, user, see "Lab review"): Save button applies (not on key release); Esc button named Cancel and outlined; Clear on the optional row only; typed box removed; Esc cancels.
- **Test plan, code:** selftests in `SelfTest/UI.cs`: (1) a pure `HotkeyRecorder` helper turns (Avalonia `Key`, `KeyModifiers`) into stored text, with the modifier order above; (2) round trip: every supported main key with Ctrl+Alt gives text that `GlobalHotkey.HotkeyGesture.Parse` accepts and whose `Label` equals it; (3) modifier-only and unsupported keys give no gesture; (4) Esc and Enter with no modifier are commands, with a modifier they are keys. One `CONTRACTS` row.
- **Test plan, UI:** both hotkey rows. Change, press Ctrl+Alt+K: caps fill live, Save enables, letting go keeps them; Save applies and the hotkey works. Esc and Cancel restore the old caps. Hold only Ctrl: ghost "key" cap, Save disabled. Press an arrow key: hint shown, caps unchanged. A combo another app holds (or Win+L): red "Windows won't give ..." message, old hotkey still works. New-note row: Clear gives "None". Check light and dark.
- **Gates:** build 0 warnings (code compiles clean); selftest (key mapping and round trip); Tokens (no new literal sizes or colors, so palette, `Shape` and `Typography` tokens only); Format (changed lines follow `.editorconfig`); Codex review (lens review on the diff).
- **Needs from you:** the hand test.

## Files
Only these files may change (`tools/gate.ps1` reads this list):
- `product/AntiNotepad/UI/HotkeyRecorder.cs` (new)
- `product/AntiNotepad/UI/SettingsWindow.cs`
- `product/AntiNotepad/SelfTest/UI.cs`
- `product/AntiNotepad/SelfTest/Contracts.cs`
- (lab file `tools/lab/A10a-hotkey-recorder-lab.html` was on the branch during the build; removed at the commit review)

## Contracts
- A key press recorded by `HotkeyRecorder` round-trips through `GlobalHotkey.HotkeyGesture.Parse` for every supported main key; modifier-only and unsupported keys record nothing.

## Approve
2026-11-09 (user): "Approve all three". Scope as planned: lab step A10a first (the user approves the capture states), build step A10b only after the user approves the lab.

## Lab review (A10a)
2026-11-09 (user): chose "Add Save button" (Save applies, not key release); asked that Esc be a clear "Cancel" button that does not look disabled; "Yes, as drawn" (typed box removed, Clear on the optional row); "Approve lab". Lab: `tools/lab/A10a-hotkey-recorder-lab.html`, commits e3f8cf1, d06c67e, ebba09a.

## For Codex
Role: director (`docs/agents.md`), because it is a visible Settings control with Avalonia key handling. Product `product/AntiNotepad`. The branch is already
checked out: never switch, merge or commit (you cannot write to `.git`). Read `AGENTS.md`, `docs/coding.md`, then the files above. Line
numbers are from the current tree; re-read before editing. The approved look is the lab `tools/lab/A10a-hotkey-recorder-lab.html` (read it; match states, texts and which buttons show where).

Build the Plan's "After":
- New `UI/HotkeyRecorder.cs` (namespace `AntiNotepad.UI`, header and `///` summaries per `.claude/rules/coding.md`): the control (replacing `HotkeyEditor`) plus a small static, testable mapper from (`Avalonia.Input.Key`, `KeyModifiers`) to stored gesture text or null. Inputs from `SettingsWindow`: `Palette`, `Func<string> current`, `Func<string,bool> apply`, `bool allowEmpty`, and a way to make the same text buttons Settings uses (pass a factory, do not duplicate styles).
- Key caps look as today (`Shape.Key`, border 1/1/1/2, `Typography.Small`); live caps use the palette accent for the border; the ghost cap is dashed. Use existing tokens in `Theme/`; no new literal sizes or colors (the Tokens gate counts them). The waiting dot is static.
- Recording: handle `KeyDown` on a focusable host and mark it handled so Avalonia does not move focus; handle `KeyUp` only to keep the last captured combo (do not clear it). Read the Win key via `KeyModifiers.Meta`. Recording ends on Save, Cancel, Clear or Esc, or when the control loses focus (treat as Cancel).
- Replace `HotkeyEditor` in `UI/SettingsWindow.cs` (about lines 539-593) with a one-line forwarder or direct construction of `HotkeyRecorder`; leave the two `SystemPage` rows' text and call shape unchanged. Touch nothing else in that file (another item edits it in parallel).
- Failure text is the existing sentence: `Windows won't give {AppInfo.Name} {text}; another app may use it.`

Constraints:
- Build exactly the Plan's "After" for the Files above. No drive-by refactors, renames, icon/token/style edits outside it.
- If the Findings turn out wrong (for example Avalonia 12 cannot deliver the keys needed, such as Win-key combos or Tab, to the control), stop and report that instead of building something else.
- Build into `build/A10-hotkey-recorder-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest` there, then delete that folder.
- Inside the Codex sandbox run the gate as `pwsh -NoProfile -File tools/gate.ps1 -Task A10-hotkey-recorder -Offline`; the manager
  re-runs it online.
- Never launch or kill AntiNotepad. Never run `--quit`. Never edit `.coord/STATUS.md` or this card.

Done means (offline checks only: build, selftest, gate, script checks, files; never ports, servers, browser or screen):
- `UI/HotkeyRecorder.cs` exists and `HotkeyEditor` is gone from `SettingsWindow.cs`; both hotkey rows use the recorder.
- The Plan's code tests exist and pass, including the round trip over every supported main key; the `CONTRACTS` row is added.
- Build 0 warnings, `SELFTEST PASS`, the gate passes.

Before you report: spawn the `lens_reviewer` agent on your diff (it reads
`.agents/skills/lens-review/SKILL.md`). Fix what it proves, or say why not.

Report: `.coord/reports/A10-hotkey-recorder.md`, short, with exactly these sections:
1. **Summary**: two or three sentences.
2. **Change list**: one row per changed block: `file:line-range | what changed | which Plan line it serves`.
3. **Behavior changes**: every changed public method result or meaning, new or changed shared state, timing, default, or
   setting effect. Write "None" only if true; the review checks this against the diff.
4. **Out-of-scope files**: file and why, or "None".
5. **Plan deviations**: anything done differently from the Plan, and why, or "None".
6. **Contracts**: rows added or changed, or "None".
7. **Lens review**: findings and what you did with each.
8. **Checks run**: build, selftest, gate result lines.
9. **Hand checks for the user**: the Plan's UI checks, plus anything new.

## Rework r2
Round 1 report: `.coord/reports/A10-hotkey-recorder-r1.md` (build, selftest and gate pass; one proven blocker). The manager lifts the "touch nothing else in `SettingsWindow.cs`" limit for this one fix, inside the allowed Files:
- `SettingsWindow.cs:77-83`: the window's tunnel `KeyDown` handler consumes Esc before the recorder sees it, so Esc closes Settings or clears search and Ctrl+Esc cannot be recorded. Make the handler skip (return without handling) when the event source is inside the hotkey recorder (for example a tag or a recorder-active check the control exposes), so unmodified Esc cancels the recording and modified Esc is recordable. When no recorder is recording, Esc behaves as today (clear search, else close).
- Add a selftest or contract line for that rule if it can be tested without a window; otherwise say so in the report.
- Keep everything else from round 1. Same checks and report format, written to `.coord/reports/A10-hotkey-recorder.md`. Run the gate with `-Task A10-hotkey-recorder -Offline`.

## Rework r3
Why this round (reviewer, 2026-11-09, cause `code`): the online gate `pwsh -NoProfile -File tools/gate.ps1 -Task A10-hotkey-recorder` fails only on Format, 13 WHITESPACE problems in `product/AntiNotepad/UI/HotkeyRecorder.cs` at lines 62-63, 123-124, 129-130, 150-151, 210, 212 (the formatter wants those lines broken and indented differently). Fix exactly those with `dotnet format whitespace --include product/AntiNotepad/UI/HotkeyRecorder.cs` (or by hand), no other change. Then run build, `--selftest` and `gate.ps1 -Task A10-hotkey-recorder -Offline` as before and rewrite `.coord/reports/A10-hotkey-recorder.md` (change list: Format only). If the online Format step cannot be reproduced offline, say so in the report.

## Rework r4
Why this round (user, 2026-11-09, at hand test): "Windows key should not be allowed at all. Ctrl, Alt, Shift, Function, thats it" (everything else passed). Taken as: the only modifiers the recorder accepts are Ctrl, Alt and Shift; the Windows key is never a modifier. Main keys stay as approved (A-Z, 0-9, F1-F24, Space, Esc, Tab, Enter, Insert, Delete). This is the user's go for a fourth round. Lab updated to match (`tools/lab/A10a-hotkey-recorder-lab.html`).
- `UI/HotkeyRecorder.cs`: stop reading `KeyModifiers.Meta` as a modifier. When the Windows key is pressed (as a key, or held with others), keep the current caps unchanged and show the hint "The Windows key can't be used. Use Ctrl, Alt or Shift." A combo that includes Meta never becomes a candidate. The mapper returns null for Meta combos.
- Do not change `GlobalHotkey` parsing or the stored format: an already stored hotkey with Win still loads and shows as caps; it just cannot be recorded.
- Selftests: mapper returns null with Meta; no recorded text contains "Win"; the 66-key round trip still passes with Ctrl+Alt. Update the `hotkey-recorder-round-trip` contract wording if it mentions Win.
- Run the format check on touched files before reporting (`dotnet format --verify-no-changes --include <file>`), then build, selftest, `gate.ps1 -Task A10-hotkey-recorder -Offline`. Rewrite `.coord/reports/A10-hotkey-recorder.md` (change list for this round only).

## Check
2026-11-09, reviewer. Verdict: **accepted** (round 3). Rework r4 (user's hand test: no Windows key): gate PASS again (Build 0 warnings, Selftest, Seam, Tokens, Format, Scope, Size). The diff is 24 lines in `HotkeyRecorder.cs` and its selftests; I read it: the mapper returns null for Meta combos, Win is no longer a modifier, a Win press shows the hint and leaves the caps unchanged. Within the user's request; no second Codex review run for a change this small (round 3's review covered the rest). Parser and stored format unchanged.
- **Gate (online, final round):** GATE: PASS: Build 0 warnings, Selftest PASS (17 contract rows, 16 pass, 1 manual), Seam, Tokens, Format, Scope, Size all PASS.
- **Codex review** (`.coord/reports/A10-hotkey-recorder.review.md`): no proven findings, 12-check reflection probe PASS, `VERDICT: accept`. Unproven: real focus transfers, window deactivation, Windows delivery of reserved/Win combos and real registration refusal (hand test).
- **Bounds check:**
  - Recorder states, Save/Enter/Esc/Cancel/Clear, ghost cap, unsupported-key hint, typed box removed: within plan (matches the approved lab).
  - Recorder in its own `UI/HotkeyRecorder.cs`, `SettingsWindow.cs` net smaller (`HotkeyEditor` gone, two row call sites unchanged in shape): within plan.
  - One-line Esc guard in the Settings tunnel handler (`SettingsWindow.cs:80`): justified (round 1's proven blocker; the brief had limited that file, the card's Files list allows it).
  - Extra `hotkey-recorder-escape-routing` contract row: justified (covers that fix).
  - Lab file added to Files list: justified (already committed on the branch; makes the Scope gate pass).
  - Registration, stored text format, defaults, `ISettingsHost`: unchanged (Must not change holds).
- **Rework rounds:** r1 build, r2 Esc routing, r3 Format whitespace (the offline gate does not run the online Format step).
- **Misses:** (1) Esc routing blocker, found at Codex's own lens review of round 1, cause `plan` (the brief limited `SettingsWindow.cs` edits to the editor block while Findings listed the window's Esc handler as unknown), caught by lens review, the Findings should have read the window's key handler; lesson: for key-capturing controls, Findings list every ancestor key handler. (2) Format whitespace, found at the online gate, cause `code`, caught by gate; the offline gate skipped the Format step, so Codex's "gate pass" missed it; lesson: run `dotnet format --verify-no-changes` on touched files before reporting.
- **Noticed:** none.

## Hand test
Build checked: dev copy from this worktree, round 4 (EXE and DATA under this worktree's `build/`, hotkey Ctrl+Alt+Shift+D).
- 2026-11-09 (user, first pass): everything passed except "Windows key should not be allowed at all. Ctrl, Alt, Shift, Function, thats it" (fixed in rework r4).
- 2026-11-11 (user, relayed by Master 5 · new director): "A10 i have finished hand testing, you can override and merge after A9". Recorded as the user's pass.
- Commit review (2026-11-11): 16 commits folded into two (`A10-hotkey-recorder: record shortcuts by pressing keys`; `A10-hotkey-recorder: card, reports and review`); the lab file `tools/lab/A10a-hotkey-recorder-lab.html` dropped from the branch (kept in git history of the lab commits before this tidy; not shipped).

## Hand test
Build checked: dev copy from this worktree, round 4 (EXE and DATA under this worktree's `build/`, hotkey Ctrl+Alt+Shift+D).
- 2026-11-09 (user, first pass): everything passed except "Windows key should not be allowed at all. Ctrl, Alt, Shift, Function, thats it" (fixed in rework r4).
- 2026-11-11 (user, relayed by Master 5 · new director): "A10 i have finished hand testing, you can override and merge after A9". Recorded as the user's pass.
- Commit review (2026-11-11): 16 commits folded into two (`A10-hotkey-recorder: record shortcuts by pressing keys`; `A10-hotkey-recorder: card, reports and review`); the lab file `tools/lab/A10a-hotkey-recorder-lab.html` dropped from the branch (kept in git history of the lab commits before this tidy; not shipped).
