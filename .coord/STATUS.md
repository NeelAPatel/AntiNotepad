# STATUS

Updated: 2026-10-29
Product: AntiNotepad (`product/AntiNotepad`), version label 0.6.0 (untested).

## Working rules right now
- Codex is back (user, 2026-10-25): director Sol medium delegating to Luna xhigh; Claude briefs, reviews, commits.
- Computer Use is off until the user's explicit go-ahead; live checks are done by the user by hand in the dev copy.
- Never run `AntiNotepad.exe --quit` without `ANTINOTEPAD_DATA`: it targets the installed copy's instance.

## Where things are
- `main`: tag `antinotepad-v0.6.0-untested-r7` (f4b8aee, 2026-10-28, user: "Yes, release r7") = r6 + A37-caret-drag-jump
  + the A39-A44 modularity reorg (one-way layering, 104 LAYERS rows, `tools/check-platform-seam.ps1`) + A43b missing-folder
  fix and recovery. Release build 0 warnings, SELFTEST PASS; tree identical to dev at release.
- r6 (2026-10-25) was dev reverted to just after A30 (A31-A33 reverted, A34 dropped: A32/A34 forced caption-window styles
  onto the frameless pad, losing corners and shadow). Windowing is still r4's.
- Installed copy: r5 (broken frame) until the user reinstalls with `tools/install.ps1` (on hold; r7 is now the latest).
- Finished briefs, reports and the A1 audit doc were removed from the tree on 2026-10-25; they live in git under tag
  `archive/coord-2026-10-25`. Locked choices stay in `docs/decisions.md`.

## Now
- C26-item-card-loop merged into dev (2d99362, 2026-10-29, user: "Yes, merge it"): every item has one item card
  (`.coord/templates/item.md`) through Request > Findings > Plan > Go > Build > Check > Hand test > Merge; Findings for
  every tier (master scout for Low), reproduction before building a bug, gates picked per item, a bounds check (requested
  vs approved vs done) at Check, and the five-line status card on every reply, reminded by a `UserPromptSubmit` hook
  (`tools/loop-card.ps1`). Gate on dev PASS (0 warnings, SELFTEST PASS, CONTRACTS 10 rows: 9 pass, 1 manual). Proposed
  follow-up (unnumbered, needs the user): `codex-run.sh` writes `usage.tsv` and logs to the worktree, so caps and logs
  are lost on worktree removal. Next: A55-split-view-clip, the first item on the new card.
- A45-dev-build-badge merged into dev (56a85cd, 2026-10-29; user hand test passed): `tools/dev.ps1` stamps the branch and
  build time into dev builds only; the top-bar chip shows ` <ID> · <time>`, expands on hover to
  `dev/<branch>@<date time>`, tooltip always full. CONTRACTS `dev-badge-build-stamp`. Gate on dev PASS (0 warnings, SELFTEST
  PASS, CONTRACTS 10 rows: 9 pass, 1 manual). Next: A55-split-view-clip.
- C25-dev-instance-handoff merged into dev (2f6c123, 2026-10-29; user hand test passed 4 of 4): cause was every dev run
  sharing the `AntiNotepad-dev` mutex while `dev.ps1` tracked only its own checkout's PID. `tools/dev.ps1` now takes a
  launcher mutex, sends the graceful dev quit via a staged helper, force-closes only a verified repo dev build if that
  fails, then builds and launches; sub-sessions open their build after review acceptance. Gate on dev PASS; Codex review
  accept; design review accept-with-followup. Proposed follow-ups (unnumbered, need the user): pin dev.ps1's copy of the
  mutex names to `SingleInstance.cs` (comment + gate check); comment the checkout-root assumption; skip the helper build
  when no dev copy runs. Next: A45-dev-build-badge.
- C24a-roadmap-local-page merged into dev (6b5483f, 2026-10-29, user: "local page is fine"): `/roadmap` opens
  `.coord/roadmap.html`, rebuilt by PostToolUse and Stop hooks and reloading itself every 3 s; no republish prompts. The
  Artifact stays, published only on request.
- A54-link-hover-click merged into dev (f32b92a, 2026-10-29; user hand test passed): cause was `PadView.OnViewMoved`
  resetting the cursor to the I-beam after AvaloniaEdit set the hand; Ctrl+click now handled in the tunnel press. User widened
  scope in r3: every URL (short, expanded on the caret line, Shorten links off) is Ctrl+clickable. CONTRACTS
  `shortened-link-opens`. Gate on dev PASS (0 warnings, SELFTEST PASS, CONTRACTS 9 rows: 8 pass, 1 manual). Next: A55-split-view-clip.
