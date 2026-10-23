# A1-codebase-audit

You are the director (gpt-6.1-sol medium); delegate the reading to gpt-6-luna xhigh subagents (for example one per folder:
`Editor/`, `UI/`, `Model/`+`Calc/`+`Theme/`, `Platform/`+root .cs, and `tools/`+`.coord/`+`docs/`+repo files), then merge.
Repo: this folder. Branch `feat/A1-codebase-audit` is checked out: work here, never switch branch, merge or commit.
App: `product/AntiNotepad` (Avalonia 12 + AvaloniaEdit 12, .NET 10, Windows 11 tray scratchpad). Read first: `AGENTS.md`,
`.coord/STATUS.md`, `docs/dev-loop.md`, `docs/agents.md`, `docs/decisions.md` (locked choices: a deliberate choice there is
not a finding unless the code contradicts it).

## Goal
A read-only audit of every tracked file. **Do not fix anything.** Fixes become follow-up tasks after the user picks findings.

## Write scope (only these two files; any other change is rejected)
- `docs/audit/A1-codebase-audit.md` (the deliverable)
- `.coord/reports/A1-codebase-audit.md` (short run report: what was covered, what was not, open questions)
Do not edit source, scripts, docs, `.coord/STATUS.md`, or anything else. Do not build, launch or kill AntiNotepad: the
measured evidence below is already collected.

## Measured evidence (inputs, already collected by the manager; quote them, do not re-run)
- `.coord/scratch/A1/build-release.txt`: `dotnet build -c Release`: 0 warnings, 0 errors.
- `.coord/scratch/A1/analyzers-dedup.txt`: Release build with `AnalysisLevel=latest-recommended`, deduplicated: 53 warnings
  (CA1859 x18, CA1305 x17, CA1310 x8, CA1822 x5, CA1869, CA1862, CA1852, CA1720, CA1001 x1 each). Treat CA1305/CA1310
  (culture) seriously where they parse or format numbers (Calc, settings, timers); triage the rest, group them, don't list
  53 findings. CA1001 (owns a disposable field) and CA1869 (JsonSerializerOptions per call) deserve a look.
- `.coord/scratch/A1/packages.txt`: `dotnet list package --outdated/--vulnerable/--deprecated --include-transitive`.
  No top-level updates: AvaloniaEdit 12.0.0 is the newest published, so the 12.1.4/12.0.0 split is a "watch" item, not an
  upgrade. The SkiaSharp/HarfBuzz "latest" versions are pinned by Avalonia: not actionable. No vulnerable or deprecated.
  Note Linux/WebAssembly native assets come in transitively for a Windows-only app (check the Release output for them).
- `.coord/scratch/A1/selftest.txt`: `--selftest` → SELFTEST PASS, exit 0.
- `.coord/scratch/A1/files.txt`: the 78 tracked files (`git ls-files`) = the checklist.

## Look for
1. **Code debt:** dead or unused code (types, methods, settings fields, env switches, dev-only paths reachable in Release),
   duplicated logic, leftover spikes or experiments (alternate menu, swipe or scroll modes kept "for comparison", Icon Lab
   remnants, debug cycles such as `ANTINOTEPAD_DEBUG_SETTINGS_CYCLES`, `MotionPreview`, `Showcase`), TODO/HACK comments,
   stale comments, docs that no longer match the code, magic numbers that should be named, inconsistent naming (any
   "Antipad"/"AntiQuickPad" left outside intentional migration code). For unused symbols: grep each type/public method/
   Settings property for references; list what has zero callers.
2. **Dependencies:** each PackageReference and the bundled `FluentSystemIcons-Regular.ttf`. Is each used and needed? Is
   `Avalonia.Themes.Fluent` needed given how much the app draws itself, or which Fluent pieces does it override anyway? The
   I6 brief locked `Glyph.Font` = Segoe Fluent Icons, yet the csproj bundles FluentSystemIcons: grep which font each glyph
   use actually resolves to, and whether the bundled font is used at all. Flag anything replaceable by first-party .NET or
   Avalonia (repo rule: first-party first).
3. **Inefficiencies:** allocations in render or hot paths (brushes, pens, FormattedText, geometry, LINQ or string building
   in `Render`/`Draw`/`OnRender`/colorizers/renderers/`ArrangeOverride`), timers or animation loops that run when idle or
   hidden, regexes built per call (should be static or `[GeneratedRegex]`), full-document scans on each keystroke (Structure,
   Calc, colorizers, completion), large startup work, P/Invoke misuse (signatures, SetLastError, handle leaks), blocking I/O
   on the UI thread (store saves, log writes).
   **Memory open item:** after Settings closes the log sometimes shows `freed=False` and private memory stays around 200 MB.
   You cannot run the app: by code reading only, list every reference that could keep a closed `SettingsWindow` alive
   (event subscriptions to longer-lived objects or statics, static caches, timers, closures, `Application`/palette events,
   tray, named callbacks) with file:line. The manager measures it separately.
4. **Correctness risks:** unhandled exceptions (async void, timer callbacks, Dispatcher posts), races in single-instance,
   hotkey, `--quit` and migration code, data-loss paths in the store (atomic writes? crash during save?) and in
   `tools/install.ps1`/`uninstall.ps1`/`dev.ps1`/`dev.sh`.
5. **Repo hygiene:** files that shouldn't be tracked (`.claude/atlas/*.json` are session files of a Claude plugin: say if they
   belong in git and whether `.claude/` should be ignored), stale `.coord/tasks` and `.coord/reports` (tasks for retired
   paths like `product/Antipad`), script duplication (`dev.sh` vs `dev.ps1`), `.gitignore`/`.gitattributes` gaps.
   Seed finding (verify and include): `AGENTS.md` says workers are `gpt-6-luna xhigh`, while `docs/agents.md` and
   `.coord/tools/codex-run.sh` say `high`.

## Evidence standard
Every finding cites `path:line` that, when opened, shows the quoted code. Quote the line(s) briefly. No finding without a
location. If unsure, say "suspected" and why. Don't pad: one well-grounded finding beats three guesses.

## Deliverable format: `docs/audit/A1-codebase-audit.md`
1. Header: date, branch, commit (`git rev-parse --short HEAD` if readable, else leave a placeholder), scope, method.
2. **Measured evidence** section: the facts above in short form (build, analyzers grouped by rule, packages, selftest).
3. **Summary table:** rows = categories (Code debt, Dependencies, Inefficiency, Correctness, Repo hygiene), columns =
   High / Medium / Low / Total.
4. **Findings**, most severe first, each:
   - id `A1-<n>-<slug>` (n sequential), category, severity (High/Medium/Low)
   - `file:line`, what it is, evidence (short quote), impact, suggested fix, effort (S/M/L), safe to auto-fix? (yes/no + why)
   Severity: High = data loss, crash, real leak, or visible slowness; Medium = waste or risk with a concrete trigger;
   Low = tidy-up.
5. **Per-file checklist:** a table with all 78 files from `files.txt` in that order: path | reviewed | finding ids or
   "nothing to report" | one-line note. Binaries (`.ttf`, `.ico`) count too (used? where? licence shipped?).

## Done means
- Both files written; nothing else in the tree changed.
- The checklist has exactly 78 rows. Every finding id in the checklist exists in Findings and vice versa.
- The run report says what was not covered and why.
