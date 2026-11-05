# A4b-design-dump: a selftest that pins every applied design value, so token steps prove the look did not change

Item card (`docs/loop.md`). Claude fills Request, Findings, Plan and Approve before any build; Codex reads the whole card and
follows "For Codex". Codex never edits this file; Check and Hand test are filled by the sub-session afterwards.

Tier: Low · selftest-only (rules: `docs/loop.md`)
Branch: `feat/A4b-design-dump` in `.claude/worktrees/feat/A4b-design-dump`, cut from the umbrella `feat/A4-design-tokens`
(not dev; user, 2026-11-01) and squashed back into it (created at Approve)

## Request
2026-11-01, user: "Start design-dump". Scope as confirmed on the STATUS row `step: design-dump`: "Selftest that builds the
real controls offline in both themes and prints their applied radius, fonts, colors, opacity and shadows; each later
step's output must match dev's byte for byte."

## Findings
- **Kind and why:** process (proof tool); type, shape and paint token steps must keep the look identical, and today
  nothing checks that short of eyeballing.
- **Now:** no selftest builds a window. `SelfTest/Editor.cs:57` already runs
  `AppBuilder.Configure<Application>().UsePlatformDetect().SetupWithoutStarting()` and builds a bare `TextEditor`.
  `Program.cs:20` routes `--selftest` to `SelfTest.Run()`.
- **Where:** `UI/PadWindow.cs:50` ctor takes `Store, SettingsService, RuntimeLog, isDev, softwareRendering, NotesSession`
  and builds the whole layout (`BuildLayout`, `ApplyAppearance`, `BindPanes`). Hotkeys, tray and clipboard hook are only
  installed by `InstallHooks()` (`UI/PadWindow.cs:132`), which the dump must not call. `UI/SettingsWindow.cs:56` ctor takes
  `SettingsService, ISettingsHost, softwareRendering, NotesSession` and builds every section; `Show(section)` switches.
  Overlays: `HelpOverlay(PadWindow, Palette)`, `SearchOverlay(PadWindow, Palette, Store, query, trash)`, `PadMenu.Show`.
  `Store(directory)` (`Model/Store.cs:50`) takes a folder, so a temp folder keeps real data out. `Theme/Palette.cs:29`:
  8 palettes, light and dark.
- **Risk:** constructing a `Window` creates a hidden native window. It is never shown, but the ctor also subscribes static
  events (`Completion.OcrRequested`, `PlatformSettings.ColorValuesChanged`) and timers; the dump must not leak into the
  other selftest checks or touch the user's data, hotkeys or tray.
- **Source:** master scout.

## Plan
- **After:**
  - `AntiNotepad --design-dump <file>` writes a sorted, plain-text dump. It builds, never shows, a `PadWindow` (temp
    store folder, default settings, seeded welcome slots), every Settings section, the help and search overlays and the
    pad menu, once per palette in `Palette.All`. Each control gets one line: tree path (types plus child index), then the
    locally set or styled values for FontSize, FontWeight, FontFamily, FontStyle, CornerRadius, Opacity, Background,
    Foreground, BorderBrush (color and brush opacity), BorderThickness, BoxShadow, Margin, Padding. No bounds, positions,
    screen-dependent or time-dependent values.
  - `--selftest` gains a `DESIGN dump` line: it builds the dump twice (it must be identical both times and non-empty),
    then compares its SHA-256 to a constant in `SelfTest/Design.cs`. A mismatch fails and prints both hashes plus the
    command to write the full dump for a diff.
  - Later steps (type, shape, paint) must keep this constant unchanged. Motion and timer steps need their own proof;
    the dump does not cover timing.
- **Expected time:** about 45 minutes, request to umbrella merge (A4a: three Codex rounds, 2 to 4 minutes each).
- **Must not change:** any product behavior or look; no file outside the list; no visible window, hotkey, tray icon
  or real data touched during the selftest.
- **Scope questions:** None.
- **Test plan, code:** the `DESIGN dump` selftest line (deterministic twice, hash matches). Builder proof: change one
  value locally (for example one CornerRadius), see the line fail, revert, and record that in the report.
- **Test plan, UI:** None (not visible). The hand test is: run `--selftest` in the dev build and confirm no window
  flashes up.
- **Gates:** the tier defaults (build, selftest, Format, Seam, Tokens): they catch build breaks and new hard-coded
  values. Layers: `SelfTest` may use UI and Theme, so a layer error means the dump went in the wrong folder.
- **Needs from you:** approve; then a 10-second look that `--selftest` does not flash a window.

## Files
Only these files may change (exact paths; `tools/gate.ps1` reads this list). Any other file needs a line under
"Out-of-scope files" in the report.
- `product/AntiNotepad/SelfTest/Design.cs`
- `product/AntiNotepad/SelfTest/Run.cs`
- `product/AntiNotepad/Program.cs`
- `tools/design-tokens-baseline.tsv`

## Contracts
- None changed. The new selftest line is the promise: "design dump is deterministic and matches the pinned hash".

## Approve
2026-11-02, user: "Go ahead" ... "okay continue as you see fit".