- A53-split-divider-height merged into dev (7299e8b, 2026-10-29): after hand-test rounds (full height, mirrored gap) the user
  kept the divider ending above the pager strip; merged as a guard, no visible change: CONTRACTS `split-divider-above-pager`.
  Gate on dev PASS (0 warnings, SELFTEST PASS, CONTRACTS 8 rows: 7 pass, 1 manual). Next: A54-link-hover-click.
- C24-roadmap-artifact (user, 2026-10-29): `/roadmap` widget replaced by a roadmap Artifact
  (https://claude.ai/artifact/VU8guxUCfxJTSZ1MxNaqyy) that a Claude Stop hook republishes when the Plan changes (main
  checkout only). Merged into dev (783e9ab, 2026-10-29, user asked); gate on dev PASS.
- C23-idea-numbering (user, 2026-10-29): numbers only on user confirmation; A53+ dissolved; Plan redrawn as unnumbered
  ideas with steps (design-tokens, design-unify, code-ownership, notes-folder-move-flow, ...). Next: user confirms ideas.
- A52-move-old-folder-rename-warning merged into dev (3ced024, 2026-10-29; user hand test passed 3 of 3): cause was a trailing
  separator in the source path; `Move` now returns an explicit retention outcome and Settings shows one truthful message.
  New CONTRACTS row `notes-move-renames-old-folder`. Gate on dev PASS (0 warnings, SELFTEST PASS, CONTRACTS 7 rows: 6 pass,
  1 manual, seam 0). Follow-ups are steps of idea notes-folder-move-flow.
- C22-roadmap-step-rows (user, 2026-10-29): one ID = one idea, work toward it = lettered steps under it; A52's six
  follow-ups folded into A78-notes-folder-move-flow (steps a-f); `/roadmap` indents steps. Merged 5887137.
- C20-branch-worktree-loop (user, 2026-10-29): every item runs on `feat/<ID-slug>` in `.claude/worktrees/<ID-slug>`; dev gets
  only `--no-ff` merges plus one STATUS commit each (STATUS edited live, committed at merge). Merged 6f8da54.
- C19-subsession-routing (user, 2026-10-29): every Codex run gets its own linked sub-session (Haiku for pure runs, Sonnet medium
  when reasoning is needed); master opens updates with a status card. A59-design-ownership-audit done, 7 corrections applied,
  A76-behavior-timer-names added; design-system rows now planned. Next: A52-move-old-folder-rename-warning on the user's go.
- C17-roadmap-view (user, 2026-10-28): `/roadmap` and `tools/roadmap.ps1` render the Plan section as a table (Claude hook
  keeps `.coord/roadmap.html` current); Plan rewritten so every row has an `<ID>-<slug>`, a scope and a State; new IDs
  C14-a38-audit-branch, C15-worktree-branch-cleanup, C16-archive-finished-sessions, A74-settings-thread-count;
  P1-project-reevaluation plan rows added as `proposed`. Next: user decides
  the 8 open P1 decisions.
