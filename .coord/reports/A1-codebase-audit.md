# A1-codebase-audit

Date: 2026-10-23  
Branch: `feat/A1-codebase-audit`  
Commit reviewed: `ce7588e`  
Status: source audit complete; no fixes, build, selftest execution, app actions or git mutations.

Deliverable: [A1-codebase-audit](../../docs/audit/A1-codebase-audit.md).

Covered all **78 tracked files** from `.coord/scratch/A1/files.txt`, preserving the supplied order. Three `gpt-6-luna xhigh` workers read Editor, UI, and the remaining product folders/root files; the Editor worker also independently read every tracked tool script and ignore/attribute file. Director reviewed guidance, tooling, coordination/history and key cross-folder paths, verified findings and merged the report.

Results: **27 findings — 7 High, 12 Medium, 8 Low** (manager later moved A1-7 to Medium: 6 High, 13 Medium, 8 Low). Highest priorities are destructive orphan cleanup during recovery/crash windows, Quit ignoring failed saves, conditional preview-timer retention, unchecked list/timer input, calculator recursion and stale fence-cache growth. The progress-swipe Settings retention path remains explicitly suspected. Mixed-currency arithmetic is separately identified. No fixes are authorized or performed.

Validation:
- Checklist: exactly 78 rows; same order as files.txt and same paths as git ls-files.
- Findings: 27 unique sequential IDs; every checklist finding exists in Findings and every finding appears in the checklist.
- Supplied manager build/analyzer/package/selftest results were read and quoted, never rerun.
- Existing `build/a1-check` read only: Linux/macOS native assets confirmed; no actual WASM/browser payload found; bundled font licence confirmed.
- Both binary assets have source/runtime references; Settings explicitly uses the bundled Fluent font, while chrome uses Segoe Fluent Icons.
- Repo-wide reference checks found no unused whole type or Settings property; zero-consumer helpers/properties are listed in the audit.
- git diff --check passed. No branch switch, merge, commit, build, app launch/quit/kill, registry or installer execution.

Write boundary:
- This task wrote only `docs/audit/A1-codebase-audit.md` and `.coord/reports/A1-codebase-audit.md`.
- At entry, `.coord/STATUS.md` and `.claude/atlas/4f4ae1d6-e998-4eb9-a1be-e100a230655b.json` were already modified, and `.coord/tasks/A1-codebase-audit.md` was untracked. These were preserved.
- Read-only SHA-256 comparisons of all 78 tracked files plus the preexisting audit brief found only that same Atlas session JSON changed again during the run. Neither director nor workers wrote it. All other 78 baseline files matched. Existing unrelated changes were not reverted; the audit does not claim the whole tree stayed byte-identical while concurrent session recording was active.
- `.coord/STATUS.md` was deliberately not edited: the explicit two-file write scope supersedes its general dispatch-update rule.

Not covered / why:
- No live GUI, hotkey, physical input, startup races, installer/uninstaller, actual-data migration, runtime heap/GC root inspection, memory/FPS/latency profiling or failure injection: explicitly prohibited for this audit.
- No fresh dependency/security/version lookup or analyzer rerun: the supplied measured evidence is authoritative input for this task.
- No library-source audit or proof of Avalonia/AvaloniaEdit native/RAF/FoldingManager retention; local app call sites and available package/output metadata were inspected. Pending framework callbacks and renderer memory need the manager's separate measurement.
- Binaries were checked for format/metadata, usage and licence references; font glyph fallback, rendered icons and visual DPI quality were not exercised.
- Untracked/ignored files and git history/tags were not audited as product source. Supplied scratch evidence and existing Release output were read as additional inputs. Historical tracked briefs/reports were reviewed as history; retired product paths intentionally preserved by the N1 brief were not falsely labeled current product debt.
- Reference searches are lexical, not compiler/linker reachability analysis; serializer/framework callbacks were considered separately.

Open questions / manager follow-up:
1. Which findings should become follow-up tasks? No source cleanup should start until the user selects them.
2. Does the measured retained Settings instance correlate with a started preview countdown, closing during progress-wheel payout, or a different framework/native root? The audit identifies mechanisms but does not attribute the observed ~200 MB to one.
3. What recovery/orphan retention policy and save-failure Quit UX should be adopted before implementing the storage fixes?
4. Which Windows architectures should published output support before RID-specific packaging?
5. Should generated Atlas session state be untracked with a narrow ignore rule? Should old task/report history get an explicit completed index rather than rewriting it?

No review rejections or permission requests were encountered.