## For Codex
Role: terra (`docs/agents.md`), because it is new selftest code that drives real Avalonia windows offline and needs care,
but is small and has no product behavior. Product `product/AntiNotepad`. The branch is already
checked out: never switch, merge or commit (you cannot write to `.git`). Read `AGENTS.md`, then the files above. Line
numbers are from the current tree; re-read before editing.

Constraints:
- Build exactly the Plan's "After" for the Files above. No drive-by refactors, renames, icon/token/style edits outside it.
- Product files under `UI/`, `Editor/`, `Theme/` stay untouched. If a window cannot be built offline without a product
  change, stop and report what blocks it instead of changing product code.
- Never call `Show()`, `ShowDialog()`, `InstallHooks()` or anything that registers hotkeys, the tray or the clipboard
  hook. Close or dispose what you build. Use a temp folder for the store and delete it afterwards.
- `tools/design-tokens-baseline.tsv` changes only if your new file adds design literals the Tokens gate counts; prefer
  none (the dump reads values, it does not set them).
- Build into `build/A4b-design-dump-check` with `AVALONIA_TELEMETRY_OPTOUT=1`, run `--selftest` there, then delete that folder.
- Inside the Codex sandbox run the gate as `pwsh -NoProfile -File tools/gate.ps1 -Task A4b-design-dump -Offline`; the
  manager re-runs it online.
- Never launch or kill AntiNotepad except the `--selftest` and `--design-dump` runs from your check build. Never run `--quit`. Never edit `.coord/STATUS.md` or this card.

Done means (offline checks only: build, selftest, gate, script checks, files; never ports, servers, browser or screen):
- `--design-dump <file>` writes the dump described in "After", covering the pad, every Settings section, help, search
  and menu, for all 8 palettes; two runs give byte-identical files.
- `--selftest` prints `DESIGN dump <hash> <line count>` and passes; it fails with both hashes when one value changes
  (proved once and reverted).
- Build 0 warnings, `SELFTEST PASS`, the gate passes.

Before you report (director only): spawn the `lens_reviewer` agent on your diff (it reads
`.agents/skills/lens-review/SKILL.md`). Fix what it proves, or say why not. Workers without spawning: run the same
checklist yourself and say so.

Report: `.coord/reports/A4b-design-dump.md`, short, with exactly these sections:
1. **Summary**: two or three sentences.
2. **Change list**: one row per changed block: `file:line-range | what changed | which Plan line it serves`.
3. **Behavior changes**: every changed public method result or meaning, new or changed shared state, timing, default, or
   setting effect. Write "None" only if true; the review checks this against the diff.
4. **Out-of-scope files**: file and why, or "None".
5. **Plan deviations**: anything done differently from the Plan, and why, or "None".
6. **Contracts**: rows added or changed, or "None".
7. **Lens review**: findings and what you did with each.
8. **Checks run**: build, selftest, gate result lines, the dump line count, and the one-value-change failure proof.
9. **Hand checks for the user**: the Plan's UI checks, plus anything new, or "None (not visible)".

## Check
Rounds: r1 wrote no code (stopped at investigation); r2 (brief with decisions) built it. Verdict: accepted.
- Gate (`-Base feat/A4-design-tokens`, as the umbrella is the base): Selftest PASS, Seam PASS, Tokens PASS, Format PASS, Scope PASS (3 files), Size PASS. Build shows "FAIL" only because of 2 NU1900 warnings (nuget.org unreachable here, env); 0 code warnings, 0 errors. Against `dev` Scope lists 4 inherited umbrella files (docs/coding.md, docs/loop.md, tools/check-design-tokens.ps1, tools/gate.ps1), not edited here.
- Verified myself: `SELFTEST PASS`; `DESIGN dump 8123f5c3eadb4f76b034cd275f7a8ac8785157da9d1c7a142c9931b7d91de55b 16160`; two `--design-dump` runs byte-identical and equal to the selftest hash; diff vs umbrella is only Program.cs (+1), SelfTest/Run.cs (+2), new SelfTest/Design.cs, card and reports. No UI/Editor/Theme change; no `Show`/`InstallHooks`/`Close`; temp store folder deleted.
- One-value-change proof: in the report (pad font size +1, selftest failed with both hashes, reverted). It ran before the final gradient-stop serialization tweak; accepted as the mechanism is unchanged.
- Bounds: within plan. Justified: PadMenu is built by reflection on the private `BuildMenu` (no product edit); hidden windows and handlers stay until the process exits (decided in the r2 brief).
- Limits (noted, not blocking): high-contrast host overrides the palette; unopened expanders and template visuals are not covered.
### Misses
- r1: found at report; cause `plan`/brief gap (hidden-window cleanup and private menu builder not decided); caught by report review; Findings should name how private builders are reached; lesson: decide cleanup and reflection rules in the brief for selftest-that-builds-windows items.
- r2 gate Scope: cause `test`/tooling; round briefs need a `## Files` list with backticked paths, and gate base should be the umbrella for umbrella-based items.

## Hand test
2026-11-02, user, asked whether any window flashed up during `--selftest` (`DESIGN dump 8123f5c3...de55b 16160`, `SELFTEST PASS`): "no". Passed.