- C13-dev-loop-kit (user, 2026-10-28): new dev loop, runbook `docs/loop.md`. Tiers, `tools/gate.ps1`, `CONTRACTS`
  selftest (6 rows, 5 pass, 1 manual), Codex `lens-review` skill + `lens_reviewer` role + `reviewer` tier, Claude
  `design-reviewer` agent + `/next-task`, `/review-task`, `/merge-task` skills, manual CI workflow `gate`. Dogfooded:
  in-run lens review accepted; fresh Codex review found a gate false PASS (no exe -> selftest skipped) -> r2 fixed it,
  plus `-Offline` (sandbox NU1900) and `-SizeGrowth`. Manager gate online PASS; false-pass probe FAIL as intended.
  Detected by a fresh Claude (agent + skills) and a fresh Codex (skill + role). Claude `design-reviewer` smoke run:
  accept-with-followup (rework rounds' scope/report paths, stale committed gate reports, contract checks run twice) ->
  manager fixed docs/skills and ignored `*.gate.md`; r3 fixed rework-round scope and made CONTRACTS the only caller of its
  four checks. Merged into dev (9bf5581, user: "Yes, merge it"), gate on dev PASS. Not yet exercised: Codex role loading
  inside a worktree, GitHub-hosted CI. Next: new Opus session resumes Priority 1 with this loop (`/next-task`).
- GitHub (user, 2026-10-28): private repo `NeelAPatel/AntiNotepad` created as origin; all branches and tags pushed,
  default branch `main`. History scanned first (no secrets, largest blob 2.9 MB font). The older WinUI repo is
  `Archived-AntiNotepad` (untouched). Public release stays with N3. Next: Priority 1 audit (can add a CI gate).
- C4-coord-archive (user, 2026-10-28): finished A35-A44 briefs, Codex reports and the A35 handoff committed, tagged
  `archive/coord-2026-10-28` and removed from the tree (kept: A42-padview-commands plan/impact-r2 for A42c/A42d, N3
  brief). Report paths in the entries below now resolve under that tag. `.gitignore` adds settings.local.json, `.vs/`,
  `*.user`. Plan 2.8 done.
- A35-bar-insets merged into dev (fad7d61, user-approved live): design tokens + insets, ink-centered glyphs, scrollable
  top gap, top-bar blur behind; one commit per feature. 
  A35 branches and worktree deleted (user). Lab history no longer kept. Installed copy not upgraded.
- A36-drop-pill-lab merged: dev-only Lab row and pill blur code removed; Solid pill ships; top-bar blur now in
  `UI/PadWindow.TopBlur.cs`. Branch and worktree deleted. Nothing in flight.
- A37-caret-drag-jump merged (user-approved live): SmoothCaret glide restarts from the drawn position, fixing the caret
  jumping back to the pause point during drag-select. Nothing in flight at that point.
- A38-mvvm-audit done (branch `feat/A38-mvvm-audit`, `docs/A38-mvvm-audit.md`, not merged): no MVVM; targeted
  refactors A39-A44 on `reorg/<ID-slug>` branches off dev.
- A39-motion-out-of-ui merged into dev (bac8a6c, user-approved live 2026-10-26): Editor no longer depends on UI
  (`Anim` -> `Motion/`, `ChromeTokens` -> `Theme/`, context menu shown via the host); IL-based `LAYERS` selftest with 8
  one-way rules. Branch kept until the reorg series ends.
- Reorg series, one at a time from the latest dev, in order: A40-settings-change-event, A41-padwindow-split,
  A42-padview-commands, A43-app-context, A44-platform-seam. Read-only investigator sessions wrote each plan on its own
  branch (`reorg/<ID-slug>`: `.coord/tasks/<ID-slug>-plan.md` + impact report); they never implement. Implementation
  starts per task on the user's go, from that plan, with the user's hand test before merge. Next: A44-platform-seam re-audit on the latest dev.
  Rule (user, 2026-10-26): after each task merges, the next task's investigator merges the latest dev into its branch and
  re-runs a read-only Codex delta audit (`<ID-slug>-impact-r2`), updating its plan with a "Recommendation on handling"
  (scope, split into sub-tasks or not, riskiest parts and hand checks). The user decides from that before implementation.
  A42 follows once A41 is done, then A43, A44.
- A41-padwindow-split re-audit done (6b91ebe on `reorg/A41-padwindow-split`): original scope dropped. User's sequence
  (2026-10-26): A41a-hotkeys -> hand test -> merge -> A41b-clipboard-policy re-audit -> implement -> hand test -> merge.
  Save scheduler, navigation core, taskbar/blur/OCR extraction skipped. A41a-hotkeys implemented on `reorg/A41a-hotkeys` (2677946: `Platform/HotkeyRegistrar`, `HOTKEYS swap` selftest, 3 LAYERS rows; gate passed, guards proven); user hand test passed 2026-10-26; merged into dev (2b85bfb, 0 warnings, SELFTEST PASS).
- A41b-clipboard-policy re-audit done (21b318b on `reorg/A41-padwindow-split`): do it now, before A42; small and optional
  (value = selftest on dedupe/format rules; privacy check stays in the window). Dispatched on `reorg/A41b-clipboard-policy`.
- A41b-clipboard-policy implemented on `reorg/A41b-clipboard-policy` (`Editor/ClipboardCapturePolicy`, `CLIPBOARD policy`
  selftest, 11 LAYERS rows); user hand test passed 2026-10-26; merged into dev (bbcaa6b, 0 warnings, SELFTEST PASS).
  A41 done.
- A42-padview-commands re-audit done (on `reorg/A42-padview-commands`): split, stop after A42b. User's sequence
  (2026-10-26): A42a-math-notes -> hand test -> merge -> A42b-note-analysis-file re-audit -> implement -> hand test -> merge
  -> stop at the gate (A42c-checkbox-indent, A42d-list-commands optional; ChipText/FoldSelection skipped). A42a dispatched
  and implemented on `reorg/A42a-math-notes` (`Editor/MathNotes`, `Editor/ListEditing`, 31 LAYERS rows, `MATH NOTES`/`LIST EDITING`
  selftests; PadView 1,190 -> 1,132; gate passed, guards proven); user hand test passed 2026-10-26; merged into dev
  (7f845be, 0 warnings, SELFTEST PASS, 57 LAYERS rows). 
- A42b-note-analysis-file (`reorg/A42b-note-analysis-file`): `NoteAnalysis` moved verbatim; `PadView.cs` 1,132 -> 864 lines; build 0 warnings/errors and `SELFTEST PASS`, all 16 new LAYERS rows 0 refs. Report: `.coord/reports/A42b-note-analysis-file.md`. Guard probes (PadView, Window, TextArea, Dispatcher fields) each failed the selftest. User hand test passed 2026-10-26 (about half-second lag in a 6,000-line note, accepted; see A49-large-note-perf in Backlog); merged into dev (6230aa9, 0 warnings, SELFTEST PASS, 73 LAYERS rows). A42 stopped at the gate:
  A42c-checkbox-indent and A42d-list-commands not started (user moved on to A43).
- A43-app-context re-audit done (3fe4004 on `reorg/A43-app-context`): smaller scope, no broad AppRuntime. User's sequence
  (2026-10-26): A43a-runtime-log-and-launch -> hand test -> merge -> A43b-notes-session re-audit -> implement -> hand test
  -> merge. A43a-runtime-log-and-launch implemented on `reorg/A43a-runtime-log-and-launch` (8c60766: `RuntimeLog`, `TaskbarTrace`, `AppLaunch`; `Program.` tokens 75 -> 21; 80 LAYERS lines, `RUNTIME LOG`/`TASKBAR TRACE` selftests; gate passed, guards proven); user hand test passed
  2026-10-26; merged into dev (5245a62, 0 warnings, SELFTEST PASS). 
- A43b-notes-session re-audit done (5787060 on `reorg/A43-app-context`): one task, two review stages (services injection,
  then NotesSession); bootstrap wiring stays in Program; doc fix bundled. Dispatched on `reorg/A43b-notes-session`. Doc fix pending: `docs/dev-loop.md:11` (custom
  ANTINOTEPAD_DATA follows its own location.json after an in-app move).
- A43b-notes-session implemented on `reorg/A43b-notes-session` (6993485: `NotesSession`, `INotesLocation`, Store/SettingsService injected; `Program.` tokens 21 -> 0; 89 LAYERS lines, `NOTES SESSION move`, static-exposure and DATA/DEV selftests; gate passed, guards proven). Hand test by Claude via GUI passed (second move, edit saved in new folder, nested refusal, restart reload). Not merged. Report: `.coord/reports/A43b-notes-session.md`.
- A43b-notes-session approved by the user 2026-10-27 ("Approve") after live checks; parent session merges into dev. A43b-notes-session-r2 completed on `reorg/A43b-notes-session`: unavailable stores retain a valid in-memory selection; `UNAVAILABLE STORE startup` passes. Build 0 warnings/errors and SELFTEST PASS; check-output removal was refused by tool policy. Report: `.coord/reports/A43b-notes-session-r2.md`.
- A43b-notes-session-r3 implemented on `reorg/A43b-notes-session`: unavailable-folder recovery now writes a validated folder pointer for the next start; build 0 warnings/errors and `SELFTEST PASS`, including `NOTES SESSION use-folder`, with the 89 existing LAYERS rows unchanged. No GUI launched; manager hand check remains. Report: `.coord/reports/A43b-notes-session-r3.md`.
- A43b-notes-session merged into dev 2026-10-27 (ffca0fb; includes r2 crash fix 79d2f7b and r3 recovery a44a636, both
  user-checked live): 0 warnings, SELFTEST PASS (`NOTES SESSION move/use-folder`, `UNAVAILABLE STORE startup`), `Program.`
  tokens outside Program.cs 0. A43 done. Reorg A39-A43b all merged; only A44-platform-seam (plan only) and
  `feat/A38-mvvm-audit` (audit doc) hold commits not in dev.
- A44-platform-seam re-audit done (6c7f5e6 on `reorg/A44-platform-seam`): split A44a-native-facade -> A44b-interop-guards
  -> A44c-shell-folder (optional), facade `DesktopPlatform`, dev metrics move skipped. A44a-native-facade implemented on
  `reorg/A44a-native-facade` (01c01c6): build 0 warnings/errors, `SELFTEST PASS` with 89 LAYERS lines; compile and layer
  guard probes passed. Report: `.coord/reports/A44a-native-facade.md`. Gated by the manager (own build 0 warnings, SELFTEST PASS, `Native.` outside Platform 0). User hand test 2026-10-28: 9 of 10 passed; #10 link click failed but is not from A44a (only the call
  name changed; short URLs are never clickable) -> A54-link-hover-click. Merged into dev (67b5031): 0 warnings, SELFTEST
  PASS, 89 LAYERS lines + `PLATFORM DesktopPlatform surface`, `Native.` outside Platform 0. A44b-interop-guards dispatched
  2026-10-28 (user: "Start A44b") on `reorg/A44b-interop-guards` (Sonnet manager, Codex luna xhigh, no computer use).
