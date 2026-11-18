# D3-release-checklist: a dev-to-main release checklist

Item card (`docs/loop.md`). Claude fills Request to Approve before any build; Codex reads the whole card, follows "For Codex", never edits it. Budget: 4 KB (logs go in reports).

Tier: Low (docs only)
State: plan ready
Branch: `docs/D3-release-checklist` in `.claude/worktrees/docs/D3-release-checklist` (created at Approve, from dev's tip)

## Request
Director (Master 6), 2026-11-18, user's go to start Findings (pending: `idea: release-checklist`, tidy list 2026-11-06; tag names were settled by loop v3).
- **Goal:** one short checklist for moving dev to main (fast-forward, tag, release notes, installer, what to re-check) that the director follows on the user's go.
- **Done check:** (1) checklist exists in one place under `docs/`, linked from `docs/loop.md` or `docs/dev-loop.md`; (2) every step matches a real tool or rule that exists today; (3) no step duplicates what `docs/loop.md` already says (point to it instead).
- **Proof:** read by the user; a dry-run of the steps' commands against the repo (read-only) in the report.
- **What we know:** main is releases only, fast-forwarded on the user's go and tagged semver (AGENTS.md); tags `v0.7.0-rc1` to `rc3`, `v0.6.0-rc1` to `rc9`; install via `tools/install.ps1` (only on request); release marker rows are not in the plan any more.
- **Stale check (user, 2026-11-18):** ideas age fast. Findings first checks what already covers releases (`docs/loop.md`, `docs/dev-loop.md`, merge-task skill, `wiki/`), and whether the last releases followed a real routine we can write down. Weigh pros and cons and recommend write it, fold two lines into an existing doc, or drop. Stop at the Plan for the user.
- **Constraints:** docs only; no tool changes; no real release steps run; US spelling; keep it under one page.

## Findings
- **Kind and why:** process (docs). Pending since 2026-11-06; three releases in, a routine exists but lives only in STATUS prose and reflogs.
- **Now:** `loop.md` and `branches.md` say only "main fast-forwards to dev on the user's go, plain semver tag". `dev-loop.md` covers install. No file lists the steps, release notes were never written, no wiki page covers it. Real routine (tags, reflog, STATUS archive): user says go, then a Low version-bump item (`A7-version-rc2`, `A12-version-rc3`: `AppInfo.props` label, gate + Release build + selftest) merges to dev, then `git branch -f main dev` (rc1: ff merge), tag, push main and tag, and the user runs `tools/install.ps1` themselves.
- **Drift:** tags rc1, rc2 lightweight, rc3 annotated; no step says who pushes or checks `origin/main`.
- **Pros of a checklist:** closes those gaps; ~8 lines. **Cons:** 3 releases, routine tiny and stable; the deferred "professional release loop" and `L10-lv5-ci` (CI on tags) will rewrite it; a new file adds a doc to keep in sync.
- **Recommendation: fold, not a new doc.** Add a "Release" section (about 8 lines) to `docs/dev-loop.md`, next to install, one pointer line in `loop.md`. Drop the separate-doc idea.
- **Source:** own scout (Low).

## Plan
- **After:** `dev-loop.md` Release section: go from user; bump item; gate + Release build + selftest; main ff-only to dev; annotated tag; push; installer on request only; what to re-check (version in About, `origin/main` = tag). Tags annotated from now.
- **Must not change:** tools, loop rules, past tags. **Time:** under an hour.
- **Scope questions:** none. **Test plan:** None (not visible); read-only dry run of each command in the report.
- **Gates:** `tools/gate.ps1 -Task D3-release-checklist` and `tools/loop-lint.ps1` (hot-file budgets).
- **Needs from you:** Approve, or choose "new file" or "drop".

## Files
- `docs/dev-loop.md`
- `docs/loop.md`

## Approve
User, 2026-11-18: "Approved the fold."

## Check
- Built by the item chat (docs-only, per `docs/loop.md` "Who builds"): `docs/dev-loop.md` "Release" (8 steps), one pointer in `docs/loop.md`. Tier Low, no hand test (not visible).
- Dry run (read-only): `git merge-base --is-ancestor main dev` ok; `git ls-remote origin main` = local main (01b1b06); tag, push and `branch -f` not run. Every command and path cited exists (`AppInfo.props`, `gate.ps1`, `install.ps1`).
- Gate PASS (scope, loop-lint). Existing WARN: `loop.md` was 8198 B (budget 8192), now within 10 B of before.

## Hand test
Docs only, no UI check. User read the Release section, 2026-11-18: "Looks good"