- A44b-interop-guards committed on `reorg/A44b-interop-guards` (98d91f8), user signed off 2026-10-28 (computer-use hand test passed; Explorer restart and tray right-click menu not exercised): GlobalHotkey/TaskbarProxy externs
  nested in private `Win32`; LAYERS +13 lines (102 total: P/Invoke-holder, raw-type, pinvoke-outside-Platform) + `PLATFORM P/Invoke holders`;
  `tools/check-platform-seam.ps1` (manager gate, 0 violations in 54 files). Manager gate: 0 warnings, SELFTEST PASS, script fails
  on a temp UI IntPtr in an interpolation hole. Report: `.coord/reports/A44b-interop-guards.md`. Merged into dev (45f7ebe): 0 warnings, SELFTEST PASS,
  102 LAYERS lines, `tools/check-platform-seam.ps1` 0 violations. Not yet exercised by hand: Explorer restart, tray
  right-click menu. A44c-shell-folder dispatched 2026-10-28 (user: "okay do a44c") on
  `reorg/A44c-shell-folder` (Sonnet manager, Codex luna xhigh, no computer use).
  A44c-shell-folder implemented (a0af213): `DesktopPlatform.OpenFolder`, SettingsWindow keeps Exists/catch/toasts; LAYERS +2 (`UI/Editor->Process.Start`, 104 total), seam script also bans `Process.Start`/`ProcessStartInfo`; guard proven with a temp probe. Manager gate: 0 warnings, SELFTEST PASS, script 0 violations. User hand test passed 2026-10-28 (Open notes/recovered folder, spaces, missing-folder toast, after move); merged into dev (adc9302): 0 warnings, SELFTEST PASS, 104 LAYERS lines, seam script 0 violations. A44 and the A39-A44 reorg series done. Released to main as `antinotepad-v0.6.0-untested-r7` (Plan step 0 done). Next: Plan Priority 1.
- A40-settings-change-event merged into dev (565a1e5, user-approved live 2026-10-26): SettingsService + one Changed event,
  SettingsWindow on ISettingsHost, all writers migrated; guards now 12 LAYERS lines + `SETTINGS Changed`; 0 warnings, SELFTEST PASS.
  Fix: SearchGrid/SplitSlot/Split now saved. Folding/InlineMath apply only on note bind/analysis (pre-existing; backlog).
- Native-window work (A32/A34) parked; any retry must not change the window's frame styles and
  goes lab-first. A34 (lab) was dropped: thick native border, bars broken in all three styles.

## Plan (user order, 2026-10-29)
Rendered as the roadmap Artifact (`tools/roadmap.url`; `/roadmap` opens it), republished by a Claude Stop hook on change.
Format (the script parses this section): `###` = priority; a row with only a bold first cell = block; Item is a numbered
`<ID>-<slug>` (confirmed by the user), an unnumbered `idea: <slug>` with its `step: <slug>` rows right under it, a
`Decision: ...` or `Release marker`. Rows run in the order listed; the script numbers them by position. Numbering rules:
`docs/loop.md` ("Numbering"). A53 and later from before 2026-10-29 were dissolved (user); the "was" notes below point to
old reports. State: `planned`, `needs you`, a `docs/loop.md` state (`briefing` ... `hand-tested`), `done` or `dropped`.
`/next-task` and `/merge-task` keep State current. Held until the user brings them back: "Open (needs the user)" and the
A44b-interop-guards leftover hand checks (Explorer restart, tray right-click menu). P1 background:
`.coord/tasks/P1-project-reevaluation-plan.md`, `.coord/reports/A59-design-ownership-audit.md` (old ID).

### Pending work
| Item | Scope | Tier | Effort | State |
|---|---|---|---|---|
| **Bugs** | | | | |
| A53-split-divider-height | Split-view divider stops short of the window's bottom edge; make it full height (was A58) | Low | Small | done |
| A54-link-hover-click | Every URL opens on Ctrl+click; shortened links always show a hand, other URLs show it while Ctrl is held (widened in r3, user) | Low | Small | done |
| C25-dev-instance-handoff | `tools/dev.ps1` closes whatever dev copy is running from any checkout (shared record, dev-only, never the installed copy) and launches its own; the implementation sub-session starts its dev copy when the gate passes | High | Small | done |
| A45-dev-build-badge | Dev badge names the exact build: branch glyph U+E0A0 (fallback ⎇), `dev/<branch>@<ISO build time>` stamped at build; short form expands on hover, tooltip always shows the full text; approved at its hand test (dev-only) | Low | Small | done |
| C26-item-card-loop | One item card per item (Request > Findings > Plan > Go > Build > Check > Hand test > Merge), findings every tier, reproduction before bug builds, gates per item, bounds check, status card hook | Low | Small | done |
| A55-split-view-clip | In split view, text scrolls across the divider; clip each pane at the divider (was A57) | Medium | Medium | planned |
| Release marker | dev -> main with a tag, on your go | - | - | planned |
| **Reorg leftovers** | | | | |
| A42c-checkbox-indent | Move checkbox indent logic out of `PadView` into its own Editor class, behavior-neutral | Low | Small | planned |
| A42d-list-commands | Move list commands out of `PadView` into its own Editor class, behavior-neutral | Medium | Medium | planned |
| **Design system and ownership** | | | | |
| idea: design-tokens | One source of truth for every design value (radius, fonts, colors, shadows, motion durations); the app looks and moves exactly as today | - | - | needs you |
| step: token-scan | Gate check that counts hard-coded design values per file and fails if a count rises; no product change | Low | Small | needs you |
| step: theme-tokens-core | Define the full token catalog in `Theme/` at today's values; `WithAlpha(byte)` and `Faded(opacity)` kept separate; no call sites changed | Low | Small | needs you |
| step: tokens-pad-chrome | Pad window chrome (toast, card, pill, divider) uses the tokens; looks identical | Medium | Medium | needs you |
| step: tokens-overlays | Menu, search and help overlays and `Kit` use the tokens; looks identical | Medium | Medium | needs you |
| step: tokens-editor | Editor drawing uses the tokens; shared brush/pen factories, each cache keeps its limits; looks identical | Medium | Medium | needs you |
| step: tokens-settings | Settings window, its parts and the motion preview use the tokens; looks identical | Medium | Medium | needs you |
| step: motion-tokens | One named duration per animation role at today's values; timing identical | Medium | Small | needs you |
| Release marker | dev -> main after the look-identical work, on your go | - | - | planned |
| idea: design-unify | You choose a unified look (radius scale, font sizes, shadows, motion timings) and it is applied; nothing changes without your lab approval | - | - | needs you |
| step: design-reference | Claude Design System page from today's values plus a canvas comparing options; no code | Low | Small | needs you |
| step: unify-lab | Lab build: today's look next to the proposed scales and Windows 11 timings; you pick | Medium | Medium | needs you |
| step: apply-radius | Apply the corner-radius scale you picked | Medium | Small | needs you |
| step: apply-type | Apply the font-size ramp you picked | Medium | Small | needs you |
| step: apply-shadows | Apply the shadow tiers you picked | Medium | Small | needs you |
| step: apply-motion | Apply the animation timings you picked | Medium | Small | needs you |
| idea: code-ownership | Every responsibility has one owner and no copies; behavior unchanged | - | - | needs you |
| step: store-split | Move `Note`, `Settings`, `StoreData` and the search types out of `Model/Store.cs`, verbatim | Low | Small | needs you |
| step: selftest-split | Split `SelfTest.cs` into one file per area (incl. the notes-move checks), verbatim | Low | Small | needs you |
| step: notes-path-dedupe | One same-or-nested path rule in `Platform/` replacing the copies in `NotesSession`, `NotesLocation`, `SettingsWindow` and `KeepOldCopy`; CONTRACTS row | High | Small | needs you |
| step: ui-kit-merge | One control kit: Settings' private controls merge into `Kit`; `Material()` and `Ago()` move to their owners | Medium | Medium | needs you |
| step: ownership-map | `docs/architecture.md` owner per responsibility; `NotesSession`, `RuntimeLog` into `App/`; LAYERS rows for `App/` and `Motion/` | Low | Small | needs you |
| step: behavior-timer-names | Optional: name save, debounce, toast and taskbar timers beside their owners, values unchanged | High | Small | needs you |
| **Cleanup** | | | | |
| C14-a38-audit-branch | Merge the A38 MVVM audit doc into dev or delete `feat/A38-mvvm-audit` | Low | Small | planned |
| C15-worktree-branch-cleanup | Remove leftover worktrees and finished `reorg/*` branches (local and origin) | Low | Small | planned |
| C16-archive-finished-sessions | Archive the finished A39-A44 Claude sessions | Low | Small | planned |
| C18-tag-scheme | One tag naming scheme and a dev -> main release checklist that re-checks gate rules; doc only | Low | Small | needs you |
| Release marker | dev -> main with a tag, on your go | - | - | planned |
| **Performance** | | | | |
| idea: settings-thread-count | Find why the Settings window runs about 90 threads; report, fix only if cheap (was A74) | Low | Small | needs you |
| A49-large-note-perf | Measure then reduce typing lag in a 6,000-line note; one change at a time (ideas in Backlog) | Medium | Large | planned |
| **Features and polish (lab-first where visible)** | | | | |
| idea: notes-folder-move-flow | Choosing and moving the notes folder is clear and safe; first step follows code-ownership's notes-path-dedupe | - | - | needs you |
| step: path-validator | One shared Platform check for a chosen folder: blocks invalid, same, nested, not writable, has notes; warns new, non-empty, network, low space, cloud-synced | High | Medium | needs you |
| step: editable-path-field | Editable path box plus Browse in Settings with a validation line; Migrate enabled only when valid or warned; lab mock first | Medium | Medium | needs you |
| step: move-preview | Before moving, show note count, size and what happens to the old folder | Medium | Small | needs you |
| step: move-undo | "Move back" after a move, using the kept old copy | High | Medium | needs you |
| step: picker-last-location | The folder picker opens at the last picked folder | Low | Small | needs you |
| step: single-result-message | Settings shows the move's own result message instead of rebuilding it | Low | Small | needs you |
| A51-recovered-folder-always-open | Recovered-notes folder always exists and its Open button always works | Low | Small | planned |
| A48-answers-toggle-clarity | Rename the "Answers" toggle to say what it does, and apply it live | Low | Small | planned |
| idea: native-timer-notification | Windows notification when a timer ends (was A55) | Medium | Medium | needs you |
| A47-hotkey-recorder | Set a hotkey in Settings by pressing the keys | Medium | Medium | planned |
| idea: chime-picker | Choose the timer chime: native sound plus two more, stretch (was A56) | Low | Medium | needs you |

## Open (needs the user)
- Reinstall the installed copy with r6 (`tools/install.ps1` from Windows Terminal) to get the r4 window back.
- Live check not yet done: A30 Windows restart/sign-out saving an edit typed just before.
- N3-release-and-update-plan: proposal at tag `proposal/N3-release-plan`; discussed in a separate chat ("Production readiness
  for antinotepad"): publishing, GitHub repo, license, signing, version label, installer.

## Backlog (someday)
IDs A53 and later below are dissolved (user, 2026-10-29); the Plan holds those as unnumbered ideas.
- A44a hand test (user, 2026-10-28), proposed backlog IDs (parent may renumber):
  - A54-link-hover-click (done, merged f32b92a): Ctrl+click on a link does nothing and links show no hand cursor. Only shortened URLs (over 34 chars, caret off the line) become `LinkGenerator` elements with Hand and Pressed (`Editor/Elements.cs`); short URLs are plain colored text. Not changed by A44a (its diff is only the OpenUrl name); confirm on the dev baseline.
  - (old A55, now idea native-timer-notification): a native Windows popup notification when a timer ends.
  - (old A56, now idea chime-picker) (stretch): chime picker, native sound plus two more.
  - (old A57, now A55-split-view-clip) (after current tasks): in split view, scrolling text overlaps across the divider; the divider should crop the visual of each pane.
  - (old A58, now A53-split-divider-height) (after current tasks): the split divider does not reach the bottom edge.
- Settings window runs about 90 threads (memory is stable, freed 5 of 5 cycles).
- A45-dev-build-badge (done, merged 56a85cd; user wish 2026-10-26; widened 2026-10-29 to branch glyph + `dev/<branch>@<build time>`, now in the Plan): the dev badge shows which task/build is live (e.g. "dev · A39"), so the user
  knows exactly what is being checked. Lab-first.
- A47-hotkey-recorder (user wish, 2026-10-26): binding a hotkey in Settings should record the actual key press, not typed text.
  Lab-first.
- A48-answers-toggle-clarity (user wish, 2026-10-26): the Settings toggle "Answers" (`Settings.InlineMath`) should say what it does
  (basic `=` math on any note; off = results only in math-mode notes), e.g. "Basic math in all notes", and apply live
  (today it applies only on note bind/analysis). Lab-first.
- A46-motion-timings (after A44, if needed): user finds the Settings page change and the pill appearing too fast;
  investigate Windows 11 motion timings as the reference across the app. Lab-first.
- A51-recovered-folder-always-open (user, 2026-10-27, A43b-notes-session hand test): the "Recovered notes" row shows Open only when the recovered folder has files; the recovered folder should always exist (even empty) and Open should always work. Today the row says Empty with no Open (`SettingsWindow.cs`, `recoveredCount is > 0`).
- A52-move-old-folder-rename-warning (user, 2026-10-27): moving notes logs "Notes are now in the new folder, but the old folder could not be renamed and remains at <old>: The parameter is incorrect." (seen moving `test targetA` to `B`). Notes move fine; investigate and fix the old-folder rename in `NotesLocation.Move` (looks like a path-format problem), then update the user-facing message. Reproduce on a pre-A43b build first.
- A49-large-note-perf (user observation, 2026-10-26, A42b live check): a 6,000-line note with live rendering shows about a half-second
  lag when typing; accepted as within the intended limits. CPU and GPU both rise during active scrolling and drop at idle.
  Task Manager during fast scrolling in the dev copy (user screenshot): 4.2% CPU, 2.8% GPU, 113.7 MB, 2 processes.
  Ideas, measure first (timestamps in the analysis path, a profiler pass on the 6,000-line fixture) and change one at a time:
  1. Cap or window the incremental analysis to the visible lines plus a margin, finish the rest in idle slices.
  2. Raise the 140 ms debounce adaptively with line count (for example above 2,000 lines).
  3. Throttle colorizer, folding and inline-math redraws while scrolling; skip off-screen lines in line transformers.
  4. Cache per-line brush/typeface lookups and shaped text; avoid per-redraw allocations (ChipText / FoldSelection).
  5. Cheaper full-fallback after a big paste or delete (over 1,500 lines): chunk it across frames.
  6. Check GPU cost of translucency/blur and the scroll animation while scrolling (less repainting, a cheaper blur path).
  7. Add a `--perf` selftest or stress scenario with a latency budget (keystroke to repaint on the 5,000-line fixture) so
     regressions are caught. Lab-first for anything visible.

## Keep
- Backups made from Claude shells live only in `%LOCALAPPDATA%\Packages\Claude_pzs8sxrjxfjjc\LocalCache\Local\`:
  `AntiNotepad-backup-20261024-155646` and `Antipad-backup-2026-10-23`. Don't delete.
- Real backups from upgrades: `%LOCALAPPDATA%\AntiNotepad-backup-20261024-215228`, `Programs\AntiNotepad.previous-*`.
